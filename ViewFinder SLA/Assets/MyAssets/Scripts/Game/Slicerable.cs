using UnityEngine;
using UnityEditor;

<<<<<<< HEAD

namespace ViewFinder.Gameplay
{
    [DisallowMultipleComponent]
=======
namespace ViewFinder.Gameplay
{
>>>>>>> 2447bc270d4931fc51d76d0d95e79f77c935282a
    [RequireComponent(typeof(Renderer))]
    [RequireComponent(typeof(MeshFilter))]
    public class Slicerable : MonoBehaviour
    {
        static Material DefaultMaterial;
<<<<<<< HEAD
        static readonly string ShaderString = "Universal Render Pipeline/Unlit";
=======
        static readonly string ShaderString = "Universal Render Pipeline/Lit";
>>>>>>> 2447bc270d4931fc51d76d0d95e79f77c935282a


        [Tooltip("The material asigned to the new triangles created by the planes intersections")]
        [SerializeField] Material CuttingMaterial = null;
<<<<<<< HEAD
        public bool IsCopy { get; private set; } = false;


        void Start()
        {
            if (!CuttingMaterial || !CuttingMaterial.shader)
                CuttingMaterial = DefaultMaterial;
        }
        public void SetAsCopy()
        {
            if (IsCopy)
                return;
            IsCopy = true;
            var render = GetComponent<Renderer>();

            render.materials = new Material[] {
                render.material,
                CuttingMaterial
            };
        }
    
=======
        public bool isCopy { get; private set; } = false;
>>>>>>> 2447bc270d4931fc51d76d0d95e79f77c935282a



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
<<<<<<< HEAD
    
        private void OnValidate()
        {
            if (HasParentComponent())
            {
                UnityEngine.Debug.LogWarning($"{nameof(Slicerable)} cannot be added because a parent already has it.", gameObject);
                DestroyImmediate(this);
            }
        }

        private bool HasParentComponent()
        {
            Transform parent = transform.parent;
            while (parent != null)
            {
                if (parent.GetComponent<Slicerable>() != null)
                    return true;
                
                parent = parent.parent;
            }
            return false;
=======
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
>>>>>>> 2447bc270d4931fc51d76d0d95e79f77c935282a
        }
    }
}