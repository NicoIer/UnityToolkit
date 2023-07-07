using System.IO;
using Newtonsoft.Json;
using UnityEngine;

namespace Nico
{
    public static class JsonManager
    {
        public static bool Deserialize<T>(string json, out T obj)
        {
            obj = JsonConvert.DeserializeObject<T>(json);
            return obj != null;
        }

        public static bool Serialize<T>(T obj, out string json)
        {
            json = JsonConvert.SerializeObject(obj);
            return !string.IsNullOrEmpty(json);
        }

        public static bool ToJsonFile<T>(T obj, string path)
        {
            if (!Serialize(obj, out var json)) return false;
            FileUtil.ReplaceContent(path,json);
            return true;
        }
        
        public static bool FromJsonFile<T>(string path, out T obj)
        {
            if (!FileUtil.TryReadAllText(path, out var json)) return Deserialize(json, out obj);
            obj = default;
            return false;
        }
    }
}