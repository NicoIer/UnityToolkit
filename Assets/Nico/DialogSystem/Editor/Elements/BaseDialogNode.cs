using Nico.Editor;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;

#if UNITY_EDITOR
namespace Nico.DialogSystem.Editor
{
    public abstract class BaseDialogNode : Node
    {
        public string dialogName;
        public string Content => ContentTextField.value;
        public DialogTypeEnum dialogTypeEnum;
        protected DialogGraphView graphView;
        public TextField NameTextField { get; private set; }
        public TextField ContentTextField { get; private set; }
        public DialogGroup group;

        #region Overrides

        public override void BuildContextualMenu(ContextualMenuPopulateEvent evt)
        {
            base.BuildContextualMenu(evt);
            evt.menu.AppendAction("delete inputs", _ => { _disconnect_ports(inputContainer); });
            evt.menu.AppendAction("delete outputs", _ => { _disconnect_ports(outputContainer); });
        }

        #endregion


        public virtual void Init()
        {
            if (string.IsNullOrEmpty(dialogName))
            {
                Debug.Log("dialogName is null or empty");
                dialogName = "DialogName";
            }
            mainContainer.AddToClassList("ds-node__main-container");
            extensionContainer.AddToClassList("ds-node__extension-container");
        }

        public void SetGraphView(DialogGraphView graphView)
        {
            this.graphView = graphView;
        }

        protected virtual void DrawTitleContainer()
        {
            //标题容器
            NameTextField =
                UIElementUtil.CreateTextField(dialogName, null, callback =>
                {
                    Debug.Log("name changed ");
                    if (group == null)
                    {
                        graphView.RemoveUnGroupedNode(this);
                        dialogName = callback.newValue;
                        graphView.AddUnGroupedNode(this);
                        return;
                    }

                    if (group != null)
                    {
                        var curGroup = group;
                        graphView.RemoveGroupedNode(group, this);
                        dialogName = callback.newValue;
                        graphView.AddGroupedNode(curGroup, this);
                    }
                });
            NameTextField.AddClasses("ds-node__textfield", "ds-node__filename-textfield",
                "ds-node__textfield__hidden");
            titleContainer.Insert(0, NameTextField);
        }

        protected virtual void DrawInputContainer()
        {
            //输入容器
            // typeof(bool) 的意思是，这个端口是bool类型的 只能连接 bool类型的端口
            Port inputPort = this.CreatePort<bool>("in", Orientation.Horizontal, Direction.Input, Port.Capacity.Multi);
            inputContainer.Add(inputPort);
        }

        protected virtual void DrawOutputContainer()
        {
        }

        protected virtual void DrawExtensionContainer()
        {
            //拓展容器
            //创建新的视觉元素
            VisualElement customDataContainer = new VisualElement();
            customDataContainer.AddClasses("ds-node__custom-data-container");
            Foldout textFoldout = UIElementUtil.CreateFoldout("Content");
            ContentTextField = UIElementUtil.CreateTextArea("Default Content");
            ContentTextField.AddClasses("ds-node__textfield", "ds-node__quote-textfield");
            //默认的大小更大一点
            ContentTextField.style.minHeight = 30;
            ContentTextField.style.minWidth = 40;
            //字体大一点
            ContentTextField.style.fontSize = 14;

            textFoldout.Add(ContentTextField);

            //将foldout添加到自定义数据容器(视觉元素)中
            customDataContainer.Add(textFoldout);

            //将自定义数据容器添加到扩展容器中
            extensionContainer.Add(customDataContainer);
            //刷新扩展容器的状态
            RefreshExpandedState();
        }

        public void Draw(Vector2 position)
        {
            SetPosition(new Rect(position, Vector2.one));
            DrawTitleContainer();
            DrawInputContainer();
            DrawOutputContainer();
            DrawExtensionContainer();
        }

        public void DisconnectAllPorts()
        {
            _disconnect_ports(inputContainer);
            _disconnect_ports(outputContainer);
        }

        private void _disconnect_ports(VisualElement container)
        {
            foreach (var element in container.Children())
            {
                if (element is not Port port)
                {
                    continue;
                }

                if (!port.connected)
                {
                    continue;
                }

                graphView.DeleteElements(port.connections);
            }
        }

        public void SetErrorColor(Color color)
        {
            mainContainer.style.backgroundColor = color;
        }

        public void ResetColor()
        {
            mainContainer.style.backgroundColor = new Color(29 / 255f, 29 / 255f, 30 / 255f, 1);
        }
    }
}
#endif