using UnityToolkit.DialogSystem;
using UnityEngine;

namespace UnityToolkit.DialogSystem.Editor
{
    internal static class DialogDataExtension
    {
        public static DialogData ConvertToRunTime(this DialogGraphEditorData graphEditorData)
        {
            DialogData data = ScriptableObject.CreateInstance<DialogData>();
            //通过反射 设置值
            var dataGlobalNodes = new SerializableDictionary<string, DialogNode>();
            var dataGroupNodes = new SerializableDictionary<string, SerializableDictionary<string, DialogNode>>();
            foreach (var (nodeName, nodeData) in graphEditorData.globalNodeData)
            {
                var runTimeNode = nodeData.ConvertToRunTime();
                dataGlobalNodes.Add(nodeName, runTimeNode);
            }

            foreach (var (groupName, groupedNodeDataes) in graphEditorData.groupNodeData)
            {
                var groupNodes = new SerializableDictionary<string, DialogNode>();
                foreach (var (nodeName, nodeData) in groupedNodeDataes)
                {
                    var runTimeNode = nodeData.ConvertToRunTime();
                    groupNodes.Add(nodeName, runTimeNode);
                }

                dataGroupNodes.Add(groupName, groupNodes);
            }

            data.Init(dataGlobalNodes, dataGroupNodes);
            return data;
        }

        public static DialogNode ConvertToRunTime(this DialogGraphEditorData.NodeData nodeData)
        {
            var runTimeNode = new DialogNode
            {
                group = nodeData.groupName,
                name = nodeData.name,
                InGroup = nodeData.inGroup,
                content = nodeData.content,
                dialogTypeEnum = nodeData.dialogTypeEnum,
                choices = new SerializableDictionary<string, ConnectInfo>()
            };
            foreach (var choice in nodeData.choices)
            {
                runTimeNode.choices.Add(choice.choiceName, choice);
            }

            return runTimeNode;
        }
    }
}