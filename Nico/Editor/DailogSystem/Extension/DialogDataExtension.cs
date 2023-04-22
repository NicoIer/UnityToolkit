using Nico.DialogSystem;
using UnityEngine;

namespace Nico.Editor.DialogSystem
{
    internal static class DialogDataExtension
    {
        public static DialogData ConvertToRunTime(this DialogGraphData graphData)
        {
            DialogData data = ScriptableObject.CreateInstance<DialogData>();
            data.globalNodes = new SerializableDictionary<string, DialogNode>();
            data.groupNodes = new SerializableDictionary<string, SerializableDictionary<string, DialogNode>>();
            foreach (var (nodeName, nodeData) in graphData.globalNodeData)
            {
                var runTimeNode = nodeData.ConvertToRunTime();
                data.globalNodes.Add(nodeName, runTimeNode);
            }

            foreach (var (groupName, groupedNodeDatas) in graphData.groupNodeData)
            {
                var groupNodes = new SerializableDictionary<string, DialogNode>();
                foreach (var (nodeName, nodeData) in groupedNodeDatas)
                {
                    var runTimeNode = nodeData.ConvertToRunTime();
                    groupNodes.Add(nodeName, runTimeNode);
                }

                data.groupNodes.Add(groupName, groupNodes);
            }

            return data;
        }

        public static DialogNode ConvertToRunTime(this DialogGraphData.NodeData nodeData)
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