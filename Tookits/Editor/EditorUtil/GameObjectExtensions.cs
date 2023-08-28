using UnityEngine;

namespace Nico.Editor
{
    public static class GameObjectExtensions
    {
        public static bool IsPrefab(this GameObject gameObject)
        {
            return UnityEditor.PrefabUtility.IsPartOfPrefabAsset(gameObject);
        }
    }
}