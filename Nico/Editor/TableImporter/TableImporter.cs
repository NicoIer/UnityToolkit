using System;
using System.Reflection;
using Nico.Data;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

#if UNITY_EDITOR
namespace Nico.Editor
{
    public class TableImporter : EditorWindow
    {
        //Uxml文件 拖入后自动生成 VisualTreeAsset对象
        [SerializeField] private VisualTreeAsset mVisualTreeAsset = default;

        /// <summary>
        /// 在Unity编辑器中显示窗口
        /// </summary>
        [MenuItem("Tools/TableImporter")]
        public static void ShowExample()
        {
            TableImporter wnd = GetWindow<TableImporter>(); //创建一个窗口的实例
            wnd.titleContent = new GUIContent("TableImporter"); //设置窗口的标题
        }

        #region UI控件

        private TextField _excelPathField;
        private TextField _codeSavePathField;
        private TextField _dataSavePathField;
        private EnumField _typeEnum;

        #endregion

        #region 存储信息

        ExcelTable[] _excelTables;

        #endregion


        public void CreateGUI()
        {
            // Each editor window contains a root VisualElement object
            VisualElement root = rootVisualElement;
            // Instantiate UXML
            VisualElement labelFromUxml = mVisualTreeAsset.Instantiate();
            root.Add(labelFromUxml);
            // 获取Top
            VisualElement topVisual = root.Query<VisualElement>("Top").First();
            Debug.Log("CreateGUI");

            #region Button

            //获取Top下的Button
            Button codeGenBtn = topVisual.Query<Button>("codeGenBtn").First();
            Button dataGenBtn = topVisual.Query<Button>("dataGenBtn").First();
            Button readBtn = topVisual.Query<Button>("readBtn").First();
            //给Button添加点击事件
            codeGenBtn.clickable.clicked += OnCodeGenBtnClick;
            dataGenBtn.clickable.clicked += OnDataGenBtnClick;
            readBtn.clickable.clicked += OnReadBtnClick;

            #endregion

            #region TextField

            _excelPathField = topVisual.Query<TextField>("excelPath").First();
            _codeSavePathField = topVisual.Query<TextField>("codePath").First();
            _dataSavePathField = topVisual.Query<TextField>("dataPath").First();

            #endregion

            #region TypeEnum

            _typeEnum = topVisual.Query<EnumField>("typeEnum").First();
            _typeEnum.Init(DataTypeEnum.ScriptableObject);

            #endregion
        }

        private void OnReadBtnClick()
        {
            if (!_CheckPathValid())
            {
                return;
            }
            string excelPath = _excelPathField.value;
            _excelTables = ExcelUtil.GetTables(excelPath);
            //TODO 后续根据枚举类型生成代码 
        }

        private void OnCodeGenBtnClick()
        {
            if (!_CheckExcelTableValid())
            {
                return;
            }

            var codeSavePath = _codeSavePathField.value;

            Debug.Log($"代码文件将被保存到:{codeSavePath}");
            foreach (var table in _excelTables)
            {
                CodeGenerator.Create(table, codeSavePath);
            }
            
            AssetDatabase.Refresh();
        }

        public void OnDataGenBtnClick()
        {
            if (!_CheckExcelTableValid())
                return;
            var dataSavePath = _dataSavePathField.value;
            Debug.Log($"数据生成,类型为{_typeEnum.value},保存路径:{dataSavePath}");

            string namespaceStr = typeof(IMetaData).Namespace;
            Assembly assembly = Assembly.Load("Assembly-CSharp");
            string containerSubFix = "Container";
            foreach (var table in _excelTables)
            {
                var tableName = table.tableName;
                var metaClassName = $"{tableName}";
                var metaContainerName = $"{tableName}Container";
                //在程序集里查找对应的Type
                Debug.Log($"查找类型:{namespaceStr}.{metaClassName} 和 {namespaceStr}.{metaContainerName}");
                Type metaDataType = assembly.GetType($"{namespaceStr}.{metaClassName}");
                Type metaContainerType = assembly.GetType($"{namespaceStr}.{metaContainerName}");
                Debug.Log($"生成数据容器:{metaContainerType},数据类型:{metaDataType}");
                IMetaDataContainer container = DataGenerator.Create(table, metaDataType, metaContainerType, assembly);
                if (container == null)
                {
                    Debug.Log("生成失败");
                    continue;
                }

                //将生成的数据保存到Asset
                Debug.Log($"保存数据到:{dataSavePath}/{tableName}{containerSubFix}.asset");
                AssetDatabase.CreateAsset(container as ScriptableObject,
                    $"{dataSavePath}/{tableName}{containerSubFix}.asset");
            }

            AssetDatabase.Refresh();
        }

        private bool _CheckPathValid()
        {
            if (string.IsNullOrEmpty(_excelPathField.value))
            {
                Debug.Log("请先选择Excel文件");
                return false;
            }

            if (string.IsNullOrEmpty(_codeSavePathField.value))
            {
                Debug.Log("请先选择代码保存路径");
                return false;
            }

            if (string.IsNullOrEmpty(_dataSavePathField.value))
            {
                Debug.Log("请先选择数据保存路径");
                return false;
            }

            return true;
        }

        private bool _CheckExcelTableValid()
        {
            if (_excelTables == null || _excelTables.Length == 0)
            {
                Debug.Log("请先读取Excel表");
                return false;
            }

            return true;
        }
        
    }
}
#endif