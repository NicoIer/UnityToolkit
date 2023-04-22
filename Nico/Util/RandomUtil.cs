using System;
using UnityEngine;

namespace Nico.Util
{
    /// <summary>
    /// 对UnityEngine.Random的封装和拓展
    /// </summary>
    public static class RandomUtil
    {
        //返回一个随机点 随机点和给定点的方向相同(cos夹角>0)
        public static Vector2 GetRandomPointWithDirection2D(Vector2 direction, float length)
        {
            throw new NotImplementedException();
        }

        public static Color GetRandomColor()
        {
            return new Color(UnityEngine.Random.value, UnityEngine.Random.value, UnityEngine.Random.value);
        }
    }
}