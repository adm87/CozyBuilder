namespace Cozy.Builder.Utility
{
    using UnityEngine;

    public class VolumeComponent : MonoBehaviour
    {
        [SerializeField]
        private Vector3 size = Vector3.one;

        public Vector3 Size => size;

        public Vector3 HalfSize => size / 2f;

        public void SetSize(Vector3 newSize)
        {
            size = newSize;
        }

        public void SetCenter(Vector3 newCenter)
        {
            transform.position = newCenter;
        }

        /// <summary>
        /// ContainmentClamp clamps the given point to be within the bounds of the volume, returning the closest point on the surface if it's outside.
        /// </summary>
        /// <param name="point">The point to clamp.</param>
        /// <returns>The clamped point within the volume.</returns>
        public Vector3 ContainmentClamp(Vector3 point)
        {
            var halfSize = HalfSize;
            var localPoint = transform.InverseTransformPoint(point);
            var clampedLocalPoint = new Vector3(
                Mathf.Clamp(localPoint.x, -halfSize.x, halfSize.x),
                Mathf.Clamp(localPoint.y, -halfSize.y, halfSize.y),
                Mathf.Clamp(localPoint.z, -halfSize.z, halfSize.z)
            );
            return transform.TransformPoint(clampedLocalPoint);
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = new Color(1f, 1f, 1f, 0.5f);
            Gizmos.matrix = Matrix4x4.TRS(transform.position, transform.rotation, Vector3.one);
            Gizmos.DrawWireCube(Vector3.zero, size);
            Gizmos.matrix = Matrix4x4.identity;
        }
    }
}
