using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace Nico.Editor
{
    [CustomEditor(typeof(UIPanel), true)]
    internal class UIPanelEditor : UnityEditor.Editor
    {
        public TextAsset panelCodeTemplate;

        const string bindingTemplate =
            "\t\t\t{#ComponentName#} = transform.Find(\"{#ComponentPath#}\").GetComponent<{#ComponentType#}>();";

        const string fieldTemplate = "\t\tprivate {#ComponentType#} {#ComponentName#};";
        private bool _generating = false;

        public List<Type> buildInComponents = new List<Type>()
        {
            typeof(Button),
            typeof(UIComponent),
        };

        public override  void OnInspectorGUI()
        {
            //这样会把Odin的序列化取消掉，变得很丑
            base.OnInspectorGUI();
            if (GUILayout.Button("Generate Code"))
            {
                if (_generating) return;
                _generating = true;
                GenerateCode(target as UIPanel);
            }
        }

        private async void GenerateCode(UIPanel uiPanel)
        {
            //找到这个类的定义文件的位置
            string path = AssetDatabase.GetAssetPath(MonoScript.FromMonoBehaviour(uiPanel));
            //同目录下生成 partial 代码
            string code = __generatePartialCode(uiPanel);
            //写入文件把生成的绑定代码 替换到 类的开头位置
            string partialPath = path.Replace(".cs", ".partial.cs");
            await System.IO.File.WriteAllTextAsync(partialPath, code);
            
            _generating = false;
            //刷新资源
            AssetDatabase.Refresh();
        }

        private string GetSearchPath(Transform child, Transform parent)
        {
            string path = "";
            while (child != parent)
            {
                path = child.name + "/" + path;
                child = child.parent;
                if (child == null)
                {
                    throw new ArgumentException($"{child} is not child of {parent}");
                    break;
                }
            }

            return path;
        }

        private List<string> __generateFieldsCode(UIPanel panel)
        {
            List<string> fields = new List<string>();
            foreach (var type in buildInComponents)
            {
                Component[] components = panel.GetComponentsInChildren(type, true);
                foreach (var component in components)
                {
                    string fieldCode = fieldTemplate;
                    fieldCode = fieldCode.Replace("{#ComponentName#}", component.name);
                    fieldCode = fieldCode.Replace("{#ComponentType#}", type.Name);
                    fields.Add(fieldCode);
                }
            }

            return fields;
        }

        private List<string> __generateBindingCode(UIPanel panel)
        {
            List<string> bindings = new List<string>();
            foreach (var type in buildInComponents)
            {
                Component[] components = panel.GetComponentsInChildren(type, true);
                foreach (var component in components)
                {
                    Transform transform = component.transform;
                    //拿到搜索路径
                    string path = GetSearchPath(transform, panel.transform);
                    string bindCode = bindingTemplate;
                    bindCode = bindCode.Replace("{#ComponentName#}", component.name);
                    bindCode = bindCode.Replace("{#ComponentPath#}", path);
                    bindCode = bindCode.Replace("{#ComponentType#}", type.Name);
                    bindings.Add(bindCode);
                }
            }

            return bindings;
        }

        private string __generatePartialCode(UIPanel panel)
        {
            Type type = panel.GetType();

            // 替换命名空间
            string code = panelCodeTemplate.text;
            code = code.Replace("{#Namespace#}", type.Namespace);
            code = code.Replace("{#ClassName#}", type.Name);

            //替换成员变量
            List<string> fieldsCode = __generateFieldsCode(panel);
            string fieldCode = "";
            foreach (var t in fieldsCode)
            {
                fieldCode += t + "\n";
            }

            code = code.Replace("{#Fields#}", fieldCode);


            //替换绑定查找代码
            List<string> bindingCodes = __generateBindingCode(panel);
            string bindingCode = "";
            foreach (var t in bindingCodes)
            {
                bindingCode += t + "\n";
            }

            code = code.Replace("{#Bindings#}", bindingCode);


            return code;
        }
    }
}