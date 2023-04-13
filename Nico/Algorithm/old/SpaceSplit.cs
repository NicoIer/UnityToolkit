using System.Collections.Generic;
using UnityEngine;

namespace Nico.Algorithm
{
    public static class SpaceSplit
    {
        public static List<BoundsInt> StandardBinarySpacePartitioning(BoundsInt space, int minWidth, int minHeight)
        {
            Queue<BoundsInt> roomsQueue = new Queue<BoundsInt>();
            List<BoundsInt> rooms = new List<BoundsInt>();
            roomsQueue.Enqueue(space);

            while (roomsQueue.Count > 0)
            {
                var room = roomsQueue.Dequeue();
                if (room.size.x < minWidth || room.size.y < minHeight)
                {
                    rooms.Add(room);
                    continue;
                }

                var split = Random.Range(0, 2); //随机选择一个方向进行分割
                if (split == 0)
                {
                    var (left, right) = _vertically_split(room, minWidth);
                    roomsQueue.Enqueue(left);
                    roomsQueue.Enqueue(right);
                }
                else
                {
                    var (top, bottom) = _horizontally_split(room, minHeight);
                    roomsQueue.Enqueue(top);
                    roomsQueue.Enqueue(bottom);
                }
            }

            return rooms;
        }

        private static (BoundsInt, BoundsInt) _vertically_split(BoundsInt space, int minWidth)
        {
            var xSplitPoint = Random.Range(space.xMin + minWidth, space.xMax - minWidth); //X方向上随机取点
            var left = new BoundsInt(space.xMin, space.yMin, space.zMin, xSplitPoint - space.xMin, space.size.y,
                space.size.z);
            var right = new BoundsInt(xSplitPoint, space.yMin, space.zMin, space.xMax - xSplitPoint, space.size.y,
                space.size.z);
            return (left, right);
        }

        private static (BoundsInt, BoundsInt) _horizontally_split(BoundsInt space, int minHeight)
        {
            var ySplitPoint = Random.Range(space.yMin + minHeight, space.yMax - minHeight);
            var top = new BoundsInt(space.xMin, space.yMin, space.zMin, space.size.x, ySplitPoint - space.yMin,
                space.size.z);
            var bottom = new BoundsInt(space.xMin, ySplitPoint, space.zMin, space.size.x, space.yMax - ySplitPoint,
                space.size.z);
            return (top, bottom);
        }


        public static HashSet<BoundsInt> RandomBinarySpacePartitioning(BoundsInt spaceToSplit, int minWidth, int maxWidth,
            int minHeight, int maxHeight)
        {
            Queue<BoundsInt> roomsQueue = new Queue<BoundsInt>();
            HashSet<BoundsInt> roomsList = new HashSet<BoundsInt>();
            roomsQueue.Enqueue(spaceToSplit);
            while (roomsQueue.Count > 0)
            {
                //拿出一个进行检验
                var room = roomsQueue.Dequeue();
                //如果太小了 则跳过 不加入
                if (room.size.y < minHeight || room.size.x < minWidth)
                    continue;

                //随机一个宽高

                //进行分割尝试
                if (Random.value < 0.5f)
                {
                    //先分割高度
                    if (room.size.y >= maxHeight)
                    {
                        var (top, bottom) = _random_horizontally_split(minHeight, roomsQueue, room);
                        //如果分割后 宽高合适 就 加入结果
                        if (_check(top.size, minHeight, maxHeight, minWidth, maxWidth))
                        {
                            roomsList.Add(top);
                        }
                        else
                        {
                            roomsQueue.Enqueue(top);
                        }

                        if (_check(bottom.size, minHeight, maxHeight, minWidth, maxWidth))
                        {
                            roomsList.Add(bottom);
                        }
                        else
                        {
                            roomsQueue.Enqueue(bottom);
                        }
                    }
                    else if (room.size.x >= maxWidth)
                    {
                        var (left, right) = _random_vertically_split(minWidth, roomsQueue, room);
                        //如果分割后合适 则 添加

                        if (_check(left.size, minHeight, maxHeight, minWidth, maxWidth))
                        {
                            roomsList.Add(left);
                        }
                        else
                        {
                            roomsQueue.Enqueue(left);
                        }

                        if (_check(right.size, minHeight, maxHeight, minWidth, maxWidth))
                        {
                            roomsList.Add(right);
                        }
                        else
                        {
                            roomsQueue.Enqueue(right);
                        }
                    }
                    else if (room.size.x >= minWidth && room.size.y >= minHeight)
                    {
                        roomsList.Add(room);
                    }
                }
                else
                {
                    if (room.size.y >= maxHeight)
                    {
                        var (top, bottom) = _random_vertically_split(minHeight, roomsQueue, room);
                        //如果分割后 宽高合适 就 加入结果
                        if (_check(top.size, minHeight, maxHeight, minWidth, maxWidth))
                        {
                            roomsList.Add(top);
                        }
                        else
                        {
                            roomsQueue.Enqueue(top);
                        }

                        if (_check(bottom.size, minHeight, maxHeight, minWidth, maxWidth))
                        {
                            roomsList.Add(bottom);
                        }
                        else
                        {
                            roomsQueue.Enqueue(bottom);
                        }
                    }
                    else if (room.size.x >= maxWidth)
                    {
                        var (left, right) = _random_horizontally_split(minWidth, roomsQueue, room);
                        //如果分割后合适 则 添加

                        if (_check(left.size, minHeight, maxHeight, minWidth, maxWidth))
                        {
                            roomsList.Add(left);
                        }
                        else
                        {
                            roomsQueue.Enqueue(left);
                        }

                        if (_check(right.size, minHeight, maxHeight, minWidth, maxWidth))
                        {
                            roomsList.Add(right);
                        }
                        else
                        {
                            roomsQueue.Enqueue(right);
                        }
                    }
                    else if (room.size.x >= minWidth && room.size.y >= minHeight)
                    {
                        roomsList.Add(room);
                    }
                }
            }

            return roomsList;
        }

        private static bool _check(Vector3Int size, int minHeight, int maxHeight, int minWidth, int maxWidth)
        {
            return size.y > minHeight && size.y < maxHeight && size.x > minWidth &&
                   size.x < maxWidth;
        }

        private static (BoundsInt, BoundsInt) _random_vertically_split(int minWidth, Queue<BoundsInt> roomsQueue,
            BoundsInt room)
        {
            var xSplit = Random.Range(1, room.size.x);
            BoundsInt room1 = new BoundsInt(room.min, new Vector3Int(xSplit, room.size.y, room.size.z));
            BoundsInt room2 = new BoundsInt(new Vector3Int(room.min.x + xSplit, room.min.y, room.min.z),
                new Vector3Int(room.size.x - xSplit, room.size.y, room.size.z));
            return (room1, room2);
        }

        private static (BoundsInt, BoundsInt) _random_horizontally_split(int minHeight, Queue<BoundsInt> roomsQueue,
            BoundsInt room)
        {
            var ySplit = Random.Range(1, room.size.y);
            BoundsInt room1 = new BoundsInt(room.min, new Vector3Int(room.size.x, ySplit, room.size.z));
            BoundsInt room2 = new BoundsInt(new Vector3Int(room.min.x, room.min.y + ySplit, room.min.z),
                new Vector3Int(room.size.x, room.size.y - ySplit, room.size.z));
            return (room1, room2);
        }
    }
}