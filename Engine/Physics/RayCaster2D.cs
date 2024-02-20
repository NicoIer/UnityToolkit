using UnityEngine;

namespace UnityToolkit
{
    public static class RayCaster2D
    {
        public const int MaxCount = 100;
        private static Collider2D[] collider2Ds = new Collider2D[MaxCount];

        public static int OverlapCircle(Vector2 position, float radius, out Collider2D[] cols,
            LayerMask layerMask)
        {
            int count = Physics2D.OverlapCircleNonAlloc(position, radius, collider2Ds, layerMask);
            cols = collider2Ds;
            return count;
        }

        public static int OverlapBox(out Collider2D[] results, Vector3 point, Vector2 size, float facingAngle,
            LayerMask layer)
        {
            int ans = Physics2D.OverlapBoxNonAlloc(point, size, facingAngle, collider2Ds, layer);
            results = collider2Ds;
            return ans;
        }
    }
}