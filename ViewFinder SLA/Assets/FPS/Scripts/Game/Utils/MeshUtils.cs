using System.Collections.Generic;
using Unity.FPS.Game;
using UnityEngine;
using System.Linq;
using System;

public class MeshUtils
{
    private static void MeshCut(Plane plane, Mesh mesh, Transform Pos)
    {
        Debug.Log("Cutting mesh: " + mesh.name);
        // var plane = new Plane(
        //     Pos.InverseTransformPoint(p.ClosestPointOnPlane(Vector3.zero)),
        //     Pos.InverseTransformDirection(p.normal)
        // );
        #region initialize variables
        mesh.subMeshCount = 2;

        var meshVerts = new List<Vector3>();
        var meshNormals = new List<Vector3>();
        var meshUVs = new List<Vector2>();
        var meshLightmapUVs = new List<Vector2>();
        var submesh1Triangles = new List<int>();
        var submesh2Triangles = new List<int>();

        mesh.GetVertices(meshVerts);
        mesh.GetNormals(meshNormals);
        mesh.GetUVs(0, meshUVs);
        mesh.GetUVs(1, meshLightmapUVs);
        submesh1Triangles = mesh.GetTriangles(0).ToList();
        submesh2Triangles = mesh.GetTriangles(1).ToList();

        // submesh 2 is where the cutted triangles are saved
        bool isInSubMesh2 = false;

        var newEdgesPoints1 = new List<Vector3>();
        var newEdgesPoints2 = new List<Vector3>();

        var newVerts = new List<Vector3>(meshVerts.Count);
        var newTrigsSubMesh1 = new List<int>(submesh1Triangles.Count);
        var newNormals = new List<Vector3>(meshNormals.Count);
        var newUVs = new List<Vector2>(meshUVs.Count);
        var newLightmapUVs = new List<Vector2>(meshLightmapUVs.Count);
        var newTrigsSubMesh2 = new List<int>(submesh2Triangles.Count);
        #endregion

        for (int i = 0; i < meshVerts.Count; i++)
        {
            meshVerts[i] = Pos.TransformPoint(meshVerts[i]);
        }


        #region return ealier
        if (meshVerts.All(x => !isInPositiveSide(x)))
        {
            mesh.Clear();
            return;
        }
        if (meshVerts.All(x => isInPositiveSide(x)))
            return;
        #endregion

        #region cut mesh
        bool[] alreadyAdded = new bool[meshVerts.Count];

        int[] vertsInside;
        int[] vertsOutside;
        int countVertsInside;
        List<int> verts;
        for (int i = 0; i < submesh1Triangles.Count; i += 3)
        {
            verts = new List<int> {
                    submesh1Triangles[i],
                    submesh1Triangles[i + 1],
                    submesh1Triangles[i + 2]
                };

            vertsInside = verts.Where(x => isInPositiveSide(meshVerts[x])).ToArray();
            vertsOutside = verts.Where(x => !isInPositiveSide(meshVerts[x])).ToArray();
            countVertsInside = vertsInside.Count();

            switch (countVertsInside)
            {
                case 3:
                    CheckAllIn(verts[0], verts[1], verts[2]);
                    break;
                case 2:
                    if (verts.IndexOf(vertsOutside[0]) == 1)
                        Check2In1Out(vertsInside[0], vertsInside[1], vertsOutside[0]);
                    else
                        Check2In1Out(vertsInside[1], vertsInside[0], vertsOutside[0]);
                    break;
                case 1:
                    if (verts.IndexOf(vertsInside[0]) == 1)
                        Check1In2Out(vertsInside[0], vertsOutside[1], vertsOutside[0]);
                    else
                        Check1In2Out(vertsInside[0], vertsOutside[0], vertsOutside[1]);
                    break;
            }
        }
        // isInSubMesh2 = true;
        for (int i = 0; i < submesh2Triangles.Count; i += 3)
        {
            verts = new List<int> {
                    submesh2Triangles[i],
                    submesh2Triangles[i + 1],
                    submesh2Triangles[i + 2]
                };

            vertsInside = verts.Where(x => isInPositiveSide(meshVerts[x])).ToArray();
            vertsOutside = verts.Where(x => !isInPositiveSide(meshVerts[x])).ToArray();
            countVertsInside = vertsInside.Count();

            switch (countVertsInside)
            {
                case 3:
                    CheckAllIn(verts[0], verts[1], verts[2]);
                    break;
                case 2:
                    if (verts.IndexOf(vertsOutside[0]) == 1)
                        Check2In1Out(vertsInside[0], vertsInside[1], vertsOutside[0]);
                    else
                        Check2In1Out(vertsInside[1], vertsInside[0], vertsOutside[0]);
                    break;
                case 1:
                    if (verts.IndexOf(vertsInside[0]) == 1)
                        Check1In2Out(vertsInside[0], vertsOutside[1], vertsOutside[0]);
                    else
                        Check1In2Out(vertsInside[0], vertsOutside[0], vertsOutside[1]);
                    break;
            }
        }
        #endregion

        #region triangulate new edges
        var planeNormal = plane.flipped.normal;
        var newEdgesPolygonsCycles = new List<List<Vector3>>();

        int new_i = -1;
        while (newEdgesPoints1.Count != 0)
        {
            if (new_i == -1)
            {
                newEdgesPolygonsCycles.Add(new List<Vector3>());
                new_i = 0;
            }
            var p1 = newEdgesPoints1[new_i];
            var p2 = newEdgesPoints2[new_i];
            newEdgesPoints1.RemoveAt(new_i);
            newEdgesPoints2.RemoveAt(new_i);

            newEdgesPolygonsCycles.Last().Add(p1);
            new_i = newEdgesPoints1.IndexOf(p2);
        }

        foreach (var cycle in newEdgesPolygonsCycles)
        {
            Debug.Log("New Cycle: " + cycle.Count);
            for (int i = 1; i < cycle.Count - 2; i++)
                if (AreCollinear(cycle[i - 1], cycle[i], cycle[i + 1]))
                    cycle.RemoveAt(i);
            var indexes = cycle.Select(pos => TryAddNewVert(pos, planeNormal)).ToArray();
            var pivotIndex = indexes[0];
            for (int i = 1; i < cycle.Count - 1; i++)
            {
                // Debug.Log("CYCLE");
                newTrigsSubMesh2.AddRange(new int[] { pivotIndex, indexes[i + 1], indexes[i] });
            }
            // var trigs = cycle.Select((value, index) => new { value, index })
            // .GroupBy(x => x.index / 3)
            // .Select(group => group.Select(x => x.value).ToList())
            // .ToList();
            // foreach()
        }
        #endregion


        for (int i = 0; i < newVerts.Count; i++)
        {
            newVerts[i] = Pos.InverseTransformPoint(newVerts[i]);
        }

        #region override mesh
        mesh.triangles = new int[newTrigsSubMesh1.Count + newTrigsSubMesh2.Count];
        mesh.vertices = newVerts.ToArray();
        mesh.normals = newNormals.ToArray();
        mesh.uv = newUVs.ToArray();
        mesh.uv2 = newLightmapUVs.ToArray();
        mesh.subMeshCount = 2;

        mesh.SetTriangles(newTrigsSubMesh1, 0);
        mesh.SetTriangles(newTrigsSubMesh2, 1);
        // currentMesh.SetIndices(cuttedTriangles, MeshTopology.Triangles, 1);
        #endregion

        #region Local Functions

        bool isInPositiveSide(Vector3 v) => plane.GetSide(v);

        int TryAddOldVert(int index)
        {
            if (alreadyAdded[index])
                for (int i = 0; i < newVerts.Count; i++)
                    if (newVerts[i] == meshVerts[index] && newNormals[i] == meshNormals[index] && newUVs[i] == meshUVs[index])
                        return i;
            alreadyAdded[index] = true;
            newVerts.Add(meshVerts[index]);
            newNormals.Add(meshNormals[index]);
            newUVs.Add(meshUVs[index]);
            newLightmapUVs.Add(meshLightmapUVs[index]);
            return newVerts.Count - 1;
        }
        int TryAddNewVert(Vector3 v, Vector3 n, Vector2 u = default, Vector3 l_u = default)
        {
            // if (newVerts.Contains(v) && newNormals.Contains(n) && newUVs.Contains(u))
            // if some int i make newVerts[i] = v and newNormals[i] = n and newUVs[i] = u
            //     return newVerts.IndexOf(v);
            // for(int i = 0; i < newVerts.Count; i++)
            //     if (newVerts[i] == v && newNormals[i] == n && newUVs[i] == u)
            //         return i;  
            var index = newVerts.Count;
            newVerts.Add(v);
            newNormals.Add(n);
            newUVs.Add(u);
            newLightmapUVs.Add(l_u);

            return index;
        }

        int EdgeCut(int Vin, int Vout)
        {
            // vertice index - inside, vertice index - outside
            var VertIn = meshVerts[Vin];
            var VertOut = meshVerts[Vout];

            Vector3 pos = VertOut - VertIn;

            var ray = new Ray(VertIn, pos);

            plane.Raycast(ray, out float dist);
            var ratioIntersection = dist / pos.magnitude;

            var uvPin = meshUVs[Vin];
            var uvPout = meshUVs[Vout];

            var lmUVPin = meshLightmapUVs[Vin];
            var lmUVPout = meshLightmapUVs[Vout];

            var normPin = meshNormals[Vin];
            var normPout = meshNormals[Vout];

            var newVert = ray.GetPoint(dist);

            var uvNewP = Vector2.Lerp(uvPin, uvPout, ratioIntersection);
            var lmUVNewP = Vector2.Lerp(lmUVPin, lmUVPout, ratioIntersection);

            var newVertIndex = TryAddNewVert(newVert, normPin, uvNewP, lmUVNewP);

            return newVertIndex;
        }

        void Check2In1Out(int index1In, int index2In, int index3Out)
        {
            int newVert1Index = EdgeCut(index1In, index3Out);
            int newVert2Index = EdgeCut(index2In, index3Out);

            newEdgesPoints1.Add(newVerts[newVert1Index]);
            newEdgesPoints2.Add(newVerts[newVert2Index]);

            var oldVert1Index = TryAddOldVert(index1In);
            var oldVert2Index = TryAddOldVert(index2In);

            var newTrigs = new int[] { oldVert1Index, newVert1Index, oldVert2Index,
                                       newVert1Index, newVert2Index, oldVert2Index};
            if (isInSubMesh2)
                newTrigsSubMesh2.AddRange(newTrigs);
            else
                newTrigsSubMesh1.AddRange(newTrigs);

        }
        void Check1In2Out(int index1In, int index2Out, int index3Out)
        {
            int newVert1Index = EdgeCut(index1In, index2Out);
            int newVert2Index = EdgeCut(index1In, index3Out);

            newEdgesPoints1.Add(newVerts[newVert1Index]);
            newEdgesPoints2.Add(newVerts[newVert2Index]);

            int oldVert1Index = TryAddOldVert(index1In);

            var newTrigs = new int[] { oldVert1Index, newVert1Index, newVert2Index };

            if (isInSubMesh2)
                newTrigsSubMesh2.AddRange(newTrigs);
            else
                newTrigsSubMesh1.AddRange(newTrigs);
        }
        void CheckAllIn(int index1In, int index2In, int index3In)
        {
            var IndexNewVert1 = TryAddOldVert(index1In);
            var IndexNewVert2 = TryAddOldVert(index2In);
            var IndexNewVert3 = TryAddOldVert(index3In);

            var newTrigs = new int[] { IndexNewVert1, IndexNewVert2, IndexNewVert3 };

            if (isInSubMesh2)
                newTrigsSubMesh2.AddRange(newTrigs);
            else
                newTrigsSubMesh1.AddRange(newTrigs);
        }
        #endregion
    }

