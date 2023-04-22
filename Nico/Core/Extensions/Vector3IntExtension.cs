using UnityEngine;

namespace Nico
{
    public static class Vector3IntExtension
    {
        public static bool Opposite(this Vector3Int a, Vector3Int b)
        {
            //判断两个方向是否相反
            return a.x == -b.x && a.y == -b.y;
        }

        public static Vector3Int RandomDir2D()
        {
            int dir = Random.Range(0, 4);
            //随机选择一个方向
            Vector3Int dirPos = Vector3Int.zero;
            switch (dir)
            {
                case 0:
                    dirPos = Vector3Int.right;
                    break;
                case 1:
                    dirPos = Vector3Int.left;
                    break;
                case 2:
                    dirPos = Vector3Int.up;
                    break;
                case 3:
                    dirPos = Vector3Int.down;
                    break;
            }

            return dirPos;
        }

        public static Vector3Int GetDir2D(int i)
        {
            var dirPos = Vector3Int.zero;
            switch (i)
            {
                case 0:
                    dirPos = Vector3Int.right;
                    break;
                case 1:
                    dirPos = Vector3Int.left;
                    break;
                case 2:
                    dirPos = Vector3Int.up;
                    break;
                case 3:
                    dirPos = Vector3Int.down;
                    break;
            }

            return dirPos;
        }
    }
}