using System;
using System.IO;
using UnityEngine;

namespace Nico.Util
{
    public static class FileUtil
    {
        public static readonly string ReadOnlyDataPath = Application.dataPath;
        public static readonly string ReadWriteDataPath = Application.persistentDataPath;

        public static void SaveJson<T>(T obj, string fileName, EncryptionEnum encryptionEnum = EncryptionEnum.None)
        {
            //文件名必须包含后缀.json 否则报错
            if (!fileName.EndsWith(".json"))
            {
                throw new Exception("文件名必须包含后缀.json");
            }

            var json = JsonUtility.ToJson(obj);
            string savePath = Path.Combine(ReadWriteDataPath, fileName);
            TryCreateFile(savePath);
            File.WriteAllText(savePath, json);
        }

        public static T LoadJson<T>(string fileName)
        {
            if (!fileName.EndsWith(".json"))
            {
                throw new Exception("文件名必须包含后缀.json");
            }

            string savePath = Path.Combine(ReadWriteDataPath, fileName);
            var json = File.ReadAllText(savePath);
            return JsonUtility.FromJson<T>(json);
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