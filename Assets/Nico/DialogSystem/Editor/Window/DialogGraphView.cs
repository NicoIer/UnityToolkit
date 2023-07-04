using System;
using System.Collections.Generic;
using System.Linq;
using Nico.DialogSystem;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;

namespace Nico.DialogSystem.Editor
{
    public class DialogGraphView : GraphView
    {
        private readonly DialogGraphWindow editorWindow;
        public SerializableDictionary<string, NodeErrorData> UgNodes { get; private set; }
        public SerializableDictionary<Group, SerializableDictionary<string, NodeErrorData>> GNodes { get; private set; }
        public SerializableDictionary<string, DialogGroupErrorData> GroupsData { get; private set; }

        private int _repeatNameCount;
        private MiniMap _miniMap;

        private int RepeatNameCount
        {
            get => _repeatNameCount;
            set
            {
                _repeatNameCount = value;
                if (_repeatNameCount == 0)
                {
                    editorWindow.EnableSaving();
                }

                if (_repeatNameCount == 1)
                {
                    editorWindow.DisableSaving();
                }
            }
        }

        public DialogGraphView(DialogGraphWindow editorWindow, StyleSheet sheet)
        {
            this.editorWindow = editorWindow;
            styleSheets.Add(sheet);

            var searchWindow = ScriptableObject.CreateInstance<DialogSearchWindow>();
            searchWindow.SetGraphView(this);

            UgNodes = new SerializableDictionary<string, NodeErrorData>();
            GNodes = new SerializableDictionary<Group, SerializableDictionary<string, NodeErrorData>>();
            GroupsData = new SerializableDictionary<string, DialogGroupErrorData>();

            nodeCreationRequest = context =>
            {
                SearchWindow.Open(new SearchWindowContext(context.screenMousePosition), searchWindow);
            };

            deleteSelection = _delete_element_callback;

            elementsAddedToGroup = (group, elements) =>
            {
                foreach (var element in elements)
                {
                    if (element is not BaseDialogNode node) continue;
                    RemoveUnGroupedNode(node);
                    AddGroupedNode(group as DialogGroup, node);
                }
            };
            //delete 时 也会触发这个移除方法 
            elementsRemovedFromGroup = (group, elements) =>
            {
                //TODO 在删除组中节点时 会触发这个方法 因此需要判断是删除还是移动
                foreach (var element in elements)
                {
                    if (element is not BaseDialogNode node) continue;
                    RemoveGroupedNode(group, node);
                    AddUnGroupedNode(node);
                }
            };

            groupTitleChanged = (group, newTitle) =>
            {
                if (group is DialogGroup dialogGroup)
                {
                    RemoveDialogGroup(dialogGroup);
                    dialogGroup.oldTitle = newTitle;
                    AddDialogGroup(dialogGroup);
                }
            };

            _add_grid_background();
            _add_manipulators();
            _add_min_map();
        }

        private void _add_min_map()
        {
            _miniMap = new MiniMap
            {
                anchored = true
            };
            _miniMap.SetPosition(new Rect(10, 30, 200, 140));
            Add(_miniMap);
            _miniMap.visible = false;
        }

        private void _delete_element_callback(string operationName, AskUser user)
        {
            List<BaseDialogNode> unGroupedNodesToDelete = new List<BaseDialogNode>();
            List<BaseDialogNode> groupedNodesToDelete = new List<BaseDialogNode>();
            List<DialogGroup> groupsToDelete = new List<DialogGroup>();
            List<Edge> edgesToDelete = new List<Edge>();
            foreach (var selectable in selection)
            {
                if (selectable is Edge edge)
                {
                    edgesToDelete.Add(edge);
                    continue;
                }

                if (selectable is BaseDialogNode node)
                {
                    if (node.group != null)
                    {
                        groupedNodesToDelete.Add(node);
                    }
                    else
                    {
                        unGroupedNodesToDelete.Add(node);
                    }

                    continue;
                }

                if (selectable is DialogGroup dialogGroup)
                {
                    groupsToDelete.Add(dialogGroup);
                    continue;
                }
            }

            foreach (var dialogGroup in groupsToDelete)
            {
                List<BaseDialogNode> diaglogNodes = new List<BaseDialogNode>();
                foreach (var element in dialogGroup.containedElements)
                {
                    if (element is BaseDialogNode node)
                    {
                        diaglogNodes.Add(node);
                    }
                }

                dialogGroup.RemoveElements(diaglogNodes);
                RemoveDialogGroup(dialogGroup);
                RemoveElement(dialogGroup);
            }

            DeleteElements(edgesToDelete); //删除选中的边
            foreach (var node in groupedNodesToDelete)
            {
                //TODO 这里非常的绕 remove会触发 elementsRemovedFromGroup 触发后 会将node添加到 unGroupedNodesToDelete 
                //TODo 因此删除后 需要再次从unGrouped中删除
                node.DisconnectAllPorts();
                RemoveElement(node);
                RemoveUnGroupedNode(node);
            }

            foreach (var node in unGroupedNodesToDelete)
            {
                node.DisconnectAllPorts();
                RemoveUnGroupedNode(node);
                RemoveElement(node);
            }
        }

