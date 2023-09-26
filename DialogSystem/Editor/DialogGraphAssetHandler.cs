using UnityEditor;
using UnityEditor.Callbacks;
using UnityEngine;

namespace UnityToolkit.DialogSystem.Editor
{
    public static class DialogGraphAssetHandler
    {
        [OnOpenAsset]
        public static bool OpenAsset(int instanceID, int line)
        {
            var obj = EditorUtility.InstanceIDToObject(instanceID);
            if (obj is DialogGraphEditorData asset)
            {
                var window = EditorWindow.GetWindow<DialogGraphWindow>();
                string filePath = AssetDatabase.GetAssetPath((UnityEngine.Object) obj);
                Debug.Log(filePath);
                window.Load(asset, filePath);
                return true;
            }

            return false;
        }
    }
}