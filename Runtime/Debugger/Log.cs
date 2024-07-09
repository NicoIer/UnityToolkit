using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Runtime.CompilerServices;
using UnityEngine;
using Debug = UnityEngine.Debug;

namespace UnityToolkit
{
    public static class Log
    {
        public static bool writeLog { get; set; }

        private static string LogFilePath => Application.persistentDataPath + "/Log_" +
                                             System.DateTime.Now.ToString("yyyy-MM-dd") + ".txt";

        // [Conditional("Debugger")]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Info(object obj)
        {
            Debug.Log(obj);
        }

        // [Conditional("Debugger")]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Warning(object obj)
        {
            Debug.LogWarning(obj);
        }

        // [Conditional("Debugger")]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Error(object obj)
        {
            Debug.LogError(obj);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void WriteLogToText(string log, LogType logType = LogType.Error)
        {
            var sw = File.AppendText(LogFilePath);
            sw.WriteLine($"{System.DateTime.Now.ToString("G")}: {logType}: {log}");
            sw.Close();
        }
    }
}