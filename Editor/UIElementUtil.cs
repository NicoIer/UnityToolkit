#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;
using UnityEngine.UIElements;


namespace UnityToolkit.Editor
{
    public static class UIElementUtil
    {
        public static void AddClasses(this VisualElement element, params string[] classList)
        {
            foreach (var className in classList)
            {
                element.AddToClassList(className);
            }
        }

        public static void ZeroPadding(this VisualElement element)
        {
            element.style.paddingLeft = 0;
            element.style.paddingRight = 0;
            element.style.paddingTop = 0;
            element.style.paddingBottom = 0;
        }

        public static void ZeroMargin(this VisualElement element)
        {
            element.style.marginLeft = 0;
            element.style.marginRight = 0;
            element.style.marginTop = 0;
            element.style.marginBottom = 0;
        }

        public static Port CreatePort<T>(this Node dialogNode, string name = "",
            Orientation orientation = Orientation.Horizontal,
            Direction direction = Direction.Output,
            Port.Capacity capacity = Port.Capacity.Single)
        {
            Port port = dialogNode.InstantiatePort(orientation, direction, capacity, typeof(T));
            port.portName = name;
            return port;
        }

        public static Button CreateButton(string text, Action onClick = null)
        {
            Button button = new Button
            {
                text = text
            };
            if (onClick != null)
            {
                button.clicked += onClick;
            }

            return button;
        }

        public static Foldout CreateFoldout(string title, bool closed = true)
        {
            Foldout foldout = new Foldout
            {
                text = title,
                value = closed
            };
            return foldout;
        }

        public static TextField CreateTextField(string value = null, string label = null,
            EventCallback<ChangeEvent<string>> onValueChanged = null, IEnumerable<string> classList = null)
        {
            TextField textField = new TextField
            {
                value = value,
                label = label
            };
            if (onValueChanged != null)
            {
                textField.RegisterValueChangedCallback(onValueChanged);
            }

            if (classList != null)
            {
                foreach (var className in classList)
                {
                    textField.AddToClassList(className);
                }
            }

            return textField;
        }

        public static TextField CreateTextArea(string value = null, string label = null,
            EventCallback<ChangeEvent<string>> onValueChanged = null, IEnumerable<string> classList = null)
        {
            TextField textArea = CreateTextField(value, label, onValueChanged, classList);
            textArea.multiline = true;
            return textArea;
        }
    }
}
#endif