    public static IEnumerable<IEnumerable<int>> FanTriangulation(IEnumerable<int> edges)
    {
        throw new NotImplementedException();
    }

    public static bool AreCollinear(Vector3 a, Vector3 b, Vector3 c)
    {
        var ab = b - a;
        var ac = c - a;
        return Mathf.Approximately(Vector3.Distance(ab.normalized, ac.normalized), 0);
    }

    public static IEnumerable<Vector3> TransformPoints(Transform transform, IEnumerable<Vector3> points, bool useInverse = false)
    {
        foreach (var point in points)
            yield return useInverse ? transform.InverseTransformPoint(point) : transform.TransformPoint(point);
    }




    public static void CutByPlanes(Slicerable obj, IEnumerable<Plane> planes, bool cutCollider = true)
    {
        obj.TryGetComponent<MeshFilter>(out var meshFilter);
        Mesh mesh = meshFilter?.mesh;

        if (!mesh)
            return;
        var t = obj.transform;
        foreach (var plane in planes)
            MeshCut(plane, mesh, t);
        mesh.Optimize();

        if (!cutCollider)
            return;

        // mesh = CollidersToMesh(obj);

        // foreach (var plane in planes)
        //     MeshCut(plane, mesh, t);

        var meshColliders = obj.GetComponents<MeshCollider>();
        var boxColliders = obj.GetComponents<BoxCollider>();
        var sphereColliders = obj.GetComponents<SphereCollider>();
        var capsuleColliders = obj.GetComponents<CapsuleCollider>();
        var terrainColliders = obj.GetComponents<TerrainCollider>();


        meshColliders.ToList().ForEach(c => c.enabled = false);
        boxColliders.ToList().ForEach(c => c.enabled = false);
        sphereColliders.ToList().ForEach(c => c.enabled = false);
        capsuleColliders.ToList().ForEach(c => c.enabled = false);
        terrainColliders.ToList().ForEach(c => c.enabled = false);


        obj.gameObject.AddComponent<MeshCollider>().sharedMesh = mesh;
    }


