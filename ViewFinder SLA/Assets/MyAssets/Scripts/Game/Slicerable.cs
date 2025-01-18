using UnityEngine;
using UnityEditor;
using System.Linq;


namespace ViewFinder.Gameplay
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(Renderer))]
    [RequireComponent(typeof(MeshFilter))]
    public class Slicerable : MonoBehaviour
    {
        static Material DefaultMaterial;
        static readonly string ShaderString = "Universal Render Pipeline/Unlit";


        [Tooltip("The material asigned to the new triangles created by the planes intersections")]
        [SerializeField] Material CuttingMaterial = null;
        [field: SerializeField] public bool IsCopy { get; private set; } = false;


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
            var mats = render.materials.ToList();

            mats.Add(CuttingMaterial);
            render.materials = mats.ToArray();
            var mesh = GetComponent<MeshFilter>().mesh;
            mesh.subMeshCount++;
        }
    



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
        }
    }
}