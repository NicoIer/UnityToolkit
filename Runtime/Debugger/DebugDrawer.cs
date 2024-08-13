#if UNITY_5_6_OR_NEWER
using UnityEngine;

namespace UnityToolkit
{
    public static class DebugDrawer
    {
        public static void DrawWireCube(Vector3 center, Vector3 size, Color color, float duration = 0.3f)
        {
            Vector3 first = center + new Vector3(size.x / 2, size.y / 2, size.z / 2);
            Vector3 second = center + new Vector3(size.x / 2, size.y / 2, -size.z / 2);
            Vector3 third = center + new Vector3(-size.x / 2, size.y / 2, -size.z / 2);
            Vector3 fourth = center + new Vector3(-size.x / 2, size.y / 2, size.z / 2);
            Vector3 fifth = center + new Vector3(size.x / 2, -size.y / 2, size.z / 2);
            Vector3 sixth = center + new Vector3(size.x / 2, -size.y / 2, -size.z / 2);
            Vector3 seventh = center + new Vector3(-size.x / 2, -size.y / 2, -size.z / 2);
            Vector3 eighth = center + new Vector3(-size.x / 2, -size.y / 2, size.z / 2);

            Debug.DrawLine(first, second, color, duration);
            Debug.DrawLine(second, third, color, duration);
            Debug.DrawLine(third, fourth, color, duration);
            Debug.DrawLine(fourth, first, color, duration);

            Debug.DrawLine(fifth, sixth, color, duration);
            Debug.DrawLine(sixth, seventh, color, duration);
            Debug.DrawLine(seventh, eighth, color, duration);
            Debug.DrawLine(eighth, fifth, color, duration);

            Debug.DrawLine(first, fifth, color, duration);
            Debug.DrawLine(second, sixth, color, duration);

            Debug.DrawLine(third, seventh, color, duration);
            Debug.DrawLine(fourth, eighth, color, duration);
        }
    }
}
#endif