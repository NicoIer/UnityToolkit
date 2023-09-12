using System.IO;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEditor.UIElements;

namespace Nico.Editor
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
            VisualElement root = rootVisualElement;
            if (uxml == null)
            {
                uxml = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(
                    $"{NicoEditorUtil.pluginPath}/DataTable/DataTable.Editor/DataTableEditorWindow.uxml");
            }

            if (uss == null)
            {
                uss = AssetDatabase.LoadAssetAtPath<StyleSheet>(
                    $"{NicoEditorUtil.pluginPath}/DataTable/DataTable.Editor/DataTableEditorWindow.uss");
            }

            VisualElement labelFromUxml = uxml.Instantiate();
            labelFromUxml.styleSheets.Add(uss);
            root.Add(labelFromUxml);

            if (config == null)
            {
                string path = $"{NicoEditorUtil.pluginPath}/DataTable/DataTable.Editor/TableDataConfig.asset";
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
            }
            catch (FileNotFoundException e)
            {
                Debug.LogError("找不到文件:" + e.FileName);
            }
            finally
            {
                AssetDatabase.Refresh();
                _importDataTableButton.SetEnabled(true);
            }
        }
    }
}