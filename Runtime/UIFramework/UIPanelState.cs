// Copyright (c) 2023 NicoIer and Contributors.
// Licensed under the MIT License (MIT). See LICENSE in the repository root for more information.
﻿namespace UnityToolkit
{
    /// <summary>
    /// UI面板状态
    /// </summary>
    public enum UIPanelState
    {
        None,
        Loaded, // 已加载
        Opening, // 打开中
        Opened, // 已打开
        Closing, // 关闭中
        Closed, //  已关闭
        Disposing // 销毁中
    }

}