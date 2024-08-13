using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Runtime.CompilerServices;
using System;
#if UNITY_5_6_OR_NEWER
using UnityEngine;
using Debug = UnityEngine.Debug;
#endif

namespace UnityToolkit
{
    public static class ToolkitLog
    {
        public static bool writeLog { get; set; }

        private static string LogFilePath =>
#if UNITY_5_6_OR_NEWER
            Application.persistentDataPath + "/Log_" +
            System.DateTime.Now.ToString("yyyy-MM-dd") + ".txt";
#else
            "./Log_" + System.DateTime.Now.ToString("yyyy-MM-dd") + ".txt";
#endif

        // [Conditional("Debugger")]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Info(object obj)
        {
#if UNITY_5_6_OR_NEWER
            Debug.Log(obj);
#else
            Console.WriteLine(obj);
#endif
        }

        // [Conditional("Debugger")]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Warning(object obj)
        {
#if UNITY_5_6_OR_NEWER
            Debug.LogWarning(obj);
#else
            Console.WriteLine(obj);
#endif
        }

        // [Conditional("Debugger")]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Error(object obj)
        {
#if UNITY_5_6_OR_NEWER
            Debug.LogError(obj);
#else
            Console.WriteLine(obj);
#endif
        }
#if UNITY_5_6_OR_NEWER
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void WriteLogToText(string log, LogType logType = LogType.Error)
        {
            var sw = File.AppendText(LogFilePath);
            sw.WriteLine($"{System.DateTime.Now.ToString("G")}: {logType}: {log}");
            sw.Close();
        }
#endif
    }
}