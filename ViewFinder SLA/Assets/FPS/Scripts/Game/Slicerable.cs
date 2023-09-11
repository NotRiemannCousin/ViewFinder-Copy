using UnityEngine;

namespace Unity.FPS.Game
{
    [RequireComponent(typeof(Renderer))]
    [RequireComponent(typeof(MeshFilter))]
    public class Slicerable : MonoBehaviour
    {
        public static Material DefaultMaterial;

        [Header("Cut Info")]
        [Tooltip("The material asigned to the triangles in the cutted planes")]
        // get material of the path 
        public Material CuttingMaterial;
        public bool isCopy { get; private set; }
        Renderer Render { get; set; }

        void Awake()
        {
            if (DefaultMaterial is not null)
                return;
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
            Render = GetComponent<Renderer>();
            if (isCopy)
                return;
            isCopy = true;
            Render.materials = new Material[] {
                Render.material,
                CuttingMaterial
                };
        }
    }
}