using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;

namespace Nico.Algorithm
{
    public static class Cluster
    {
        public static List<List<Vector2Int>> KMeansCluster(List<Vector2Int> points, int k, int maxIter, float limit,
            int seed)
        {
            System.Random random = new System.Random(seed);
            //随机选取k个初始点
            var clusters = new List<Vector2Int>();
            for (int i = 0; i < k; i++)
            {
                var index = random.Next(0, points.Count);
                clusters.Add(points[index]);
            }

            //分组结果
            List<List<Vector2Int>> groups = new List<List<Vector2Int>>();
            //迭代直到收敛
            int count = 0;
            while (count < maxIter)
            {
                //将所有点分配到离它最近的聚类中心
                groups.Clear();
                for (int i = 0; i < k; i++)
                {
                    groups.Add(new List<Vector2Int>());
                }

                //对每个点 计算与其最近的聚类中心
                foreach (var point in points)
                {
                    var minDistance = float.MaxValue;
                    var minIndex = 0;
                    for (int i = 0; i < k; i++)
                    {
                        var distance = Vector2Int.Distance(point, clusters[i]);
                        if (distance < minDistance)
                        {
                            minDistance = distance;
                            minIndex = i;
                        }
                    }

                    groups[minIndex].Add(point);
                }

                //重新计算每个聚类中心
                var newClusters = groups.Select(GetCenter).ToList();

                //判断是否收敛
                var isSame = true;
                for (var i = 0; i < k; i++)
                {
                    if (Vector2Int.Distance(newClusters[i], clusters[i]) < limit)
                    {
                        isSame = false;
                        break;
                    }
                }

                if (isSame)
                {
                    break;
                }

                clusters = newClusters;
                count++;
            }

            return groups;
        }

        private static Vector2Int GetCenter(List<Vector2Int> points)
        {
            if (points.Count == 0)
            {
                return Vector2Int.zero;
            }

            var x = 0;
            var y = 0;
            foreach (var point in points)
            {
                x += point.x;
                y += point.y;
            }

            x /= points.Count;
            y /= points.Count;
            return new Vector2Int(x, y);
        }

        public static List<List<Vector2Int>> KMeansClusterParallel(List<Vector2Int> points, int k, int maxIter,
            float limit,
            int seed)
        {
            System.Random random = new System.Random(seed);
            //随机选取k个初始点
            var clusters = new List<Vector2Int>();
            for (int i = 0; i < k; i++)
            {
                var index = random.Next(0, points.Count);
                clusters.Add(points[index]);
            }

            //分组结果
            List<List<Vector2Int>> groups = new List<List<Vector2Int>>();
            //迭代直到收敛
            int count = 0;
            while (count < maxIter)
            {
                //将所有点分配到离它最近的聚类中心
                groups.Clear();
                for (int i = 0; i < k; i++)
                {
                    groups.Add(new List<Vector2Int>());
                }

                //对每个点 计算与其最近的聚类中心
                var tasks = new List<Task>();
                foreach (var point in points)
                {
                    var task = Task.Run(() =>
                    {
                        var minDistance = float.MaxValue;
                        var minIndex = 0;
                        for (int i = 0; i < k; i++)
                        {
                            var distance = Vector2Int.Distance(point, clusters[i]);
                            if (distance < minDistance)
                            {
                                minDistance = distance;
                                minIndex = i;
                            }
                        }

                        lock (groups)
                        {
                            groups[minIndex].Add(point);
                        }
                    });
                    tasks.Add(task);
                }

                Task.WaitAll(tasks.ToArray());

                //重新计算每个聚类中心
                var newClusters = groups.Select(GetCenter).ToList();

                //判断是否收敛
                var isSame = true;
                for (var i = 0; i < k; i++)
                {
                    if (Vector2Int.Distance(newClusters[i], clusters[i]) < limit)
                    {
                        isSame = false;
                        break;
                    }
                }

                if (isSame)
                {
                    break;
                }

                clusters = newClusters;
                count++;
            }

            return groups;
        }

        public static List<List<Vector2Int>> DBSCANCluster(List<Vector2Int> points, int minPts, float eps)
        {
            var clusters = new List<List<Vector2Int>>();
            var visited = new HashSet<Vector2Int>();
            var noise = new List<Vector2Int>();

            for (int i = 0; i < points.Count; i++)
            {
                if (visited.Contains(points[i]))
                    continue;

                visited.Add(points[i]);
                var neighbors = GetNeighbors(points, points[i], eps);

                if (neighbors.Count < minPts)
                {
                    noise.Add(points[i]);
                }
                else
                {
                    var cluster = new List<Vector2Int>();
                    clusters.Add(cluster);
                    ExpandCluster(points, visited, neighbors, cluster, minPts, eps);
                }
            }

            if (noise.Count > 0)
                clusters.Add(noise);

            return clusters;
        }

        private static void ExpandCluster(List<Vector2Int> points, HashSet<Vector2Int> visited,
            List<Vector2Int> neighbors, List<Vector2Int> cluster, int minPts, float eps)
        {
            for (int i = 0; i < neighbors.Count; i++)
            {
                var p = neighbors[i];
                if (!visited.Contains(p))
                {
                    visited.Add(p);
                    var newNeighbors = GetNeighbors(points, p, eps);
                    if (newNeighbors.Count >= minPts)
                        neighbors.AddRange(newNeighbors);
                }

                if (!cluster.Contains(p))
                    cluster.Add(p);
            }
        }

        private static List<Vector2Int> GetNeighbors(List<Vector2Int> points, Vector2Int point, float eps)
        {
            var neighbors = new List<Vector2Int>();
            for (int i = 0; i < points.Count; i++)
            {
                if (point != points[i] && Vector2Int.Distance(point, points[i]) <= eps)
                    neighbors.Add(points[i]);
            }

            return neighbors;
        }
    }
}