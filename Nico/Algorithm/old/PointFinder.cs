using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Nico.Algorithm
{
    public static class PointFinder
    {
        /// <summary>
        /// 获取一个圆心,其构成的圆的点都在点集中
        /// </summary>
        /// <param name="points"></param>
        /// <param name="radius"></param>
        /// <returns></returns>
        public static Vector2Int? GetCirclePoint(HashSet<Vector2Int> points, int radius)
        {
            HashSet<Vector2Int> set = new HashSet<Vector2Int>(points);
            //对每个陆地上的点
            foreach (var point in points)
            {
                bool isValidSpawnPoint = true;
                for (int x = point.x - radius; x <= point.x + radius; x++)
                {
                    for (int y = point.y - radius; y <= point.y + radius; y++)
                    {
                        //检测半径范围内的点
                        Vector2Int currentPosition = new Vector2Int(x, y);
                        if (!set.Contains(currentPosition))
                        {
                            //如果半径内的点有一个不存在
                            isValidSpawnPoint = false;
                            break;
                        }
                    }

                    //找到了合适的点
                    if (!isValidSpawnPoint)
                    {
                        break;
                    }
                }

                if (isValidSpawnPoint)
                {
                    return point;
                }
            }

            return null;
        }

        public static Vector2Int GetCirclePointForce(HashSet<Vector2Int> points, int radius)
        {
            radius = Math.Abs(radius);
            for (int i = radius; i != 0; i--)
            {
                var result = GetCirclePoint(points, radius);
                if (result != null)
                {
                    return result.Value;
                }
            }

            //找不到则随机选点
            return points.ElementAt(UnityEngine.Random.Range(0, points.Count));
        }


        /// <summary>
        /// 获取点集的中心点 不保证中心点在陆地上
        /// </summary>
        /// <param name="points"></param>
        /// <returns></returns>
        public static Vector2Int GetCenterPoint(HashSet<Vector2Int> points)
        {
            if (points.Count == 0)
            {
                return Vector2Int.zero;
            }

            Vector2Int sum = Vector2Int.zero;
            foreach (Vector2Int point in points)
            {
                sum += point;
            }

            return sum / points.Count;
        }

        public static List<Vector2Int> GetNeighbors(List<Vector2Int> points, Vector2Int point)
        {
            var neighbors = new List<Vector2Int>();
            for (int i = -1; i < 1; i++)
            {
                for (int j = -1; j < 1; j++)
                {
                    var neighbor = new Vector2Int(point.x + i, point.y + j);
                    if (points.Contains(neighbor)) //ToDo 可优化
                    {
                        neighbors.Add(neighbor);
                    }
                }
            }

            return neighbors;
        }
        /// <summary>
        /// 从点集中寻找一个圆心点,其半径radius构成的圆都在点集中,且这个点不在visited中
        /// </summary>
        /// <param name="regionPoints"></param>
        /// <param name="radius"></param>
        /// <param name="visited"></param>
        /// <returns></returns>
        public static Vector2Int GetSquareCenterPoint(IEnumerable<Vector2Int> regionPoints, int radius,
            HashSet<Vector2Int> visited)
        {
            // 遍历每个点，以它为圆心判断是否满足条件
            List<Vector2Int> temp = new List<Vector2Int>(regionPoints);
            Matrix.Shuffle(temp);
            HashSet<Vector2Int> points = new HashSet<Vector2Int>(temp);

            foreach (var center in points)
            {
                // 如果圆心已经被访问过了，则跳过当前点
                if (visited.Contains(center))
                    continue;

                // 判断以center为圆心，半径为radius的圆内是否全是点集中的点
                bool allInCircle = true;
                for (int x = -radius; x <= radius && allInCircle; x++)
                {
                    for (int y = -radius; y <= radius && allInCircle; y++)
                    {
                        var point = new Vector2Int(center.x + x, center.y + y);
                        // 如果圆内有点不在点集中，则跳过
                        if (!points.Contains(point) //不在点集中
                            ||
                            visited.Contains(point) //已经被拿去做圆心了
                           )
                        {
                            // Debug.Log($"点{point}没有被选择");
                            allInCircle = false;
                            break;
                        }
                    }
                }

                if (allInCircle)
                {
                    visited.Add(center);
                    return center;
                }
            }

            return Vector2Int.zero;
        }

        
    }
}