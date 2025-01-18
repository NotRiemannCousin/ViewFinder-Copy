using System.Collections.Generic;
using ViewFinder.Gameplay;
using UnityEngine;
using System.Linq;
using System;
using UnityEngine.Rendering;

public static class MeshUtils
{
    // TODO: Make a new Method like this but with MeshData(for Jobs)
    // TODO: Instead of use transform methods on points, use transform methods the plane  
    static void MeshCut(Plane plane, Mesh mesh, Transform Pos)
    {
        #region initialize variables
        int subMeshCount = mesh.subMeshCount;
        int UVCount = 0;

        for (VertexAttribute attr = VertexAttribute.TexCoord0; attr <= VertexAttribute.TexCoord7; attr++)
            if(mesh.HasVertexAttribute(attr))
                UVCount++;

        var meshVerts      = new List<Vector3>();
        var meshNormals    = new List<Vector3>();
        var meshUVChannels = new List<List<Vector2>>();
        var meshTrigs      = new List<List<int>>();

        mesh.GetVertices(meshVerts);
        mesh.GetNormals(meshNormals);
        for(int i = 0; i < UVCount; i++){
            meshUVChannels.Add(new List<Vector2>());
            mesh.GetUVs(i, meshUVChannels[i]);
        }
        for(int i = 0; i < subMeshCount; i++){
            meshTrigs.Add(new List<int>());
            mesh.GetTriangles(meshTrigs[i], i);
        }

        var newVerts       = new List<Vector3>(meshVerts.Count);
        var newNormals     = new List<Vector3>(meshNormals.Count);

        var newUVs         = new List<List<Vector2>>(meshUVChannels.Count);
        var newTrigs       = new List<List<int>>(meshTrigs.Count);
        var newEdgesPoints = new List<(Vector3, Vector3)>();


        foreach(var channel in meshUVChannels)
            newUVs.Add(new List<Vector2>(channel.Count * 2));
        
        foreach(var trig in meshTrigs){
            newTrigs.Add(new List<int>(trig.Count * 2));
        }
        
        #endregion

        for(int i = 0; i < meshVerts.Count; i++)
            meshVerts[i] = Pos.TransformPoint(meshVerts[i]);


        #region return ealier
        if (meshVerts.All(x => !isInPositiveSide(x))){
            mesh.Clear();
            return;
        }
        if (meshVerts.All(x => isInPositiveSide(x)))
            return;
        #endregion

        #region cut mesh
        Dictionary<(Vector3 v, Vector3 n, List<Vector2> uvs), int> VertsDict = new();

        int[] vertsInside;
        int[] vertsOutside;
        int countVertsInside;
        List<int> verts;
        for(int i = 0; i < subMeshCount; i++)
            for(int j = 0; j < meshTrigs[i].Count; j += 3)
            {
                verts = new List<int> {
                        meshTrigs[i][j],
                        meshTrigs[i][j + 1],
                        meshTrigs[i][j + 2]
                    };

                vertsInside  = verts.Where(x =>  isInPositiveSide(meshVerts[x])).ToArray();
                vertsOutside = verts.Where(x => !isInPositiveSide(meshVerts[x])).ToArray();
                countVertsInside = vertsInside.Count();

                switch (countVertsInside)
                {
                    case 3:
                        CheckAllIn(verts[0], verts[1], verts[2], i);
                        break;
                    case 2:
                        if (verts.IndexOf(vertsOutside[0]) == 1)
                            Check2In1Out(vertsInside[0], vertsInside[1], vertsOutside[0], i);
                        else
                            Check2In1Out(vertsInside[1], vertsInside[0], vertsOutside[0], i);
                        break;
                    case 1:
                        if (verts.IndexOf(vertsInside[0]) == 1)
                            Check1In2Out(vertsInside[0], vertsOutside[1], vertsOutside[0], i);
                        else
                            Check1In2Out(vertsInside[0], vertsOutside[0], vertsOutside[1], i);
                        break;
                }
            }
        #endregion

        #region triangulate new edges
        // * replace with the method of Triangulation when implemented
        var planeNormal = plane.flipped.normal;
        var newEdgesPolygonsCycles = new List<List<Vector3>>();
        var UVZero = new Vector2[UVCount].ToList();

        int new_i = -1;
        while (newEdgesPoints.Count != 0)
        {
            if (new_i == -1)
            {
                newEdgesPolygonsCycles.Add(new List<Vector3>());
                new_i = 0;
            }
            var (p1, p2) = newEdgesPoints[new_i];
            newEdgesPoints.RemoveAt(new_i);

            newEdgesPolygonsCycles.Last().Add(p1);
            new_i = newEdgesPoints.FindIndex(x => x.Item1 == p2);
        }

        foreach(var cycle in newEdgesPolygonsCycles)
        {
            for(int i = 1; i < cycle.Count - 2; i++)
                if (AreCollinear(cycle[i - 1], cycle[i], cycle[i + 1]))
                    cycle.RemoveAt(i);
            var indexes = cycle.Select(pos => TryAddNewVert(pos, planeNormal, UVZero)).ToArray();
            var pivotIndex = indexes[0];
            for(int i = 1; i < cycle.Count - 1; i++)
            {
                newTrigs.Last().AddRange(new int[] { pivotIndex, indexes[i + 1], indexes[i] });
            }
            // var trigs = cycle.Select((value, index) => new { value, index })
            // .GroupBy(x => x.index / 3)
            // .Select(group => group.Select(x => x.value).ToList())
            // .ToList();
            // foreach()
        }
        #endregion


        for(int i = 0; i < newVerts.Count; i++)
        {
            newVerts[i] = Pos.InverseTransformPoint(newVerts[i]);
        }

        #region override mesh
        mesh.triangles = new int[newTrigs.Select(p => p.Count).Sum()];
        mesh.SetVertices(newVerts);
        mesh.SetNormals(newNormals);

        mesh.subMeshCount = subMeshCount;
        for(int i = 0; i < subMeshCount; i++)
            mesh.SetTriangles(newTrigs[i], i);
        for(int i = 0; i < UVCount; i++)
            mesh.SetUVs(i, newUVs[i]);

        #endregion

        #region Local Functions

        bool isInPositiveSide(Vector3 v) => plane.GetSide(v);

        // Try to add a existing Vertice to the new mesh or create a new one and return its index.
        // If the Vertice is already added (exist a Vertice with same position, normal, UV and UV2), return its index.
        int TryAddOldVert(int index)
        {
            return TryAddNewVert(meshVerts[index], meshNormals[index], meshUVChannels.Select(x => x[index]).ToList());
        }
        int TryAddNewVert(Vector3 v, Vector3 n, List<Vector2> u)
        {
            var key = (v, n, u);
            if(!VertsDict.ContainsKey(key)){
                VertsDict.Add(key, VertsDict.Count);
                newVerts.Add(v);
                newNormals.Add(n);
                for(int i = 0; i < UVCount; i++)
                    newUVs[i].Add(u[i]);
            }
            return VertsDict[key];
        }

        // Given 2 Vertices (first in positive side and second in negative side of the plane),
        // and 
        int EdgeCut(int Vin, int Vout)
        {
            // vertice index - inside, vertice index - outside
            var VertIn = meshVerts[Vin];
            var VertOut = meshVerts[Vout];

            Vector3 pos = VertOut - VertIn;

            var ray = new Ray(VertIn, pos);

            plane.Raycast(ray, out float dist);
            var ratioIntersection = dist / pos.magnitude;

            List<Vector2> uvNewPs = new();

            for(int i = 0; i < UVCount; i++){
                var uvPin  = meshUVChannels[i][Vin];
                var uvPout = meshUVChannels[i][Vout];
                uvNewPs.Add(Vector2.Lerp(uvPin, uvPout, ratioIntersection));
            }
            
            var normPin = meshNormals[Vin];
            var normPout = meshNormals[Vout];

            var newVert = ray.GetPoint(dist);


            var newVertIndex = TryAddNewVert(newVert, normPin, uvNewPs);

            return newVertIndex;
        }

        // Add the Vertices to the new mesh according the 3 possible cases.
        void Check2In1Out(int index1In, int index2In, int index3Out, int submesh)
        {
            int newVert1Index = EdgeCut(index1In, index3Out);
            int newVert2Index = EdgeCut(index2In, index3Out);

            newEdgesPoints.Add((newVerts[newVert1Index], newVerts[newVert2Index]));

            var oldVert1Index = TryAddOldVert(index1In);
            var oldVert2Index = TryAddOldVert(index2In);

            var trigsToAdd = new int[] { oldVert1Index, newVert1Index, oldVert2Index,
                                       newVert1Index, newVert2Index, oldVert2Index};
            
            newTrigs[submesh].AddRange(trigsToAdd);

        }
        void Check1In2Out(int index1In, int index2Out, int index3Out, int submesh)
        {
            int newVert1Index = EdgeCut(index1In, index2Out);
            int newVert2Index = EdgeCut(index1In, index3Out);

            newEdgesPoints.Add((newVerts[newVert1Index], newVerts[newVert2Index]));

            int oldVert1Index = TryAddOldVert(index1In);

            var trigsToAdd = new int[] { oldVert1Index, newVert1Index, newVert2Index };

            
            newTrigs[submesh].AddRange(trigsToAdd);
        }
        void CheckAllIn(int index1In, int index2In, int index3In, int submesh)
        {
            var IndexNewVert1 = TryAddOldVert(index1In);
            var IndexNewVert2 = TryAddOldVert(index2In);
            var IndexNewVert3 = TryAddOldVert(index3In);

            var trigsToAdd = new int[] { IndexNewVert1, IndexNewVert2, IndexNewVert3 };

            
            newTrigs[submesh].AddRange(trigsToAdd);
        }
        #endregion
    }

