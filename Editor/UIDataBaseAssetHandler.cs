#if UNITY_EDITOR
using System;
using UnityEditor;
using UnityEditor.Callbacks;
using UnityEngine;

namespace UnityToolkit.Editor
{
    public static class UIDataBaseAssetHandler
    {
        [OnOpenAsset]
        public static bool OnOpenAsset(int instanceId, int line)
        {
            if (EditorUtility.InstanceIDToObject(instanceId) is UIDatabase asset)
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