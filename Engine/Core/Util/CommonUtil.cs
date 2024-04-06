using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace UnityToolkit
{
    public static class CommonUtil
    {
        public static string PrettyBytes(long bytes)
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
        public static string PrettySeconds(double seconds)
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



    }
}