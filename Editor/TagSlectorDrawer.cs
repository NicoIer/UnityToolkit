// Copyright (c) 2023 NicoIer and Contributors.
// Licensed under the MIT License (MIT). See LICENSE in the repository root for more information.

#if UNITY_EDITOR

namespace UnityToolkit.Editor
{
    using UnityEditor;
    using UnityEngine;

    [CustomPropertyDrawer(typeof(TagSelectorAttribute))]
    public class TagSelectorDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            // 开始绘制属性
            EditorGUI.BeginProperty(position, label, property);

            // 只有字符串类型才能使用 Tag 选择器
            if (property.propertyType == SerializedPropertyType.String)
            {
                // 如果当前值为空，默认为 "Untagged"
                if (string.IsNullOrEmpty(property.stringValue))
                {
                    property.stringValue = "Untagged";
                }

                // 核心：调用 Unity 内置的 TagField 绘制方法
                property.stringValue = EditorGUI.TagField(position, label, property.stringValue);
            }
            else
            {
                // 如果不是字符串，提示错误
                EditorGUI.LabelField(position, label.text, "Use [TagSelector] with string.");
            }

            EditorGUI.EndProperty();
        }
    }
}
#endif