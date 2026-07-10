// Copyright (c) 2023 NicoIer and Contributors.
// Licensed under the MIT License (MIT). See LICENSE in the repository root for more information.
﻿#if UNITY_EDITOR
using System;
using UnityEditor;
using UnityEditor.Callbacks;
using UnityEngine;

namespace UnityToolkit.Editor
{
    public static class UIDataBaseAssetHandler
    {
        [OnOpenAsset]
#if UNITY_6000_5_OR_NEWER
        public static bool OnOpenAsset(EntityId assetId, int line)
#else
        public static bool OnOpenAsset(int assetId, int line)
#endif
        {
#if UNITY_6000_3_OR_NEWER
            if (EditorUtility.EntityIdToObject(assetId) is UIDatabase asset)
#else
            if (EditorUtility.InstanceIDToObject(assetId) is UIDatabase asset)
#endif
            {
                Debug.LogWarning("Open UIPanelDatabase not implemented yet.");
                // UIDataBaseEditorWindow.ShowWindow(asset);
                // return true;
            }

            return false;
        }
    }
}
#endif
