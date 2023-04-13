using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Nico.Algorithm
{
    /// <summary>
    /// 矩形相关算法
    /// </summary>
    public static class Matrix
    {
        /// <summary>
        /// 二维数组 矩阵相减的运算
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <returns></returns>
        public static async void Sub(float[,] a, float[,] b, float[,] result)
        {
            int rowCount = a.GetLength(0);
            int colCount = a.GetLength(1);
            List<Task> tasks = new List<Task>();
            for (int i = 0; i < rowCount; i++)
            {
                tasks.Add(Task.Run(() =>
                {
                    for (int j = 0; j < colCount; j++)
                    {
                        result[i, j] = a[i, j] - b[i, j];
                    }
                }));
            }

            await Task.WhenAll(tasks);
        }


        /// <summary>
        /// 获取网格地图中最大的陆地网格点集合
        /// </summary>
        /// <param name="map"></param>
        /// <returns></returns>
        public static List<Vector2Int> GetBiggestLand(int[,] map)
        {
            var width = map.GetLength(0);
            var height = map.GetLength(1);
            var visited = new bool[width, height]; //访问记录
            var biggestLand = new List<Vector2Int>(); //最大陆地网格点集合
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    //遍历每个网格点
                    if (map[x, y] == 1 && !visited[x, y])
                    {
                        //如果是陆地且未被访问过
                        var land = GetLand(map, x, y, visited);
                        if (land.Count > biggestLand.Count)
                        {
                            biggestLand = land;
                        }
                    }
                }
            }

            return biggestLand;
        }

        //获取(x,y)所在陆地的网格点集合
        private static List<Vector2Int> GetLand(int[,] map, int x, int y, bool[,] visited)
        {
            var width = map.GetLength(0);
            var height = map.GetLength(1);
            var land = new List<Vector2Int>();
            //BFS
            var queue = new Queue<Vector2Int>();
            queue.Enqueue(new Vector2Int(x, y));
            while (queue.Count > 0)
            {
                var pos = queue.Dequeue();
                if (pos.x < 0 || pos.x >= width || pos.y < 0 || pos.y >= height)
                {
                    continue;
                }

                //当前网格点是陆地且未被访问过
                if (map[pos.x, pos.y] == 1 && !visited[pos.x, pos.y])
                {
                    land.Add(pos); //加入陆地网格点集合
                    visited[pos.x, pos.y] = true; //标记为已访问
                    //将当前网格点的上下左右四个网格点加入队列
                    queue.Enqueue(new Vector2Int(pos.x - 1, pos.y));
                    queue.Enqueue(new Vector2Int(pos.x + 1, pos.y));
                    queue.Enqueue(new Vector2Int(pos.x, pos.y - 1));
                    queue.Enqueue(new Vector2Int(pos.x, pos.y + 1));
                }
            }

            return land;
        }

        public static List<List<Vector2Int>> GetRegions(int[,] map, int targetType)
        {
            var width = map.GetLength(0);
            var height = map.GetLength(1);
            var regions = new List<List<Vector2Int>>();
            bool[,] visited = new bool[width, height];
            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    if (!visited[x, y] && map[x, y] == targetType)
                    {
                        var region = GetRegion(map, x, y, visited, targetType);
                        regions.Add(region);
                    }
                }
            }

            return regions;
        }

        //获取(x,y)targetType 陆地的集合
        public static List<Vector2Int> GetRegion(int[,] map, int x, int y, bool[,] visited, int targetType)
        {
            var width = map.GetLength(0);
            var height = map.GetLength(1);
            var region = new List<Vector2Int>();
            var queue = new Queue<Vector2Int>();
            queue.Enqueue(new Vector2Int(x, y));
            visited[x, y] = true;
            while (queue.Count > 0)
            {
                var pos = queue.Dequeue();
                region.Add(pos);
                for (int i = pos.x - 1; i <= pos.x + 1; i++)
                {
                    for (int j = pos.y - 1; j <= pos.y + 1; j++)
                    {
                        if (i >= 0 && i < width && j >= 0 && j < height)
                        {
                            if (!visited[i, j] && map[i, j] == targetType)
                            {
                                visited[i, j] = true;
                                queue.Enqueue(new Vector2Int(i, j));
                            }
                        }
                    }
                }
            }

            return region;
        }

        /// <summary>
        /// 网格转换 将originType组成的陆地按照limit大小转换为targetType
        /// </summary>
        /// <param name="grid"></param>
        /// <param name="limit"></param>
        /// <param name="originType"></param>
        /// <param name="targetType"></param>
        public static void GridTransForm(int[,] grid, uint limit, int originType, int targetType)
        {
            var emptyRegions = GetRegions(grid, originType);
            //洞太小 则 填充
            foreach (var region in emptyRegions)
            {
                if (region.Count < limit)
                {
                    foreach (var point in region)
                    {
                        grid[point.x, point.y] = targetType;
                    }
                }
            }
        }

        /// <summary>
        /// 添加边界
        /// </summary>
        /// <param name="grid"></param>
        /// <param name="borderSize"></param>
        /// <returns></returns>
        public static int[,] AddBorder(int[,] grid, int borderSize)
        {
            var width = grid.GetLength(0);
            var height = grid.GetLength(1);
            //加一圈边界
            var borderedMap = new int[width + borderSize * 2, height + borderSize * 2];
            for (int x = 0; x < borderedMap.GetLength(0); x++)
            {
                for (int y = 0; y < borderedMap.GetLength(1); y++)
                {
                    if (x >= borderSize && x < width + borderSize && y >= borderSize && y < height + borderSize)
                    {
                        //非边界区域进行复制
                        borderedMap[x, y] = grid[x - borderSize, y - borderSize];
                    }
                    else
                    {
                        //边界区域填充
                        borderedMap[x, y] = 1;
                    }
                }
            }

            return borderedMap;
        }


        /// <summary>
        /// 对数据归一化
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public static float[] Normalization(float[] data)
        {
            var sum = data.Sum();
            if (sum == 0)
            {
                return MinMaxNormalization(data);
            }

            for (var i = 0; i < data.Length; i++)
            {
                data[i] /= sum;
            }

            return data;
        }

        public static float[] MinMaxNormalization(float[] data)
        {
            //如果加和为0
            //min max 归一化
            var min = data.Min();
            var max = data.Max();
            if (Math.Abs(min - max) < float.Epsilon)
            {
                //如果最大值最小值相等
                for (var i = 0; i < data.Length; i++)
                {
                    data[i] = 1f / data.Length;
                }

                return data;
            } 
            
            //min max归一化
            for (int i = 0; i < data.Length; i++)
            {
                data[i] = (data[i] - min) / max;
            }

            return data;
        }
        public static void Shuffle<T>(List<T> list, int? seed = null)
        {
            seed ??= DateTime.Now.Millisecond;
            var rng = new System.Random((int)seed);

            for (int i = list.Count - 1; i > 0; i--)
            {
                int j = rng.Next(i + 1);
                (list[i], list[j]) = (list[j], list[i]);
            }
        }

        public static int GetMinWeightIdx(float[] weights)
        {
            int poorIdx = -1;
            float poorWeight = float.MaxValue;
            for (int i = 0; i < weights.Length; i++)
            {
                if (poorWeight > weights[i])
                {
                    poorWeight = weights[i];
                    poorIdx = i;
                }
            }

            return poorIdx;
        }

        public static int GetMaxWeightIdx(float[] weights)
        {
            int strongIdx = -1;
            float poorWeight = float.MinValue;
            for (int i = 0; i < weights.Length; i++)
            {
                if (poorWeight < weights[i])
                {
                    poorWeight = weights[i];
                    strongIdx = i;
                }
            }

            return strongIdx;
        }

        public static int GetRandomWeightedIdx(float[] weights)
        {
            var value = Random.value;
            var sum = 0f;
            for (int i = 0; i < weights.Length; i++)
            {
                sum += weights[i];
                if (value <= sum)
                    return i;
            }

            throw new ArgumentException();
        }



        public static void FastShuffle<T>(List<T>origin)
        {
            throw new NotImplementedException();
        }
    }
}