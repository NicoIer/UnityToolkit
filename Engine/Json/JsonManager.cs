using Newtonsoft.Json;

namespace Nico
{
    public static class JsonManager
    {
        private static readonly JsonSerializerSettings serializerSettings = new JsonSerializerSettings
        {
            ReferenceLoopHandling = ReferenceLoopHandling.Ignore
        };

        public static bool Deserialize<T>(string json, out T obj)
        {
            if (json == null)
            {
                obj = default;
                return false;
            }


            obj = JsonConvert.DeserializeObject<T>(json);
            return obj != null;
        }

        public static bool Serialize<T>(T obj, out string json)
        {
            json = JsonConvert.SerializeObject(obj, serializerSettings);
            return !string.IsNullOrEmpty(json);
        }

        public static bool ToJsonFile<T>(T obj, string path)
        {
            if (!Serialize(obj, out var json)) return false;
            FileUtil.ReplaceContent(json, path);
            return true;
        }

        public static bool FromJsonFile<T>(string path, out T obj)
        {
            if (!FileUtil.TryReadAllText(path, out string json))
            {
                obj = default;
                return false;
            }
            return Deserialize(json, out obj);
        }
    }
}