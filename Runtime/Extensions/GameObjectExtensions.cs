#if UNITY_5_6_OR_NEWER
using UnityEngine;

namespace UnityToolkit
{
    public static class GameObjectExtensions
    {
        public static string FullPath(this GameObject gameObject)
        {
            string path = gameObject.name;
            Transform parent = gameObject.transform.parent;
            while (parent != null)
            {
                path = parent.name + "/" + path;
                parent = parent.parent;
            }

            return path;
        }
    }
}
#endif