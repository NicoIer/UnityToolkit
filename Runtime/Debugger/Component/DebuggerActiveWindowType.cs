// Copyright (c) 2023 NicoIer and Contributors.
// Licensed under the MIT License (MIT). See LICENSE in the repository root for more information.
﻿namespace UnityToolkit.Debugger
{
    /// <summary>
    /// 调试器激活窗口类型。
    /// </summary>
    public enum DebuggerActiveWindowType : byte
    {
        /// <summary>
        /// 总是打开。
        /// </summary>
        AlwaysOpen = 0,

        /// <summary>
        /// 仅在开发模式时打开。
        /// </summary>
        OnlyOpenWhenDevelopment,

        /// <summary>
        /// 仅在编辑器中打开。
        /// </summary>
        OnlyOpenInEditor,

        /// <summary>
        /// 总是关闭。
        /// </summary>
        AlwaysClose,
    }
}
