#if UNITY_5_6_OR_NEWER
using System;
using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace UnityToolkit
{
    public static class EngineUtils
    {
        public static Color RandomColor()
        {
            float r = UnityEngine.Random.value;
            float g = UnityEngine.Random.value;
            float b = UnityEngine.Random.value;
            //随机生成颜色
            return new Color(r, g, b);
        }

        public static bool IsSceneActive(string scene)
        {
            Scene activeScene = SceneManager.GetActiveScene();
            return activeScene.path == scene ||
                   activeScene.name == scene;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsPointInScreen(Vector2 point)
        {
            return 0 <= point.x && point.x < Screen.width &&
                   0 <= point.y && point.y < Screen.height;
        }


        public static Rect KeepInScreen(Rect rect)
        {
            // ensure min
            rect.x = Math.Max(rect.x, 0);
            rect.y = Math.Max(rect.y, 0);

            // ensure max
            rect.x = Math.Min(rect.x, Screen.width - rect.width);
            rect.y = Math.Min(rect.y, Screen.width - rect.height);

            return rect;
        }
    }
}
#endif