    // TODO: Implement this but with other algorithm
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
        if(useInverse)
            foreach(var point in points)
                yield return transform.InverseTransformPoint(point);
        else
            foreach(var point in points)
                yield return  transform.TransformPoint(point);
    }



    public static void CutByPlanes(Slicerable obj, IEnumerable<Plane> planes, bool cutColliders = true)
    {
        if(!obj.TryGetComponent<MeshFilter>(out var meshFilter)) return;
        Mesh mesh = meshFilter.mesh;

        if (mesh == null)
            return;


        var t = obj.transform;
        foreach(var plane in planes)
            MeshCut(plane, mesh, t);
        mesh.Optimize();

        if (!cutColliders)
            return;

        // mesh = CollidersToMesh(obj);

        // foreach(var plane in planes)
        //     MeshCut(plane, mesh, t);

        var meshColliders = obj.GetComponents<MeshCollider>();
        var boxColliders = obj.GetComponents<BoxCollider>();
        var sphereColliders = obj.GetComponents<SphereCollider>();
        var capsuleColliders = obj.GetComponents<CapsuleCollider>();
        var terrainColliders = obj.GetComponents<TerrainCollider>();


        meshColliders.ToList().ForEach(   c => c.enabled = false);
        boxColliders.ToList().ForEach(    c => c.enabled = false);
        sphereColliders.ToList().ForEach( c => c.enabled = false);
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

        foreach(var m in meshColliders.Select(c => c.sharedMesh))
            combineInstances.Add(new CombineInstance() { mesh = m, transform = gameObject.transform.worldToLocalMatrix });
        foreach(var m in boxColliders.Select(c => BoxColliderToMesh(c)))
            combineInstances.Add(new CombineInstance() { mesh = m, transform = gameObject.transform.worldToLocalMatrix });
        // foreach(var m in sphereColliders.Select(c => SphereColliderToMesh(c)))
        //     combineInstances.Add(new CombineInstance() { mesh = m, transform = gameObject.transform.worldToLocalMatrix });
        // foreach(var m in capsuleColliders.Select(c => CapsuleColliderToMesh(c)))
        //     combineInstances.Add(new CombineInstance() { mesh = m, transform = gameObject.transform.worldToLocalMatrix });
        // foreach(var m in terrainColliders.Select(c => TerrainColliderToMesh(c)))
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

    static Mesh TerrainColliderToMesh(TerrainCollider c)
    {
        throw new NotImplementedException();
    }

    static Mesh CapsuleColliderToMesh(CapsuleCollider c)
    {
        throw new NotImplementedException();
    }

    static Mesh SphereColliderToMesh(SphereCollider c)
    {
        throw new NotImplementedException();
    }

    static Mesh BoxColliderToMesh(BoxCollider boxCollider)
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