#if UNITY_5_6_OR_NEWER
using UnityEngine;

namespace UnityToolkit
{
    public static class BulletGeometry
    {
        /// <summary>
        /// 子弹预测算法
        /// </summary>
        /// <param name="monsterVec">移动的物体的速度</param>
        /// <param name="monsterPos">移动物体的起始位置</param>
        /// <param name="bulletSpeed">子弹的速度大小</param>
        /// <param name="startPos">子弹的发射位置</param>
        /// <returns></returns>
        public static Vector3 BulletPredicted(Vector3 monsterVec, Vector3 monsterPos, float bulletSpeed,
            Vector3 startPos)
        {
            // 怪物的速度大小 非向量
            float monsterSpeed = monsterVec.magnitude;
            // 怪物前进方向 和 (子弹发射点 - 怪物位置) 的夹角
            float theta = Vector3.Angle(monsterVec, startPos - monsterPos);
            // 怪物和子弹发射点的距离
            float distance = Vector3.Distance(startPos, monsterPos);
            // 求根公式的delta
            float delta = Mathf.Pow((2 * distance * Mathf.Cos(theta)), 2) -
                          4 * (1 - Mathf.Pow(monsterSpeed / bulletSpeed, 2)) * Mathf.Pow(distance, 2);
            // 无解则返回怪物的位置
            if (delta < 0)
            {
                return monsterPos;
            }

            // 求根公式的分母
            float denominator = 2 * (1 - Mathf.Pow(monsterSpeed / bulletSpeed, 2));
            // 两个根
            float targetDistance1 = -(-2 * distance * Mathf.Cos(theta)) + Mathf.Sqrt(delta) / denominator;
            float targetDistance2 = -(-2 * distance * Mathf.Cos(theta)) - Mathf.Sqrt(delta) / denominator;
            // 使用最小的那个
            float usefulDistance = Mathf.Min(targetDistance1, targetDistance2);
            // 按照指定方向 走 对应的距离 就是目标点
            return monsterPos + monsterVec.normalized * usefulDistance;
        }
    }
}
#endif