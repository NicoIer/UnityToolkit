using System;
using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace UnityToolkit
{
    public static class CommonUtils
    {
        public static string PrettyBytes(this long bytes)
        {
            // bytes
            if (bytes < 1024)
            {
                return $"{bytes} B";
            }
            // kilobytes

            if (bytes < 1024L * 1024L)
            {
                return $"{(bytes / 1024f):F2} KB";
            }
            // megabytes

            if (bytes < 1024 * 1024L * 1024L)
            {
                return $"{(bytes / (1024f * 1024f)):F2} MB";
            }

            // gigabytes
            return $"{(bytes / (1024f * 1024f * 1024f)):F2} GB";
        }

        // pretty print seconds as hours:minutes:seconds(.milliseconds/100)s.
        // double for long running servers.
        public static string PrettySeconds(this double seconds)
        {
            TimeSpan t = TimeSpan.FromSeconds(seconds);
            string res = "";
            if (t.Days > 0) res += $"{t.Days}d";
            if (t.Hours > 0) res += $"{(res.Length > 0 ? " " : "")}{t.Hours}h";
            if (t.Minutes > 0) res += $"{(res.Length > 0 ? " " : "")}{t.Minutes}m";
            // 0.5s, 1.5s etc. if any milliseconds. 1s, 2s etc. if any seconds
            if (t.Milliseconds > 0) res += $"{(res.Length > 0 ? " " : "")}{t.Seconds}.{(t.Milliseconds / 100)}s";
            else if (t.Seconds > 0) res += $"{(res.Length > 0 ? " " : "")}{t.Seconds}s";
            // if the string is still empty because the value was '0', then at least
            // return the seconds instead of returning an empty string
            return res != "" ? res : "0s";
        }
        
        public static Rect KeepInScreen(this Rect rect)
        {
            // ensure min
            rect.x = Math.Max(rect.x, 0);
            rect.y = Math.Max(rect.y, 0);

            // ensure max
            rect.x = Math.Min(rect.x, Screen.width - rect.width);
            rect.y = Math.Min(rect.y, Screen.width - rect.height);

            return rect;
        }
        
        public static bool IsSceneActive(this string scene)
        {
            Scene activeScene = SceneManager.GetActiveScene();
            return activeScene.path == scene ||
                   activeScene.name == scene;
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsPointInScreen(this Vector2 point) =>
            0 <= point.x && point.x < Screen.width &&
            0 <= point.y && point.y < Screen.height;
    }
}