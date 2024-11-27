using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Unity.Jobs;

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
        List<Slicerable> Projections;
        Plane[] planes;


        Texture BackgroundTexture;
        static Material BackgroundMaterial;
        static Mesh BackgroundMesh;


        void Awake()
        { 
            planes = new Plane[6];

            if (BackgroundMaterial == null)
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
                    new(-length,  length, distance),
                    new( length,  length, distance),
                    new( length, -length, distance),
                    new(-length, -length, distance),
                },
                triangles = new[] { 0, 1, 3, 1, 2, 3 },
                normals = new[] {
                    Vector3.up,
                    Vector3.up,
                    Vector3.up,
                    Vector3.up
                },
                uv = new[] {
                    Vector2.up,
                    Vector2.one,
                    Vector2.right,
                    Vector2.zero
                },
                uv2 = new[] {
                    Vector2.zero,
                    Vector2.zero,
                    Vector2.zero,
                    Vector2.zero
                }
            };
        }

        private IEnumerable<Slicerable> GetObjectsInFrustum(){
            GeometryUtility.CalculateFrustumPlanes(CameraObjects, planes);

            return FindObjectsOfType<Slicerable>().Where(
                p => {
                   return p.isActiveAndEnabled &&
                          p.gameObject.activeInHierarchy &&
                          p.TryGetComponent<Renderer>(out var renderer) &&
                          renderer != null &&
                          GeometryUtility.TestPlanesAABB(planes, renderer.bounds);
                });
        }

        private Slicerable DeepCopy(Slicerable original, Transform parent, bool worldPositionStays = true){
            var copy = Instantiate(
                        original,
                        parent,
                        worldPositionStays
                    );
                var renderCopy = copy.GetComponent<Renderer>();
                var renderOriginal = original.GetComponent<Renderer>();
                copy.SetAsCopy();

                renderCopy.lightmapIndex               = renderOriginal.lightmapIndex;
                renderCopy.lightmapScaleOffset         = renderOriginal.lightmapScaleOffset;
                renderCopy.realtimeLightmapIndex       = renderOriginal.realtimeLightmapIndex;
                renderCopy.realtimeLightmapScaleOffset = renderOriginal.realtimeLightmapScaleOffset;

                
                if(renderCopy is MeshRenderer){
                    var meshRenderCopy = renderCopy as MeshRenderer;
                    var meshRenderOriginal = renderOriginal as MeshRenderer;

                    meshRenderCopy.scaleInLightmap = meshRenderOriginal.scaleInLightmap;
                    meshRenderCopy.stitchLightmapSeams = meshRenderOriginal.stitchLightmapSeams;
                }

                return copy;
        }

        public void SayCheese()
        {
            Projections = GetObjectsInFrustum().ToList();

            BackgroundTexture = TextureUtils.GetScreenshot(CameraBackground);
            PictureTexture = TextureUtils.GetScreenshot(CameraObjects);

            GetComponent<Renderer>().material.mainTexture = PictureTexture;

            gameObject.SetActive(true);

            CopyObjects();

            #region background image
            var PhotoBackground = Instantiate(new GameObject("Photo Background"), PhotoOutputParent.transform);
            var rend = PhotoBackground.AddComponent<MeshRenderer>();
            
            rend.material = BackgroundMaterial;
            rend.material.mainTexture = BackgroundTexture;
            PhotoBackground.AddComponent<MeshFilter>().sharedMesh = BackgroundMesh;
            PhotoBackground.AddComponent<Slicerable>().SetAsCopy();
            #endregion

        }

        public void CopyObjects()
        {
            PhotoOutputParent = new GameObject("Photo Output");
            PhotoOutputParent.transform.SetPositionAndRotation(CameraObjects.transform.position, CameraObjects.transform.rotation);
            foreach (var original in Projections)
            {
                var copy = DeepCopy(original, PhotoOutputParent.transform);

                MeshUtils.CutByPlanes(copy, planes);

                if (copy.GetComponent<MeshFilter>().mesh.vertices.Length == 0)
                    Destroy(copy);
            }
            PhotoOutputParent.SetActive(false);
        }

        protected override void OnUse()
        {
            Projections = GetObjectsInFrustum().ToList();
            GeometryUtility.CalculateFrustumPlanes(CameraObjects, planes);

            var replace = new GameObject("Replace");

            foreach(var projection in Projections)
            {
                foreach(var plane in planes)
                {
                    var copy = DeepCopy(projection, replace.transform);
                    MeshUtils.CutByPlanes(copy, new[]{ plane.flipped });

                    if (copy.GetComponent<MeshFilter>().mesh.vertices.Length == 0)
                    Destroy(copy);
                }

                projection.gameObject.SetActive(false);
            }


            
            PhotoOutputParent.transform.SetPositionAndRotation(CameraObjects.transform.position, CameraObjects.transform.rotation);
            PhotoOutputParent.SetActive(true);
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