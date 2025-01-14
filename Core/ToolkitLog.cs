using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Runtime.CompilerServices;
using System;
using System.Collections.Concurrent;
using System.Threading;
#if UNITY_5_6_OR_NEWER
using UnityEngine;
#endif

namespace UnityToolkit
{
    public static class ToolkitLog
    {
        public enum LogLevel
        {
            Debug,
            Info,
            Warning,
            Error
        }

        public static bool writeLog { get; set; } = false;

        public static Action<string> infoAction =
#if UNITY_5_6_OR_NEWER
            UnityEngine.Debug.Log;
#else
            Console.WriteLine;
#endif
        public static Action<string> warningAction =
#if UNITY_5_6_OR_NEWER
            UnityEngine.Debug.LogWarning;
#else
            Console.WriteLine;
#endif
        public static Action<string> errorAction =
#if UNITY_5_6_OR_NEWER
            UnityEngine.Debug.LogError;
#else
            Console.WriteLine;
#endif

        public static string logFilePath =
#if UNITY_5_6_OR_NEWER
            $"{Application.persistentDataPath}/log/Log_{DateTime.Now:yyyy-MM-dd}.txt";
#else
            $"./log/Log_{DateTime.Now:yyyy-MM-dd}.txt";
#endif

#if UNITY_EDITOR
        [UnityEngine.RuntimeInitializeOnLoadMethod(UnityEngine.RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetStatic()
        {
            writeLog = false;
            infoAction = UnityEngine.Debug.Log;
            warningAction = UnityEngine.Debug.LogWarning;
            errorAction = UnityEngine.Debug.LogError;
            logFilePath = $"{Application.persistentDataPath}/log/Log_{DateTime.Now:yyyy-MM-dd}.txt";
        }
#endif

        public delegate void WriteLogDelegate(string log, LogLevel logType, string path);

        /// <summary>
        /// 写日志到文件 默认的实现是非线程安全的
        /// </summary>
        public static WriteLogDelegate writeLogDelegate = (log, level, path) =>
        {
            using (var sw = File.AppendText(path))
            {
                sw.WriteLine($"{DateTime.Now:G}[{level}]: {log}");
            }
        };


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void WriteLog(string log, LogLevel logType)
        {
            if (!writeLog) return;
            // _logQueue.Enqueue($"{DateTime.Now:G}[{logType}]: {log}");
            // 第一次访问时 开启线程 定时写入文件
            writeLogDelegate(log, logType, logFilePath);
        }


        // [Conditional("Debugger")]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Info(string obj)
        {
            infoAction(obj);
            if (writeLog)
            {
                WriteLog(obj, LogLevel.Info);
            }
        }

        // [Conditional("Debugger")]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Warning(string obj)
        {
            warningAction(obj);
            if (writeLog)
            {
                WriteLog(obj, LogLevel.Warning);
            }
        }

        // [Conditional("Debugger")]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Error(string obj)
        {
            errorAction(obj);
            if (writeLog)
            {
                WriteLog(obj, LogLevel.Error);
            }
        }
        
        public static void Debug(string msg)
        {
#if DEBUG || UNITY_EDITOR
            infoAction(msg);
            if (writeLog)
            {
                WriteLog(msg, LogLevel.Debug);
            }
#endif
        }
    }
}