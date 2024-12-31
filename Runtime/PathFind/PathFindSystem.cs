#if UNITY_5_4_OR_NEWER
using System;
using System.Buffers;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.Assertions;
using UnityToolkit;

namespace UnityToolkit
{
    
    public class PathFindSystem : MonoSingleton<PathFindSystem>
    {
        public struct Node : IEquatable<Node>
        {
            public Vector2Int pos; //节点的位置
            public ushort cost; //从相邻点 抵达这个点的代价

            public bool Equals(Node other)
            {
                return pos.Equals(other.pos) && cost == other.cost;
            }

            public override bool Equals(object obj)
            {
                return obj is Node other && Equals(other);
            }

            public override int GetHashCode()
            {
                unchecked
                {
                    return (pos.GetHashCode() * 397) ^ (int)cost;
                }
            }
        }
        
        private struct PositionPair
        {
            public int sx;
            public int sy;
            public int ex;
            public int ey;

            public PositionPair(ref Vector2Int start, ref Vector2Int end)
            {
                sx = start.x;
                sy = start.y;
                ex = end.x;
                ey = end.y;
            }
        }

        public const byte StandardNodeWidth = 1; // 默认一个正方形节点的边长
        public const ushort ZeroCost = 0;
        public const ushort BlockCost = ushort.MaxValue;


        public Vector2Int size = new Vector2Int(24, 24);
        public ushort nodeWidth = StandardNodeWidth;


        private bool[] calculated;
        private int[][] parents;
        private ushort[] costs;

        public event Action OnPathUpdate = delegate { };


        // TODO 这样没办法多线程同时进行寻路 但是暂时这样写
        private bool[] visited;
        private uint[] distance;
        private SimplePriorityQueue<int, uint> queue;
        public static readonly IReadOnlyList<int> dx = new int[] { 0, 0, -1, 1 };
        public static readonly IReadOnlyList<int> dy = new int[] { -1, 1, 0, 0 };

        // 预计算数据

        public void Load(Vector2Int size, ushort[] costs, ushort nodeWidth = StandardNodeWidth)
        {
            this.nodeWidth = nodeWidth;
            this.costs = costs;
            Assert.IsTrue(size.x * size.y == costs.Length);
            queue = new SimplePriorityQueue<int, uint>();
            visited = new bool[size.x * size.y];
            distance = new uint[size.x * size.y]; // s到任意点的最短路径长度
            parents = new int[size.x * size.y][]; // 查询记录
            // Array.Fill(parents, new int[size.x * size.y]);
            calculated = new bool[size.x * size.y]; // 计算过的点
            Array.Fill(calculated, false);
            ResetCollections();
            OnPathUpdate();
        }

        public void Replace(int index, ushort cost)
        {
            if (costs[index] == cost)
                return;
            costs[index] = cost;
            Array.Fill(calculated, false);
            ResetCollections();
            OnPathUpdate();
        }

        private void ResetCollections()
        {
            queue.Clear();
            Array.Fill(visited, false);
            Array.Fill(distance, uint.MaxValue);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool QueryPath(int start, int end, out Path path)
        {
            if (calculated[start])
            {
                return CalPath(parents[start], start, end, out path);
            }

            Assert.IsNull(parents[start]);
            parents[start] = new int[size.x * size.y];
            Array.Fill(parents[start], -1);
            var parent = parents[start];

            // 初始化
            ResetCollections();
            // 初始化
            int startIdx = start;
            distance[startIdx] = 0;
            queue.Enqueue(startIdx, costs[startIdx]);

            while (queue.Count != 0)
            {
                int curIndex = queue.Dequeue();
                if (visited[curIndex])
                    continue;
                visited[curIndex] = true;

                // 遍历相邻节点
                Vector2Int curPos = Index2Pos(curIndex);
                for (int i = 0; i < 4; i++)
                {
                    int nx = curPos.x + dx[i];
                    int ny = curPos.y + dy[i];
                    if (nx < 0 || nx >= size.x || ny < 0 || ny >= size.y) // 越界
                        continue;
                    int nextIndex = Pos2Index(new Vector2Int(nx, ny));
                    if (costs[nextIndex] == BlockCost) // 障碍物
                        continue;
                    uint newDistance = distance[curIndex] + costs[nextIndex];
                    if (newDistance < distance[nextIndex])
                    {
                        distance[nextIndex] = newDistance;
                        parent[nextIndex] = curIndex;
                        queue.Enqueue(nextIndex, newDistance);
                    }
                }
            }

            calculated[start] = true;
            return CalPath(parent, start, end, out path);
        }

        private ObjectPool<List<int>> _listPool =
            new ObjectPool<List<int>>(() => new List<int>(), null, list => list.Clear());

        private bool CalPath(int[] parent, int startIdx, int end, out Path path)
        {
            path = default;
            List<int> pathList = _listPool.Get();
            int endIndex = end;
            while (endIndex != startIdx)
            {
                pathList.Add(endIndex);
                endIndex = parent[endIndex];
                if (endIndex == -1) // 无法到达
                {
                    return false;
                }
            }

            pathList.Reverse(); // 反转才是正向路径
            path = new Path(pathList);
            return true;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool QueryPath(Vector2Int start, Vector2Int end, out Path path)
        {
            int startIdx = Pos2Index(start);
            int endIdx = Pos2Index(end);
            return QueryPath(startIdx, endIdx, out path);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Vector2Int Index2Pos(int index)
        {
            return new Vector2Int(index % size.x, index / size.y);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int Pos2Index(Vector2Int pos)
        {
            return pos.x + pos.y * size.x;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Vector3 LocalToWorld(Vector2Int node)
        {
            return new Vector3(node.x * nodeWidth, 0, node.y * nodeWidth);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Vector2Int WorldToLocal(Vector3 world)
        {
            return new Vector2Int((int)(world.x / nodeWidth), (int)(world.z / nodeWidth));
        }
    }
}
#endif