using UnityEngine;

namespace UnityToolkit
{
    /// <summary>
    /// 空间几何计算
    /// </summary>
    public static partial class Geometry
    {
        public static bool CircleCollision(Vector2 c1, float r1, Vector2 c2, float r2)
        {
            return Vector2.Distance(c1, c2) <= r1 + r2;
        }

        public static bool CircleCollision(Vector3 c1, float r1, Vector3 c2, float r2)
        {
            return Vector3.Distance(c1, c2) <= r1 + r2;
        }

        /// <summary>
        /// 计算从矩形中心,Vector.right为0度,顺时针旋转angel度后的交点位置
        /// </summary>
        /// <param name="rect"></param>
        /// <param name="angel"></param>
        /// <returns></returns> 
        public static Vector2 Intersection(Rect rect, float angel)
        {
            //angel转换到-180~180
            angel = (angel + 180) % 360 - 180;

            float rectAngel = Mathf.Atan2(rect.height, rect.width) * Mathf.Rad2Deg;

            if (angel >= -rectAngel && angel <= rectAngel)
            {
                return new Vector2(rect.center.x + rect.width / 2,
                    rect.center.y + Mathf.Tan(angel * Mathf.Deg2Rad) * rect.width / 2);
            }

            else if (angel >= rectAngel && angel <= 180 - rectAngel)
            {
                return new Vector2(rect.center.x + Mathf.Tan((90 - angel) * Mathf.Deg2Rad) * rect.height / 2,
                    rect.center.y + rect.height / 2);
            }
            else if (angel >= 180 - rectAngel && angel <= 180)
            {
                return new Vector2(rect.center.x - rect.width / 2,
                    rect.center.y + Mathf.Tan((180 - angel) * Mathf.Deg2Rad) * rect.width / 2);
            }
            else if (angel >= -180 && angel <= -180 + rectAngel)
            {
                return new Vector2(rect.center.x - rect.width / 2,
                    rect.center.y - Mathf.Tan((180 + angel) * Mathf.Deg2Rad) * rect.width / 2);
            }
            else
            {
                // (angel <= -rectAngel && angel >= -180 + rectAngel)
                return new Vector2(rect.center.x + Mathf.Tan((90 + angel) * Mathf.Deg2Rad) * rect.height / 2,
                    rect.center.y - rect.height / 2);
            }
        }

        public static bool InTriangle(Vector2 position, Vector2 p1, Vector2 p2, Vector2 p3)
        {
            float s = p1.y * p3.x - p1.x * p3.y + (p3.y - p1.y) * position.x + (p1.x - p3.x) * position.y;
            float t = p1.x * p2.y - p1.y * p2.x + (p1.y - p2.y) * position.x + (p2.x - p1.x) * position.y;

            if ((s < 0) != (t < 0))
            {
                return false;
            }

            float A = -p2.y * p3.x + p1.y * (p3.x - p2.x) + p1.x * (p2.y - p3.y) + p2.x * p3.y;
            if (A < 0.0)
            {
                s = -s;
                t = -t;
                A = -A;
            }

            return s > 0 && t > 0 && (s + t) <= A;
        }
        
        
        
        
    }

}