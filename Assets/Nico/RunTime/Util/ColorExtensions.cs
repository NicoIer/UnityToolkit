using UnityEngine;

namespace Nico
{
    public static class RandomUtil
    {
        public static Color Random()
        {
            //随机生成颜色
            return new Color(UnityEngine.Random.value, UnityEngine.Random.value, UnityEngine.Random.value);
        }
    }
}