    #region Coliders
    public static Mesh CollidersToMesh(GameObject gameObject)
    {

        var meshColliders = gameObject.GetComponents<MeshCollider>();
        var boxColliders = gameObject.GetComponents<BoxCollider>();
        var sphereColliders = gameObject.GetComponents<SphereCollider>();
        var capsuleColliders = gameObject.GetComponents<CapsuleCollider>();
        var terrainColliders = gameObject.GetComponents<TerrainCollider>();

        var mesh = new Mesh();
        mesh.name = "custom mesh: " + gameObject.name;
        // Debug.Log(mesh.name + " created: " + meshColliders.Count + boxColliders.Count + sphereColliders.Count + capsuleColliders.Count + terrainColliders.Count);
        var combineInstances = new List<CombineInstance>(meshColliders.Length + boxColliders.Length);// + sphereColliders.Count + capsuleColliders.Count + terrainColliders.Count);

        foreach (var m in meshColliders.Select(c => c.sharedMesh))
            combineInstances.Add(new CombineInstance() { mesh = m, transform = gameObject.transform.worldToLocalMatrix });
        foreach (var m in boxColliders.Select(c => BoxColliderToMesh(c)))
            combineInstances.Add(new CombineInstance() { mesh = m, transform = gameObject.transform.worldToLocalMatrix });
        // foreach (var m in sphereColliders.Select(c => SphereColliderToMesh(c)))
        //     combineInstances.Add(new CombineInstance() { mesh = m, transform = gameObject.transform.worldToLocalMatrix });
        // foreach (var m in capsuleColliders.Select(c => CapsuleColliderToMesh(c)))
        //     combineInstances.Add(new CombineInstance() { mesh = m, transform = gameObject.transform.worldToLocalMatrix });
        // foreach (var m in terrainColliders.Select(c => TerrainColliderToMesh(c)))
        //     combineInstances.Add(new CombineInstance() { mesh = m, transform = gameObject.transform.worldToLocalMatrix });

        // desactive colliders
        meshColliders.ToList().ForEach(c => c.enabled = false);
        boxColliders.ToList().ForEach(c => c.enabled = false);
        sphereColliders.ToList().ForEach(c => c.enabled = false);
        capsuleColliders.ToList().ForEach(c => c.enabled = false);
        terrainColliders.ToList().ForEach(c => c.enabled = false);

        mesh.CombineMeshes(combineInstances.ToArray());
        return mesh;
    }

