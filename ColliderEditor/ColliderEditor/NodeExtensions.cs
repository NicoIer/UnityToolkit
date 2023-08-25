#if UNITY_2021_1_OR_NEWER
using ConcaveHull;
using UnityEngine;

namespace ColliderEditor
{
    public static class NodeExtensions
    {
        public static Vector2 ToVector2(this Node node)
        {
            return new Vector2((float)node.x, (float)node.y);
        }
    }
}
#endif
