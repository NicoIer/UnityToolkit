using System;
using System.IO;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEditor.UIElements;

namespace UnityToolkit.Editor
{
    public class DataTableEditorWindow : EditorWindow
    {
        [SerializeField] private TableDataConfig config;

        [SerializeField] private VisualTreeAsset uxml;
        [SerializeField] private StyleSheet uss;
        private Button _importDataTableButton;
        private Button _codeGenButton;
        private ObjectField _configObjectField;
        private TextField _codeGenPathTextField;
        private TextField _assetGenPathTextField;

        [MenuItem("Tools/Nico/DataTable")]
        public static void ShowExample()
        {
            DataTableEditorWindow wnd = GetWindow<DataTableEditorWindow>();
            wnd.titleContent = new GUIContent("DataTableEditorWindow");
        }

        public void CreateGUI()
        {
            //加载自己当前所在的路径 
            string selfPath = AssetDatabase.GetAssetPath(MonoScript.FromScriptableObject(this));
            //去除文件名
            selfPath = Path.GetDirectoryName(selfPath);
            //  读取自己的 uxml 和 uss 文件
            VisualElement root = rootVisualElement;
            if (uxml == null)
            {
                string path = Path.Combine(selfPath, "DataTableEditorWindow.uxml");
                uxml = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(path);
            }

            if (uss == null)
            {
                string path = Path.Combine(selfPath, "DataTableEditorWindow.uss");
                uss = AssetDatabase.LoadAssetAtPath<StyleSheet>(path);
            }

            VisualElement labelFromUxml = uxml.Instantiate();
            labelFromUxml.styleSheets.Add(uss);
            root.Add(labelFromUxml);

            if (config == null)
            {
                string path = $"{selfPath}/TableDataConfig.asset";
                config = AssetDatabase.LoadAssetAtPath<TableDataConfig>(path);
                if (config == null)
                {
                    config = ScriptableObject.CreateInstance<TableDataConfig>();
                    AssetDatabase.CreateAsset(config, path);
                }
            }


            _configObjectField = rootVisualElement.Q<ObjectField>("config");
            _configObjectField.objectType = typeof(TableDataConfig);
            _configObjectField.value = config;
            _configObjectField.RegisterValueChangedCallback(evt => { config = evt.newValue as TableDataConfig; });

            _codeGenPathTextField = rootVisualElement.Q<TextField>("code_gen_path");
            _codeGenPathTextField.value = config.codeSavePath;
            _codeGenPathTextField.RegisterValueChangedCallback(evt => { config.codeSavePath = evt.newValue; });

            _assetGenPathTextField = rootVisualElement.Q<TextField>("asset_gen_path");
            _assetGenPathTextField.value = config.assetSavePath;
            _assetGenPathTextField.RegisterValueChangedCallback(evt => { config.assetSavePath = evt.newValue; });

            QueryImportDataTable();
            QueryCodeGenerate();
        }

        private void QueryImportDataTable()
        {
            VisualElement dataTable = rootVisualElement.Q<VisualElement>("import_datatable");
            _importDataTableButton = dataTable.Q<Button>();
            _importDataTableButton.clickable.clicked += ImportDataTable;
        }

        private void QueryCodeGenerate()
        {
            VisualElement codeGen = rootVisualElement.Q<VisualElement>("code_generate");
            _codeGenButton = codeGen.Q<Button>();
            _codeGenButton.clickable.clicked += CodeGenerate;
        }

        private void CodeGenerate()
        {
            _codeGenButton.SetEnabled(false);
            string projectPath = Application.dataPath; //从Assets 回退到工程目录
            projectPath = projectPath.Substring(0, projectPath.Length - 6);
            string excelPath = EditorUtility.OpenFilePanelWithFilters("select excel", projectPath,
                new string[] { "Excel Files", "csv,xlsx,xls" });
            if (string.IsNullOrEmpty(excelPath))
            {
                _codeGenButton.SetEnabled(true);
                return;
            }

            TableImporter.ImportExcel(excelPath, config, _codeGenPathTextField.value);
            _codeGenButton.SetEnabled(true);
            AssetDatabase.Refresh();
        }

        private void ImportDataTable()
        {
            _importDataTableButton.SetEnabled(false);
            string projectPath = Application.dataPath; //从Assets 回退到工程目录
            projectPath = projectPath.Substring(0, projectPath.Length - 6);
            string excelPath = EditorUtility.OpenFilePanelWithFilters("select excel", projectPath,
                new string[] { "Excel Files", "csv,xlsx,xls" });
            if (string.IsNullOrEmpty(excelPath))
            {
                _importDataTableButton.SetEnabled(true);
                return;
            }

            try
            {
                TableImporter.ImportData(excelPath, _assetGenPathTextField.value);
                AssetDatabase.Refresh();
            }
            finally
            {
                _importDataTableButton.SetEnabled(true);
            }
        }
    }
}