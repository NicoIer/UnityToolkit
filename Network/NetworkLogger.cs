// A simple logger class that uses Console.WriteLine by default.
// Can also do Logger.LogMethod = Debug.Log for Unity etc.
// (this way we don't have to depend on UnityEngine)

using System;
using UnityToolkit;

namespace Network
{
    public static class NetworkLogger
    {
        public static Action<string> Debug = ToolkitLog.Debug;
        public static Action<string> Info = ToolkitLog.Info;
        public static Action<string> Warning = ToolkitLog.Warning;
        public static Action<string> Error = ToolkitLog.Error;
    }
}