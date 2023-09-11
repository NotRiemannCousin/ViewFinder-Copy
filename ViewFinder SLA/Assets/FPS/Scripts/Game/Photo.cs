using System.Collections.Generic;
using System.Linq;
using Unity.Jobs;
using UnityEngine;
using Unity.Collections;
using static UnityEngine.Mesh;

namespace Unity.FPS.Game
{
    // public struct PhotoJob : IJob
    // {
    //     [ReadOnly]
    //     public MeshDataArray readMeshes;
    //     public MeshDataArray writeMeshes;
    //     public NativeArray<Matrix4x4> projectionMatrices;
    //     public NativeArray<Plane> planes { get; set; }
    //     public PhotoJob(MeshDataArray readMeshes, MeshDataArray writeMeshes, NativeArray<Matrix4x4> projectionMatrices, NativeArray<Plane> planes)
    //     {
    //         this.readMeshes = readMeshes;
    //         this.writeMeshes = writeMeshes;
    //         this.projectionMatrices = projectionMatrices;
    //         this.planes = planes;
    //     }

    //     public void Execute()
    //     {
    //         for (int i = 0; i < readMeshes.Length; i++)
    //         {
    //             var mesh = writeMeshes[i];
    //             // MeshUtils.CopyMesh(readMeshes[i], ref mesh);
    //         }
    //         // if (readMeshes.Length == 0)
    //         //     return;
    //         // Debug.Log("Printing");
    //         // MeshUtils.CutByPlanesJob(readMeshes, ref writeMeshes, planes, projectionMatrices);

    //     }
    // }

    public class Photo : ItemController
    {
        public Camera CameraBackground;
        public Camera CameraObjects;

        GameObject PhotoOutputParent { get; set; }
        Texture PictureTexture { get; set; }
        List<Slicerable> m_projections { get; set; }
        Plane[] planes { get; set; }
        JobHandle jobHandle { get; set; }


        Texture BackgroundTexture { get; set; }
        Material BackgroundMaterial { get; set; }
        Mesh BackgroundMesh { get; set; }

        public bool[] ActivePlanes = new bool[6]{false, false, false, false, false, false};

        void Awake()
        {
            planes = new Plane[6];
            BackgroundMaterial = new Material(Shader.Find("Unlit/Texture"))
            {
                mainTexture = BackgroundTexture,
                color = Color.white
            };
            BackgroundMaterial.SetInt("_Smoothness", 0);

            var distance = CameraObjects.farClipPlane / 4;
            var length = Mathf.Tan(CameraObjects.fieldOfView * Mathf.Deg2Rad / 2) * distance;

            BackgroundMesh = new Mesh
            {
                vertices = new Vector3[]
                {
                    new Vector3(-length, length, distance),
                    new Vector3(length, length, distance),
                    new Vector3(length, -length, distance),
                    new Vector3(-length, -length, distance),
                },
                triangles = new[] { 0, 1, 3, 1, 2, 3 },
                uv = new[] {
                    new Vector2(0, 1),
                    new Vector2(1, 1),
                    new Vector2(1, 0),
                    new Vector2(0, 0),
                }
            };
        }

        public void SayX()
        {
            GeometryUtility.CalculateFrustumPlanes(CameraObjects, planes);
            m_projections = new List<Slicerable>();

            Slicerable[] Projections = FindObjectsOfType<Slicerable>();
            BackgroundTexture = TextureUtils.GetScreenshot(CameraBackground);
            PictureTexture = TextureUtils.GetScreenshot(CameraObjects);

            GetComponent<Renderer>().material.mainTexture = PictureTexture;

            foreach (var projection in Projections)
            {
                if (!projection.gameObject.activeInHierarchy)
                    continue;
                projection.TryGetComponent<Renderer>(out var renderer);
                if (!renderer)
                    continue;

                var bounds = renderer.bounds;
                // add the default to top

                if (GeometryUtility.TestPlanesAABB(planes, bounds))
                    m_projections.Add(projection);
            }
            gameObject.SetActive(true);

            CopyObjects();

            #region background image
            var rend = PhotoOutputParent.AddComponent<MeshRenderer>();
            rend.material = BackgroundMaterial;
            rend.material.mainTexture = BackgroundTexture;
            PhotoOutputParent.AddComponent<MeshFilter>().sharedMesh = BackgroundMesh;
            #endregion

        }

        public void CopyObjects()
        {
            PhotoOutputParent = new GameObject("Photo Output");
            PhotoOutputParent.transform.position = CameraObjects.transform.position;
            PhotoOutputParent.transform.rotation = CameraObjects.transform.rotation;

            foreach (var original in m_projections)
            {
                var copy = Instantiate(
                        original,
                        PhotoOutputParent.transform,
                        true
                    );
                var renderCopy = copy.GetComponent<Renderer>();
                var renderOriginal = original.GetComponent<Renderer>();
                copy.SetAsCopy();

                renderCopy.lightmapIndex = renderOriginal.lightmapIndex;
                renderCopy.lightmapScaleOffset = renderOriginal.lightmapScaleOffset;
                MeshUtils.CutByPlanes(copy, planes.Where((p, i) => ActivePlanes[i]));

                if (copy.GetComponent<MeshFilter>()?.mesh.vertices.Length == 0)
                    Destroy(copy);
            }

            PhotoOutputParent.SetActive(false);
        }

        protected override void OnUse()
        {
            PhotoOutputParent.SetActive(true);
            PhotoOutputParent.transform.position = CameraObjects.transform.position;
            PhotoOutputParent.transform.rotation = CameraObjects.transform.rotation;
        }
    }
}