using System;
using System.Collections.Generic;
using System.Linq;
using UnityToolkit.DialogSystem;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;

namespace UnityToolkit.DialogSystem.Editor
{
    public static class DialogIOUtil
    {
        public static DialogGraphEditorData GetGraphData(this DialogGraphView dialogGraphView, string fileName)
        {
            DialogGraphEditorData graphEditorData = ScriptableObject.CreateInstance<DialogGraphEditorData>();
            graphEditorData.fileName = fileName;
            graphEditorData.globalNodeData = dialogGraphView.GetGlobalNodeData();
            graphEditorData.groupNodeData = dialogGraphView.GetGroupNodeData();
            graphEditorData.groupDatas = dialogGraphView.GetGroupData();
            return graphEditorData;
        }

        public static SerializableDictionary<string, DialogGraphEditorData.NodeData> GetGlobalNodeData(
            this DialogGraphView dialogGraphView)
        {
            var data = new SerializableDictionary<string, DialogGraphEditorData.NodeData>();
            foreach ((string nodeName, var errorData) in dialogGraphView.UgNodes)
            {
                if (errorData.nodes.Count > 1)
                {
                    throw new Exception("节点名重复");
                }

                var node = errorData.nodes.First();
                var nodeData = new DialogGraphEditorData.NodeData
                {
                    name = nodeName,
                    content = node.Content,
                    inGroup = false,
                    groupName = null,
                    dialogTypeEnum = node.dialogTypeEnum,
                    choices = GetConnectInfo(node, node.outputContainer.Children().OfType<Port>()),
                    position = node.GetPosition().position
                };

                data.Add(nodeName, nodeData);
            }

            return data;
        }


        public static SerializableDictionary<string, SerializableDictionary<string, DialogGraphEditorData.NodeData>>
            GetGroupNodeData(this DialogGraphView dialogGraphView)
        {
            var data = new SerializableDictionary<string, SerializableDictionary<string, DialogGraphEditorData.NodeData>>();
            foreach (var (group, nodeErrorDatas) in dialogGraphView.GNodes)
            {
                var groupedNodes = new SerializableDictionary<string, DialogGraphEditorData.NodeData>();
                foreach ((string nodeName, var errorData) in nodeErrorDatas)
                {
                    if (errorData.nodes.Count > 1)
                    {
                        throw new Exception("节点名重复");
                    }

                    var nodeData = new DialogGraphEditorData.NodeData();
                    var node = errorData.nodes.First();
                    nodeData.name = nodeName;
                    nodeData.content = node.Content;
                    nodeData.inGroup = true;
                    nodeData.groupName = group.title;
                    nodeData.dialogTypeEnum = node.dialogTypeEnum;
                    nodeData.choices =
                        GetConnectInfo(node, node.outputContainer.Children().OfType<Port>());

                    nodeData.position = node.GetPosition().position;
                    groupedNodes.Add(nodeName, nodeData);
                }

                data.Add(group.title, groupedNodes);
            }

            return data;
        }

        private static List<ConnectInfo> GetConnectInfo(in BaseDialogNode startNode, IEnumerable<Port> outputPorts)
        {
            var choices = new List<ConnectInfo>();
            foreach (var port in outputPorts)
            {
                if (!port.connected) continue;

                if (port.connections.Count() > 1)
                {
                    throw new Exception("端口连接数大于1");
                }

                var edge = port.connections.First();
                var endNode = edge.input.node as BaseDialogNode;
                //如果有连接
                if (endNode == null)
                {
                    throw new Exception("目标节点不是DialogNode");
                }

                var connectInfo = new ConnectInfo
                {
                    isNextInGroup = false,
                    nextGroupName = null,
                    nextNodeName = null,
                    choiceName = null
                };


                if (endNode.group != null)
                {
                    connectInfo.isNextInGroup = true;
                    connectInfo.nextGroupName = endNode.group.oldTitle;
                    connectInfo.nextNodeName = endNode.dialogName;
                }
                else
                {
                    connectInfo.isNextInGroup = false;
                    connectInfo.nextGroupName = null;
                    connectInfo.nextNodeName = endNode.dialogName;
                }

                //根据 node 的type 来判断 choices 的key
                if (startNode is SingleDialogNode)
                {
                    connectInfo.choiceName = "out";
                }

                else if (startNode is MulDialogNode)
                {
                    //从port中拿到choiceTextField的value
                    TextField choiceTextField = port.Q<TextField>();
                    connectInfo.choiceName = choiceTextField.value;
                }
                else
                {
                    throw new Exception($"起始节点类型错误:{startNode.GetType()}");
                }

                choices.Add(connectInfo);
            }

            return choices;
        }

        public static List<DialogGraphEditorData.GroupData> GetGroupData(
            this DialogGraphView dialogGraphView)
        {
            var data = new List<DialogGraphEditorData.GroupData>();
            foreach (var (groupName, groupErrorData) in dialogGraphView.GroupsData)
            {
                if (groupErrorData.groupList.Count > 1)
                {
                    throw new Exception("组名重复");
                }

                var groupData = new DialogGraphEditorData.GroupData
                {
                    name = groupName,
                    position = groupErrorData.groupList.First().GetPosition().position
                };
                data.Add(groupData);
            }

            return data;
        }
    }
}