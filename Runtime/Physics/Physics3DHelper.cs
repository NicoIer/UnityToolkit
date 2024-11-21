#if UNITY_5_6_OR_NEWER
using UnityEngine;

namespace UnityToolkit
{
    public static class Physics3DHelper
    {
        /// <summary>
        ///  Check if the point is inside the box collider.
        /// </summary>
        /// <param name="collider"></param>
        /// <param name="point"></param>
        /// <returns></returns>
        public static bool Contains(this BoxCollider collider,Vector3 point)
        {
            Vector3 localPoint = collider.transform.InverseTransformPoint(point);
            Vector3 halfSize = collider.size * 0.5f;
            return Mathf.Abs(localPoint.x) <= halfSize.x && Mathf.Abs(localPoint.y) <= halfSize.y && Mathf.Abs(localPoint.z) <= halfSize.z;
        }
    }
}
#endif