        private void _add_grid_background()
        {
            GridBackground gridBackground = new GridBackground();
            gridBackground.StretchToParentSize();
            Insert(0, gridBackground);
        }

        private void _add_manipulators()
        {
            SetupZoom(ContentZoomer.DefaultMinScale, ContentZoomer.DefaultMaxScale);
            this.AddManipulator(new ContentDragger());
            //顺序很重要，先添加拖拽，再添加选择
            this.AddManipulator(new SelectionDragger());
            this.AddManipulator(new RectangleSelector());
            this.AddManipulator(CreateNodeContextualMenuManipulator());
            this.AddManipulator(CreateGroupContextMenu());
        }

        #region Override

        public override List<Port> GetCompatiblePorts(Port startPort, NodeAdapter nodeAdapter)
        {
            List<Port> compatiblePorts = new List<Port>();
            ports.ForEach(port =>
            {
                if (startPort == port)
                {
                    return;
                }

                if (startPort.node == port.node)
                {
                    return;
                }

                if (startPort.direction == port.direction)
                {
                    return;
                }

                compatiblePorts.Add(port);
            });
            return compatiblePorts;
        }

        #endregion


        #region IManipulator

        private IManipulator CreateNodeContextualMenuManipulator()
        {
            //TODO understand this
            ContextualMenuManipulator manipulator = new ContextualMenuManipulator(
                menuEvent =>
                {
                    menuEvent.menu.AppendAction("Add Single Node",
                        actionEvent =>
                        {
                            AddElement(CreateNode<SingleDialogNode>("DialogName",
                                GetLocalMousePosition(actionEvent.eventInfo.mousePosition)));
                        });
                    menuEvent.menu.AppendAction("Add Multi Node",
                        actionEvent =>
                        {
                            AddElement(CreateNode<MulDialogNode>("DialogName",
                                GetLocalMousePosition(actionEvent.eventInfo.mousePosition)));
                        });
                });
            return manipulator;
        }

        private IManipulator CreateGroupContextMenu()
        {
            ContextualMenuManipulator manipulator = new ContextualMenuManipulator(
                menuEvent =>
                {
                    menuEvent.menu.AppendAction("Add Group",
                        actionEvent =>
                        {
                            AddElement(CreateGroup("Group",
                                GetLocalMousePosition(actionEvent.eventInfo.mousePosition)));
                        });
                });
            return manipulator;
        }

        #endregion

        #region Elements

        public DialogGroup CreateGroup(string name, Vector2 localMousePosition)
        {
            DialogGroup group = new DialogGroup(name, localMousePosition);

            AddDialogGroup(group);
            AddElement(group);
            foreach (var selectable in selection)
            {
                if (selectable is not BaseDialogNode diaglogNode)
                {
                    continue;
                }

                group.AddElement(diaglogNode);
            }

            return group;
        }

        public T CreateNode<T>(string dialogName, Vector2 position) where T : BaseDialogNode, new()
        {
            T node = new T();
            node.dialogName = dialogName;
            node.Init();
            node.SetGraphView(this);
            node.Draw(position);
            AddUnGroupedNode(node);
            AddElement(node);
            return node;
        }

        #endregion

        #region Data

        public void RemoveUnGroupedNode(BaseDialogNode dialogNode)
        {
            UgNodes[dialogNode.dialogName].nodes.Remove(dialogNode);
            //移除后，如果没有节点了，就移除这个dialog
            if (UgNodes[dialogNode.dialogName].nodes.Count <= 0)
            {
                UgNodes.Remove(dialogNode.dialogName);
                return;
            }

            //移除后，如果只有一个节点了，就重置颜色
            if (UgNodes[dialogNode.dialogName].nodes.Count != 1) return;

            --RepeatNameCount;
            foreach (var baseNode in UgNodes[dialogNode.dialogName].nodes)
            {
                baseNode.ResetColor();
            }
        }

