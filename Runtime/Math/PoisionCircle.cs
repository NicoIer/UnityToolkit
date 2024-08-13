#if UNITY_5_6_OR_NEWER
using System;
using UnityEngine;
using UnityEngine.Assertions;

namespace UnityToolkit
{
    public struct Circle2D
    {
        public Vector2 center;
        public float radius;
        
        public Circle2D(Vector2 center, float radius)
        {
            this.center = center;
            this.radius = radius;
        }
    }

    /// <summary>
    /// 大逃杀游戏中的毒圈缩圈算法
    /// </summary>
    public static class PoisionCircle
    {
        public static Circle2D Step(Vector3 center, float radius, Vector3 destination, float percent)
        {
            Assert.IsTrue(percent is >= 0 and <= 1);
            var direction = (destination - center).normalized;
            var distance = Vector3.Distance(destination, center);
            var newRadius = radius * (1 - percent);
            var newCenter = center + direction * (distance - newRadius);
            return new Circle2D(newCenter, newRadius);
        }
    }
}
#endif