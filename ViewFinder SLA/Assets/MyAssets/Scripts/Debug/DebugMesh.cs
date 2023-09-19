using UnityEngine;


namespace ViewFinder.Debug
{
    [RequireComponent(typeof(MeshFilter))]
    public class DebugMesh : MonoBehaviour
    {
        Mesh mesh;
        Vector3[] verts;
        Vector3[] norms;

        public bool showVerts = true;
        public bool showNormals = true;
        public bool showTrigs = true;

        void Start()
        {
            mesh = GetComponent<MeshFilter>().mesh;
        }
        void FixedUpdate()
        {
            verts = mesh.vertices;
            norms = mesh.normals;
        }
        void OnDrawGizmosSelected()
        {
            if (verts == null)
                return;

            for (int i = 0; i < verts.Length; i++)
            {
                var vertex = verts[i];
                var normal = norms[i];
                var transformedVertex = transform.TransformPoint(vertex);

                if (showVerts)
                {
                    Gizmos.color = Color.red;
                    Gizmos.DrawSphere(transformedVertex, 0.05f);
                }

                if (showNormals)
                {
                    Gizmos.color = Color.blue;
                    var transformedNormal = transform.TransformVector(normal);
                    Gizmos.DrawLine(transformedVertex, transformedVertex + transformedNormal * 0.05f);
                }
            }

            if (showTrigs)
            {
                Gizmos.color = Color.yellow;
                for (var i = 0; i < verts.Length; i += 3)
                {
                    var v1 = verts[mesh.triangles[i]];
                    var v2 = verts[mesh.triangles[i + 1]];
                    var v3 = verts[mesh.triangles[i + 2]];

                    v1 = transform.TransformPoint(v1);
                    v2 = transform.TransformPoint(v2);
                    v3 = transform.TransformPoint(v3);

                    Gizmos.DrawLine(v1, v2);
                    Gizmos.DrawLine(v2, v3);
                    Gizmos.DrawLine(v3, v1);
                }
            }
        }
    }
}