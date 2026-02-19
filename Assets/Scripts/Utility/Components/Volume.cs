namespace Cozy.Builder.Utility
{
    using UnityEngine;

    public class Volume : MonoBehaviour
    {
        [SerializeField]
        private Vector3 size = Vector3.one;

        public Vector3 Size => size;

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = new Color(1f, 1f, 1f, 0.5f);
            Gizmos.DrawWireCube(transform.position, size);
        }
    }
}
