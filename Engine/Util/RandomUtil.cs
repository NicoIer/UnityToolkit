
namespace UnityToolkit
{
    public static class RandomUtil
    {
        public static UnityEngine.Color Random()
        {
            //随机生成颜色
            return new UnityEngine.Color(UnityEngine.Random.value, UnityEngine.Random.value, UnityEngine.Random.value);
        }
    }
}