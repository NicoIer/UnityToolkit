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

        public static Action<string> infoAction =
#if UNITY_5_6_OR_NEWER
            Debug.Log;
#else
            Console.WriteLine;
#endif
        public static Action<string> warningAction =
#if UNITY_5_6_OR_NEWER
            Debug.LogWarning;
#else
            Console.WriteLine;
#endif
        public static Action<string> errorAction =
#if UNITY_5_6_OR_NEWER
            Debug.LogError;
#else
            Console.WriteLine;
#endif

        private static string LogFilePath =>
#if UNITY_5_6_OR_NEWER
            Application.persistentDataPath + "/Log_" +
            System.DateTime.Now.ToString("yyyy-MM-dd") + ".txt";
#else
            "./Log_" + System.DateTime.Now.ToString("yyyy-MM-dd") + ".txt";
#endif

        // [Conditional("Debugger")]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Info(string obj)
        {
            infoAction(obj);
        }

        // [Conditional("Debugger")]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Warning(string obj)
        {
            warningAction(obj);
        }

        // [Conditional("Debugger")]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Error(string obj)
        {
            errorAction(obj);
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

        public static void Debug(string msg)
        {
#if DEBUG || UNITY_EDITOR
            infoAction(msg);
#endif
        }
    }
}