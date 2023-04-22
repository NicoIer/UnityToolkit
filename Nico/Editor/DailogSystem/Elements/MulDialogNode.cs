using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;

namespace Nico.Editor.DialogSystem
{
    public class MulDialogNode : BaseDialogNode
    {
        public override void Init()
        {
            base.Init();
            dialogTypeEnum = DialogTypeEnum.MultipleChoice;
            Button addChoiceButton = UIElementUtil.CreateButton("add", () =>
            {
                Port outputPort = CreateChoicePort();
                outputContainer.Add(outputPort);
                RefreshPorts();
            });
            addChoiceButton.AddToClassList("ds-node__button");
            mainContainer.Insert(1, addChoiceButton);
        }

        public Port CreateChoicePort(string choice = "")
        {
            Port outputPort = this.CreatePort<bool>();
            outputPort.ZeroMargin();
            outputPort.ZeroPadding();


            var port = outputPort;
            Button deleteButton = UIElementUtil.CreateButton("del", () =>
            {
                if (port.connected)
                {
                    graphView.DeleteElements(port.connections);
                }

                graphView.RemoveElement(port);
            });

            deleteButton.style.flexGrow = 0f;
            deleteButton.AddToClassList("ds-node__button");

            TextField choiceTextField = UIElementUtil.CreateTextField(choice);
            //固定choiceTextField的宽度
            choiceTextField.style.flexGrow = 0.9f;
            choiceTextField.style.width = 160;
            choiceTextField.style.minWidth = 60;
            choiceTextField.style.maxWidth = 1000;
            choiceTextField.style.flexShrink = 0f;
            choiceTextField.ZeroMargin();
            choiceTextField.ZeroPadding();
            choiceTextField.AddClasses("ds-node__textfield", "ds-node__choice-textfield", "ds-node__textfield__hidden");
            //让子元素的宽度,高度自适应
            _change_port_style(ref outputPort);
            outputPort.Add(choiceTextField);
            outputPort.Add(deleteButton);
            return outputPort;
        }

        private static void _change_port_style(ref Port port)
        {
            port.style.marginLeft = 0;
            port.style.marginRight = 0;
            port.style.marginTop = 0;
            port.style.marginBottom = 0;
            //不要有padding
            port.style.paddingLeft = 0;
            port.style.paddingRight = 0;
            port.style.paddingTop = 0;
            port.style.paddingBottom = 0;
        }
    }
}