using UnityEngine;


namespace ViewFinder.Debug
{
    public class DebugBounds : MonoBehaviour
    {
        void OnDrawGizmos()
        {
            var r = GetComponent<Renderer>();
            if (r == null)
                return;
            var bounds = r.bounds;
            Gizmos.matrix = Matrix4x4.identity;
            Gizmos.color = Color.blue;
            Gizmos.DrawWireCube(bounds.center, bounds.size);
        }
    }
}