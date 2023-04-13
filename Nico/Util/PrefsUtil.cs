using System;
using System.IO;
using Newtonsoft.Json;
using UnityEngine;

namespace Nico.Util
{
    public static class PrefsUtil
    {
        public static readonly string ReadOnlyDataPath = Application.dataPath;
        public static readonly string ReadWriteDataPath = Application.persistentDataPath;

        public static void SaveJson<T>(T obj, string fileName, Formatting formatting = Formatting.Indented,
            EncryptionEnum encryptionEnum = EncryptionEnum.None)
        {
            //文件名必须包含后缀.json 否则报错
            if (!fileName.EndsWith(".json"))
            {
                fileName += ".json";
            }

            var json = JsonConvert.SerializeObject(obj, formatting);
            string savePath = Path.Combine(ReadWriteDataPath, fileName);
            TryCreateFile(savePath);
            File.WriteAllText(savePath, json);
        }

        public static T LoadJson<T>(string fileName)
        {
            if (!fileName.EndsWith(".json"))
            {
                fileName += ".json";
            }

            string savePath = Path.Combine(ReadWriteDataPath, fileName);
            if (!File.Exists(savePath)) return default;
            var json = File.ReadAllText(savePath);
            try
            {
                return JsonConvert.DeserializeObject<T>(json);
            }
            catch (Exception)
            {
                Debug.LogWarning($"{nameof(PrefsUtil)}加载{fileName}失败");
                return default;
            }
        }

        public static void TryCreateFile(string filePath)
        {
            // 如果文件已经存在，直接返回
            //如果文件不存在，创建文件
            //如果文件夹不存在，创建文件夹
            if (File.Exists(filePath))
            {
                return;
            }

            //创建文件夹
            var dir = Path.GetDirectoryName(filePath);
            if (dir != null)
            {
                if (!Directory.Exists(dir))
                {
                    Directory.CreateDirectory(dir);
                }
            }

            //创建文件
            File.Create(filePath).Close();
        }
    }
}