using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

#if UNITY_EDITOR
namespace Nico.Util
{
    public static class AssetUtil
    {
        public static List<string> GetScriptableObject<T>(string folderPath)
        {
            var soList = AssetDatabase.FindAssets($"t:{nameof(ScriptableObject)}", new[] { folderPath });
            List<string> resultPath = new List<string>();
            foreach (var soGuid in soList)
            {
                var soPath = AssetDatabase.GUIDToAssetPath(soGuid);
                //判断这个资源的类型是否是 IMetaDataContainer 的实现类
                Type soType = AssetDatabase.GetMainAssetTypeAtPath(soPath);
                if (typeof(T).IsAssignableFrom(soType))
                {
                    resultPath.Add(soPath);
                    continue;
                }
            }

            return resultPath;
        }
    }
}
#endif