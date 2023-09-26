using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;
using UnityToolkit.Editor;
using Path = System.IO.Path;

namespace UnityToolkit.DialogSystem.Editor
{
    public class DialogGraphWindow : EditorWindow
    {
        //在Inspector中可以拖拽的Sheet文件
        [SerializeField] private StyleSheet graphViewSheet;
        private Button _saveButton;
        private DialogGraphView _graphView;
        private TextField _fileNameTextField;
        private string currentFilePath=null;

        [MenuItem("Tools/Nico/DialogGraph")]
        public static void ShowEditorWindow()
        {
            GetWindow<DialogGraphWindow>("Dialog Graph Editor");
        }

        private void CreateGUI()
        {
            AddGraphView();
            AddToolBar();
        }

        private void AddToolBar()
        {
            Toolbar toolbar = new Toolbar();
            _fileNameTextField = UIElementUtil.CreateTextField("DialogGraph", "filePath:");
            _saveButton = UIElementUtil.CreateButton("save");
            Button loadButton = UIElementUtil.CreateButton("load");
            Button minimapButton = UIElementUtil.CreateButton("minmap");

            toolbar.Add(_fileNameTextField);
            toolbar.Add(_saveButton);
            toolbar.Add(loadButton);
            toolbar.Add(minimapButton);

            _saveButton.clicked += Save;
            loadButton.clicked += Load;
            minimapButton.clicked += () =>
            {
                _graphView.ToggleMinimap();
            };
            rootVisualElement.Add(toolbar);
        }

        private void Save()
        {
            string fileName = null;
            string path = null;
           
            if (currentFilePath == null)
            {
                fileName = _fileNameTextField.value;
                var folderPath = EditorUtility.OpenFolderPanel("Dialogue Graphs", "Assets/Settings/", "");
                if (folderPath == null)
                {
                    return;
                }
                //去除前面的路径 只保留Assets/后面的部分
                folderPath = folderPath.Replace(Application.dataPath, "Assets");
                path = Path.Combine(folderPath, $"{fileName}.asset");
            }
            else //如果当前已经load了一个文件
            {
                fileName = Path.GetFileName(currentFilePath);
                path = currentFilePath;
            }
            
            DialogGraphEditorData graphEditorData = _graphView.GetGraphData(fileName);
            AssetUtil.SaveScriptableObject(graphEditorData, path);
        }

        public void Load(DialogGraphEditorData editorData,string filePath)
        {
            currentFilePath = filePath;
            //修改fileNameTextField的值
            _fileNameTextField.value = editorData.fileName;
            //首先清空GraphView中的所有元素
            _graphView.ClearGraphView();
            //然后加载数据
            _graphView.LoadGraphData(editorData);
        }

        private void Load()
        {
            var filePath = EditorUtility.OpenFilePanel("Dialogue Graphs", "Assets/Settings/", "asset");
            if (string.IsNullOrEmpty(filePath))
            {
                return;
            }

            //裁剪掉前面的路径 只保留Assets/后面的部分
            filePath = filePath.Replace(Application.dataPath, "Assets");

            DialogGraphEditorData editorData = AssetDatabase.LoadAssetAtPath<DialogGraphEditorData>(filePath);

            if (editorData == null)
            {
                Debug.LogError("加载失败");
                return;
            }
            Load(editorData,filePath);
        }


        private void AddGraphView()
        {
            _graphView = new DialogGraphView(this, graphViewSheet);
            _graphView.StretchToParentSize(); //修改GraphView的大小
            rootVisualElement.Add(_graphView);
        }

        public void EnableSaving()
        {
            _saveButton.SetEnabled(true);
        }

        public void DisableSaving()
        {
            _saveButton.SetEnabled(false);
        }
    }
}