#if UNITY_EDITOR


using UnityEngine;

namespace UnityToolkit.Editor
{
    public static class GameObjectExtensions
    {
        public static bool IsPrefab(this GameObject gameObject)
        {
            return UnityEditor.PrefabUtility.IsPartOfPrefabAsset(gameObject);
        }
    }
}
#endif