#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEditor;


namespace Nico.Editor
{
    /// <summary>
    /// 仅在编辑器下使用的工具类
    /// </summary>
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

        public static void SaveScriptableObject<T>(T so, in string path) where T : ScriptableObject
        {
            if (so == null)
            {
                Debug.LogError("保存的资源为空");
                return;
            }

            if (string.IsNullOrEmpty(path))
            {
                Debug.LogError("保存的路径为空");
                return;
            }

            if (AssetDatabase.LoadAssetAtPath<T>(path))
            {
                AssetDatabase.DeleteAsset(path);
            }

            //检查文件夹是否存在
            var folderPath = Path.GetDirectoryName(path);
            if (AssetDatabase.IsValidFolder(folderPath) == false)
            {
                AssetDatabase.CreateFolder(Path.GetDirectoryName(folderPath), Path.GetFileName(folderPath));
            }
            AssetDatabase.CreateAsset(so, path);
        }
    }
}
#endif