    private static Mesh TerrainColliderToMesh(TerrainCollider c)
    {
        throw new NotImplementedException();
    }

    private static Mesh CapsuleColliderToMesh(CapsuleCollider c)
    {
        throw new NotImplementedException();
    }

    private static Mesh SphereColliderToMesh(SphereCollider c)
    {
        throw new NotImplementedException();
    }

    private static Mesh BoxColliderToMesh(BoxCollider boxCollider)
    {
        var mesh = new Mesh();
        var vertices = new List<Vector3>(8);
        var triangles = new List<int>(36);

        var center = boxCollider.center;
        var extents = boxCollider.size / 2;

        vertices.Add(new Vector3(center.x - extents.x, center.y - extents.y, center.z - extents.z));
        vertices.Add(new Vector3(center.x + extents.x, center.y - extents.y, center.z - extents.z));
        vertices.Add(new Vector3(center.x - extents.x, center.y + extents.y, center.z - extents.z));
        vertices.Add(new Vector3(center.x + extents.x, center.y + extents.y, center.z - extents.z));
        vertices.Add(new Vector3(center.x - extents.x, center.y - extents.y, center.z + extents.z));
        vertices.Add(new Vector3(center.x + extents.x, center.y - extents.y, center.z + extents.z));
        vertices.Add(new Vector3(center.x - extents.x, center.y + extents.y, center.z + extents.z));
        vertices.Add(new Vector3(center.x + extents.x, center.y + extents.y, center.z + extents.z));

        triangles.AddRange(new int[] {
            0, 1, 2, 2, 1, 3,
            4, 5, 6, 6, 5, 7,
            0, 4, 1, 1, 4, 5,
            2, 3, 6, 6, 3, 7,
            0, 2, 4, 4, 2, 6,
            1, 5, 3, 3, 5, 7
        });

        mesh.vertices = vertices.ToArray();
        mesh.triangles = triangles.ToArray();
        mesh.RecalculateNormals();

        return mesh;
    }
    #endregion
}

