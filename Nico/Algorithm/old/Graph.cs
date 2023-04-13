using System;
using System.Collections.Generic;
using UnityEngine;

namespace Nico.Algorithm
{
    /// <summary>
    /// 图形学相关算法
    /// </summary>
    public static class Graph
    {

        public static List<Vector2Int> GetLinePoint2D(Vector2Int begin, Vector2Int end)
        {
            var line = new List<Vector2Int>();
            int x = begin.x;
            int y = begin.y;

            int dx = end.x - x;
            int dy = end.y - y;

            bool inverted = false;
            int step = Math.Sign(dx);
            int gradientStep = Math.Sign(dy);

            int longest = Mathf.Abs(dx);
            int shortest = Mathf.Abs(dy);

            if (longest < shortest)
            {
                inverted = true;
                longest = Mathf.Abs(dy);
                shortest = Mathf.Abs(dx);

                step = Math.Sign(dy);
                gradientStep = Math.Sign(dx);
            }

            int gradientAccumulation = longest / 2;
            for (int i = 0; i < longest; i++)
            {
                line.Add(new Vector2Int(x, y));

                if (inverted)
                {
                    y += step;
                }
                else
                {
                    x += step;
                }

                gradientAccumulation += shortest;
                if (gradientAccumulation >= longest)
                {
                    if (inverted)
                    {
                        x += gradientStep;
                    }
                    else
                    {
                        y += gradientStep;
                    }

                    gradientAccumulation -= longest;
                }
            }

            return line;
        }


        public static HashSet<Vector2Int> GetEdgePoint2D(HashSet<Vector2Int> points)
        {
            var edgePoints = new HashSet<Vector2Int>();

            foreach (var point in points)
            {
                var up = new Vector2Int(point.x, point.y + 1);
                var down = new Vector2Int(point.x, point.y - 1);
                var left = new Vector2Int(point.x - 1, point.y);
                var right = new Vector2Int(point.x + 1, point.y);

                if (!points.Contains(up) || !points.Contains(down) || !points.Contains(left) ||
                    !points.Contains(right))
                {
                    edgePoints.Add(point);
                }
            }

            return edgePoints;
        }
        
        
        public static List<(T, T, float)> CalEdges<T>(List<T> points, Func<T, T, float> distanceFunc)
        {
            List<(T, T, float)> edges = new List<(T, T, float)>();
            for (int i = 0; i < points.Count; i++)
            {
                for (int j = i + 1; j < points.Count; j++)
                {
                    float distance = distanceFunc(points[i], points[j]);
                    edges.Add((points[i], points[j], distance));
                }
            }

            return edges;
        }


        public static List<(T, T)> MinimumSpanningTree<T>(List<T> nodes, Func<T, T, float> distanceFunc)
        {
            var edges = CalEdges(nodes, distanceFunc);

            // 对边集按权值排序
            edges.Sort((a, b) => a.Item3.CompareTo(b.Item3));

            // 初始化parentDict
            Dictionary<T, T> parentDict = new Dictionary<T, T>();
            foreach (var node in nodes)
            {
                parentDict[node] = node;
            }

            // Kruskal算法构造最小生成树
            List<(T, T)> result = new List<(T, T)>();
            foreach (var edge in edges)
            {
                T root1 = Find(edge.Item1, parentDict);
                T root2 = Find(edge.Item2, parentDict);
                if (!root1.Equals(root2))
                {
                    Union(root1, root2, parentDict);
                    result.Add((edge.Item1, edge.Item2));
                }
            }

            return result;
        }

        private static void Union<T>(T root1, T root2, Dictionary<T, T> parentDict)
        {
            parentDict[root2] = root1;
        }

        private static T Find<T>(T point, Dictionary<T, T> parentDict)
        {
            if (!parentDict[point].Equals(point))
            {
                parentDict[point] = Find(parentDict[point], parentDict);
            }

            return parentDict[point];
        }
    }
}