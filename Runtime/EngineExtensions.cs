#if UNITY_5_6_OR_NEWER
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using UnityEngine;
using Random = UnityEngine.Random;

namespace UnityToolkit
{
    public enum TextColor
    {
        Sky = 0,
        Red = 1,
        Blue = 2,
        Pink = 3,
        Green = 4,
        White = 5,
        Yellow = 6,
        Purple = 7,
        Orange = 8,
    }

    public static class EngineExtensions
    {
        /// <summary>
        /// 为字符串添加HTML格式的颜色标签
        /// </summary>
        /// <param name="s"></param>
        /// <param name="color"></param>
        /// <returns></returns>
        public static string Color(this string s, Color color)
        {
            string hex = ColorUtility.ToHtmlStringRGB(color);
            return $"<color=#{hex}>{s}</color>";
        }

        /// <summary>
        /// 将Color转换为16进制颜色
        /// </summary>
        /// <param name="color"></param>
        /// <returns></returns>
        public static string ColorToHex(this Color32 color)
        {
            string hex = color.r.ToString("X2") + color.g.ToString("X2") + color.b.ToString("X2") +
                         color.a.ToString("X2");
            return hex;
        }


        /// <summary>
        /// 将16进制颜色转换为Color
        /// </summary>
        /// <param name="hex"></param>
        /// <returns></returns>
        public static Color HexToColor(this string hex)
        {
            hex = hex.Replace("0x", ""); //in case the string is formatted 0xFFFFFF
            hex = hex.Replace("#", ""); //in case the string is formatted #FFFFFF
            byte a = 255; //assume fully visible unless specified in hex
            byte r = byte.Parse(hex.Substring(0, 2), System.Globalization.NumberStyles.HexNumber);
            byte g = byte.Parse(hex.Substring(2, 2), System.Globalization.NumberStyles.HexNumber);
            byte b = byte.Parse(hex.Substring(4, 2), System.Globalization.NumberStyles.HexNumber);
            //Only use alpha if the string has enough characters
            if (hex.Length == 8)
            {
                a = byte.Parse(hex.Substring(6, 2), System.Globalization.NumberStyles.HexNumber);
            }

            return new Color32(r, g, b, a);
        }


        /// <summary>
        /// 获取透视相机的矩形区域
        /// </summary>
        /// <param name="camera"></param>
        /// <returns></returns>
        public static Rect GetPerspectiveCameraRect(this Camera camera, float distance)
        {
            Vector3 cameraPos = camera.transform.position;
            if (distance <= 0 || distance > camera.farClipPlane)
            {
                distance = camera.farClipPlane;
            }

            // fov表示相机的垂直视野角度
            // 2 * tan(fov/2) = height / distance
            float height = 2 * distance * Mathf.Tan(camera.fieldOfView / 2 * Mathf.Deg2Rad);
            float width = height * camera.aspect;

            Rect rect = new Rect
            {
                xMin = cameraPos.x - width / 2,
                xMax = cameraPos.x + width / 2,
                yMin = cameraPos.y - height / 2,
                yMax = cameraPos.y + height / 2
            };
            return rect;
        }

        /// <summary>
        /// 获取正交相机的矩形区域
        /// </summary>
        /// <param name="camera"></param>
        /// <returns></returns>
        public static Rect GetOrthographicCameraRect(this Camera camera)
        {
            Vector3 cameraPos = camera.transform.position;
            float height = camera.orthographicSize * 2; //高度 = 正交相机的size*2
            float width = height * camera.aspect; //宽度 = 高度*宽高比

            Rect rect = new Rect
            {
                xMin = cameraPos.x - width / 2,
                xMax = cameraPos.x + width / 2,
                yMin = cameraPos.y - height / 2,
                yMax = cameraPos.y + height / 2
            };
            return rect;
        }


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Sync(this Transform transform, Transform target)
        {
            transform.position = target.position;
            transform.rotation = target.rotation;
            transform.localScale = target.localScale;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void SyncPosition(this Transform transform, Transform target)
        {
            transform.position = target.position;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void SyncRotation(this Transform transform, Transform target)
        {
            transform.rotation = target.rotation;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector3 ZeroY(this Vector3 v)
        {
            v.y = 0;
            return v;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector3 ZeroX(this Vector3 v)
        {
            v.x = 0;
            return v;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector3 ZeroZ(this Vector3 v)
        {
            v.z = 0;
            return v;
        }

        public static T RandomTake<T>(this HashSet<T> set)
        {
            if (set.Count == 0)
            {
                throw new NullReferenceException($"HashSet<{typeof(T)}> is empty");
            }

            int index = Random.Range(0, set.Count);
            T t = set.ElementAt(index);
            set.Remove(t);
            return t;
        }
        
        public static List<T> Shuffle<T>(this List<T> list)
        {
            for (int i = 0; i < list.Count; i++)
            {
                int index = Random.Range(0, list.Count);
                (list[i], list[index]) = (list[index], list[i]);
            }

            return list;
        }

        public static T RandomTake<T>(this List<T> list)
        {
            if (list.Count == 0)
            {
                throw new NullReferenceException($"List<{typeof(T)}> is empty");
            }

            int index = Random.Range(0, list.Count);
            T t = list[index];
            list.RemoveAt(index);
            return t;
        }

        public static T RandomTakeWithoutRemove<T>(this List<T> list)
        {
            if (list.Count == 0)
            {
                throw new NullReferenceException($"List<{typeof(T)}> is empty");
            }

            int index = Random.Range(0, list.Count);
            T t = list[index];
            return t;
        }
    }
}
#endif