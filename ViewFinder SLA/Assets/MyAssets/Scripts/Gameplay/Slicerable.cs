using UnityEngine;

namespace ViewFinder.Gameplay
{
    [RequireComponent(typeof(Renderer))]
    [RequireComponent(typeof(MeshFilter))]
    public class Slicerable : MonoBehaviour
    {
        public static Material DefaultMaterial;

        [Tooltip("The material asigned to the new triangles created by the planes intersection")]
        [field: SerializeField]
        Material CuttingMaterial { get; private set; }
        public bool isCopy { get; private set; }

        [RuntimeInitializeOnLoad(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void Initialize()
        {
            if (DefaultMaterial) return;

            DefaultMaterial = new Material(Shader.Find("Unlit/Texture"))
            {
                color = Color.gray
            };
            DefaultMaterial.SetInt("_Smoothness", 0);
        }
        private void Start()
        {
            CuttingMaterial = CuttingMaterial ?? DefaultMaterial;
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