        public void AddUnGroupedNode(BaseDialogNode dialogNode)
        {
            dialogNode.ResetColor();
            if (!UgNodes.ContainsKey(dialogNode.dialogName))
            {
                NodeErrorData errorData = new NodeErrorData();
                errorData.nodes.Add(dialogNode);
                UgNodes.Add(dialogNode.dialogName, errorData);
                return;
            }

            //这里已经重复了
            UgNodes[dialogNode.dialogName].nodes.Add(dialogNode);
            Color errorColor = UgNodes[dialogNode.dialogName].errorData.color;
            dialogNode.SetErrorColor(errorColor);
            //第一次重复
            if (UgNodes[dialogNode.dialogName].nodes.Count == 2)
            {
                ++RepeatNameCount;
                UgNodes[dialogNode.dialogName].nodes.ElementAt(0).SetErrorColor(errorColor);
            }
        }

        public void RemoveGroupedNode(Group group, BaseDialogNode dialogNode)
        {
            dialogNode.ResetColor();
            GNodes[group][dialogNode.dialogName].nodes.Remove(dialogNode);

            //如果删除后 该组只有一个节点了，那么就把颜色重置
            if (GNodes[group][dialogNode.dialogName].nodes.Count == 1)
            {
                --RepeatNameCount;
                GNodes[group][dialogNode.dialogName].nodes.ElementAt(0).ResetColor();
                return;
            }

            //如果删除后 该组没有节点了，那么就把该组删除
            if (GNodes[group][dialogNode.dialogName].nodes.Count <= 0)
            {
                GNodes[group].Remove(dialogNode.dialogName);
            }

            //如果删除后 该组没有节点了，那么就把该组删除
            if (GNodes[group].Count <= 0)
            {
                GNodes.Remove(group);
            }

            dialogNode.group = null;
        }


        public void AddGroupedNode(DialogGroup group, BaseDialogNode dialogNode)
        {
            dialogNode.group = group;
            dialogNode.ResetColor();
            //新组
            if (!GNodes.ContainsKey(group))
            {
                GNodes.Add(group, new SerializableDictionary<string, NodeErrorData>());
            }

            //组内新名称
            if (!GNodes[group].ContainsKey(dialogNode.dialogName))
            {
                var errorData = new NodeErrorData();
                errorData.nodes.Add(dialogNode);
                GNodes[group].Add(dialogNode.dialogName, errorData);
                return;
            }

            //组内重名
            Color errorColor = GNodes[group][dialogNode.dialogName].errorData.color;
            GNodes[group][dialogNode.dialogName].nodes.Add(dialogNode);
            dialogNode.SetErrorColor(errorColor);
            //第一次重名
            if (GNodes[group][dialogNode.dialogName].nodes.Count == 2)
            {
                ++RepeatNameCount;
                GNodes[group][dialogNode.dialogName].nodes.ElementAt(0).SetErrorColor(errorColor);
            }
        }

        private void RemoveDialogGroup(DialogGroup dialogGroup)
        {
            dialogGroup.ResetColor();
            GroupsData[dialogGroup.oldTitle].groupList.Remove(dialogGroup);

            if (GroupsData[dialogGroup.oldTitle].groupList.Count == 1)
            {
                --RepeatNameCount;
                GroupsData[dialogGroup.oldTitle].groupList[0].ResetColor();
                return;
            }

            if (GroupsData[dialogGroup.oldTitle].groupList.Count == 0)
            {
                GroupsData.Remove(dialogGroup.oldTitle);
            }
        }

        private void AddDialogGroup(DialogGroup group)
        {
            if (!GroupsData.ContainsKey(group.title))
            {
                DialogGroupErrorData errorData = new DialogGroupErrorData();
                errorData.groupList.Add(group);
                GroupsData.Add(group.title, errorData);
                return;
            }

            Color errorColor = GroupsData[group.title].errorData.color;
            group.SetErrorColor(errorColor);
            GroupsData[group.title].groupList.Add(group);
            //第一次重复
            if (GroupsData[group.title].groupList.Count == 2)
            {
                ++RepeatNameCount;
                //将第一个节点的颜色设置为错误颜色
                GroupsData[group.title].groupList.ElementAt(0).SetErrorColor(errorColor);
            }
        }

        #endregion

        #region Util

        public Vector2 GetLocalMousePosition(Vector2 mousePosition, bool isSerachWindow = false)
        {
            if (isSerachWindow)
            {
                mousePosition -= editorWindow.position.position;
            }

            return contentViewContainer.WorldToLocal(mousePosition);
        }

