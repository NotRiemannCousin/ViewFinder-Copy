using System.Collections.Generic;
using System.Linq;
using Unity.Jobs;
using UnityEngine;
using Unity.Collections;
using static UnityEngine.Mesh;

// TODO: Make this run using Jobs
// TODO: Crop the objects that are already in the scene but with reverse Planes

namespace ViewFinder.Gameplay
{
    public class Photo : ItemController
    {
        [Tooltip("Reference to a Camera that can only see the Skybox, for the photo in background.")]
        [SerializeField] Camera CameraBackground;

        [Tooltip("Reference to the Camera that will be displayed in the photo.")]
        [SerializeField] Camera CameraObjects;

        GameObject PhotoOutputParent;
        Texture PictureTexture;
        List<Slicerable> projections;
        Plane[] planes;


        Texture BackgroundTexture;
        static Material BackgroundMaterial;
        static Mesh BackgroundMesh;


        void Awake()
        { 
            planes = new Plane[6];

            if (BackgroundMaterial is null)
            {
                BackgroundMaterial = new Material(Shader.Find("Unlit/Texture"))
                {
                    mainTexture = BackgroundTexture,
                    color       = Color.white
                };
                BackgroundMaterial.SetInt("_Smoothness", 0);
            }

            if (BackgroundMesh)
                return;

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


        public void SayCheese()
        {
            GeometryUtility.CalculateFrustumPlanes(CameraObjects, planes);
            projections = new List<Slicerable>();

            Slicerable[] Projections = FindObjectsOfType<Slicerable>().Where(p => p.isActiveAndEnabled).ToArray();
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

                if (GeometryUtility.TestPlanesAABB(planes, bounds))
                    projections.Add(projection);
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

            foreach (var original in projections)
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
                MeshUtils.CutByPlanes(copy, planes);

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


// May be useful, but Im lazy to fix it


// public struct PhotoJob : IJob
// {
//     [ReadOnly]
//     public MeshDataArray readMeshes;
//     public MeshDataArray writeMeshes;
//     public NativeArray<Matrix4x4> projectionMatrices;
//     public NativeArray<Plane> planes;
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