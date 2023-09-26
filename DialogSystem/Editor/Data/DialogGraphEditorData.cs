#if UNITY_EDITOR
using System.Collections.Generic;
using UnityToolkit.DialogSystem;
using UnityEditor;
using UnityEngine;
using UnityToolkit.Editor;

namespace UnityToolkit.DialogSystem.Editor
{
    //Editor中的保存数据
    public partial class DialogGraphEditorData : ScriptableObject
    {
        public string fileName;
        public SerializableDictionary<string, NodeData> globalNodeData;
        public SerializableDictionary<string, SerializableDictionary<string, NodeData>> groupNodeData;
        public List<GroupData> groupDatas;

        public NodeData GetGlobalNode(string name)
        {
            return globalNodeData[name];
        }

        public NodeData GetGroupNode(string groupName, string nodeName)
        {
            return groupNodeData[groupName][nodeName];
        }
    }

    //给EditorGraph转换成RunTimeGraph的Editor拓展
    [CustomEditor(typeof(DialogGraphEditorData))]
    public class DialogGraphDataInspectorEditor : UnityEditor.Editor
    {
        DialogGraphEditorData dialogGraphEditorData;

        private void OnEnable()
        {
            dialogGraphEditorData = target as DialogGraphEditorData;
        }

        public override void OnInspectorGUI()
        {
            
            if (GUILayout.Button("ConvertToRuntime"))
            {
                DialogData dialogData = dialogGraphEditorData.ConvertToRunTime();
                var path = AssetDatabase.GetAssetPath(target);
                //将最后的文件名替换成RunTime
                path = path.Replace(".asset", "-RunTime.asset");
                AssetUtil.SaveScriptableObject(dialogData, path);
            }
            base.OnInspectorGUI();
        }
    }
}
#endif