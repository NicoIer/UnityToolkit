// A simple logger class that uses Console.WriteLine by default.
// Can also do Logger.LogMethod = Debug.Log for Unity etc.
// (this way we don't have to depend on UnityEngine)

using System;
using UnityToolkit;

namespace Network
{
    internal static class NetworkLogger
    {
        internal static Action<string> Debug = ToolkitLog.Debug;
        internal static Action<string> Info = ToolkitLog.Info;
        internal static Action<string> Warning = ToolkitLog.Warning;
        internal static Action<string> Error = ToolkitLog.Error;
    }
}