        #endregion

        #region Save

        #endregion


        #region Load

        #endregion

        public void ClearGraphView()
        {
            //清空数据 
            UgNodes.Clear();
            GNodes.Clear();
            GroupsData.Clear();
            RepeatNameCount = 0;
            //清空视图中的数据元素
            foreach (var element in graphElements)
            {
                if (element is BaseDialogNode)
                {
                    RemoveElement(element);
                }

                if (element is DialogGroup)
                {
                    RemoveElement(element);
                }

                if (element is Edge)
                {
                    RemoveElement(element);
                }
            }
        }

        public void LoadGraphData(DialogGraphEditorData editorData)
        {
            //首先复原数据
            //复原组
            foreach (var groupData in editorData.groupDatas)
            {
                CreateGroup(groupData.name, groupData.position);
            }

            List<Tuple<BaseDialogNode, ConnectInfo>> connectInfos = new List<Tuple<BaseDialogNode, ConnectInfo>>();
            //复原节点
            foreach (var (_, nodeData) in editorData.globalNodeData)
            {
                if (nodeData.inGroup)
                {
                    throw new Exception("节点数据错误");
                }


                BaseDialogNode node;
                if (nodeData.dialogTypeEnum == DialogTypeEnum.SingleChoice)
                {
                    node = CreateNode<SingleDialogNode>(nodeData.name, nodeData.position);
                }
                else if (nodeData.dialogTypeEnum == DialogTypeEnum.MultipleChoice)
                {
                    node = CreateNode<MulDialogNode>(nodeData.name, nodeData.position);
                }
                else
                {
                    Debug.LogError("未知的节点类型");
                    continue;
                }

                node.group = null;
                node.NameTextField.value = nodeData.name;
                node.ContentTextField.value = nodeData.content;
                foreach (var connectInfo in nodeData.choices)
                {
                    connectInfos.Add(new(node, connectInfo));
                }
            }

            foreach (var (groupName, groupData) in editorData.groupNodeData)
            {
                var group = GroupsData[groupName].groupList[0];
                foreach (var (_, nodeData) in groupData)
                {
                    if (!nodeData.inGroup)
                    {
                        throw new Exception("节点数据错误");
                    }

                    BaseDialogNode node;
                    if (nodeData.dialogTypeEnum == DialogTypeEnum.SingleChoice)
                    {
                        node = CreateNode<SingleDialogNode>(nodeData.name, nodeData.position);
                    }
                    else if (nodeData.dialogTypeEnum == DialogTypeEnum.MultipleChoice)
                    {
                        node = CreateNode<MulDialogNode>(nodeData.name, nodeData.position);
                    }
                    else
                    {
                        Debug.LogError("未知的节点类型");
                        continue;
                    }

                    node.NameTextField.value = nodeData.name;
                    node.ContentTextField.value = nodeData.content;
                    group.AddElement(node);


                    foreach (var connectInfo in nodeData.choices)
                    {
                        connectInfos.Add(new(node, connectInfo));
                    }
                }
            }

            //复原连线
            foreach (var (startNode, connectInfo) in connectInfos)
            {
                BaseDialogNode targetNode;

                if (connectInfo.isNextInGroup)
                {
                    var group = GroupsData[connectInfo.nextGroupName].groupList[0];
                    targetNode = GNodes[group][connectInfo.nextNodeName].nodes.ElementAt(0);
                }
                else
                {
                    targetNode = UgNodes[connectInfo.nextNodeName].nodes.ElementAt(0);
                }

                Port endPort = targetNode.inputContainer.Q<Port>();
                Port startPort;
                //多选节点需要创建输出端口
                if (startNode is MulDialogNode mulDialogNode)
                {
                    startPort = mulDialogNode.CreateChoicePort(connectInfo.choiceName);
                    mulDialogNode.outputContainer.Add(startPort);
                    mulDialogNode.RefreshPorts();
                }
                else if (startNode is SingleDialogNode singleDialogNode)
                {
                    startPort = singleDialogNode.outputContainer.Q<Port>();
                }
                else
                {
                    throw new Exception("未知的节点类型");
                }

                Edge edge = new Edge
                {
                    input = endPort,
                    output = startPort
                };
                edge.input.Connect(edge);
                edge.output.Connect(edge);
                AddElement(edge);
            }
        }


        public void ToggleMinimap()
        {
            _miniMap.visible = !_miniMap.visible;
        }
    }
}