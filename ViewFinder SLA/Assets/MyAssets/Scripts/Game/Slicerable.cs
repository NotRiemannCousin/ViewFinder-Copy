using UnityEngine;
using UnityEditor;

namespace ViewFinder.Gameplay
{
    [RequireComponent(typeof(Renderer))]
    [RequireComponent(typeof(MeshFilter))]
    public class Slicerable : MonoBehaviour
    {
        static Material DefaultMaterial;
        static readonly string ShaderString = "Universal Render Pipeline/Lit";


        [Tooltip("The material asigned to the new triangles created by the planes intersections")]
        [SerializeField] Material CuttingMaterial = null;
        public bool isCopy { get; private set; } = false;



        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        static void Initialize()
        {
            if (DefaultMaterial) return;

            if (!Shader.Find(ShaderString))
            {
                UnityEngine.Debug.LogError($"Shader {ShaderString} not found.");
                return;
            }

            DefaultMaterial = new Material(Shader.Find(ShaderString))
            {
                color = Color.gray
            };
            DefaultMaterial.SetInt("_Smoothness", 0);
        }
        void Start()
        {
            // CuttingMaterial = null;
            // UnityEngine.Debug.Log(CuttingMaterial?.shader + " " +
            // (CuttingMaterial is null));
            // CuttingMaterial = CuttingMaterial ?? DefaultMaterial;
            // if(CuttingMaterial is null || CuttingMaterial?.shader is null)
            if (!CuttingMaterial || !CuttingMaterial.shader)
                CuttingMaterial = DefaultMaterial;
        }
        public void SetAsCopy()
        {
            if (isCopy)
                return;
            isCopy = true;
            var render = GetComponent<Renderer>();

            render.materials = new Material[] {
                render.material,
                CuttingMaterial
            };
        }
    }
}