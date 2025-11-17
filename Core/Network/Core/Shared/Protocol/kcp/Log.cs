// Copyright (c) 2023 NicoIer and Contributors.
// Licensed under the MIT License (MIT). See LICENSE in the repository root for more information.
﻿// A simple logger class that uses Console.WriteLine by default.
// Can also do Logger.LogMethod = Debug.Log for Unity etc.
// (this way we don't have to depend on UnityEngine)
using System;
using Network;

namespace kcp2k
{
    internal static class Log
    {
        public static Action<string> Info = NetworkLogger.Info;
        public static Action<string> Warning = NetworkLogger.Warning;
        public static Action<string> Error = NetworkLogger.Error;
    }
}
