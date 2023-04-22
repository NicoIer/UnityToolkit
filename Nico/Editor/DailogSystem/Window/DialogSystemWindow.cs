using System;
using System.IO;
using Nico.DialogSystem;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace Nico.Editor.DialogSystem
{
    public class DialogSystemWindow : EditorWindow
    {
        //在Inspector中可以拖拽的Sheet文件
        [SerializeField] private StyleSheet graphViewSheet;
        private Button saveButton;
        private DialogGraphView graphView;
        private TextField fileNameTextField;
        private TextField pathTextField;

        [MenuItem("Tools/DialogEditor")]
        public static void ShowEditorWindow()
        {
            GetWindow<DialogSystemWindow>("Dialog Graph Editor");
        }

        private void CreateGUI()
        {
            AddGraphView();
            AddToolBar();
        }

        private void AddToolBar()
        {
            Toolbar toolbar = new Toolbar();
            fileNameTextField = UIElementUtil.CreateTextField("DialogFileName", "fileName:");
            pathTextField = UIElementUtil.CreateTextField("Assets/Settings/", "path:");
            saveButton = UIElementUtil.CreateButton("save");
            Button loadButton = UIElementUtil.CreateButton("load");
            toolbar.Add(fileNameTextField);
            toolbar.Add(pathTextField);
            toolbar.Add(saveButton);
            toolbar.Add(loadButton);
            saveButton.clicked += Save;
            loadButton.clicked += Load;

            rootVisualElement.Add(toolbar);
        }

        private void Save()
        {
            var fileName = fileNameTextField.value;
            var path = pathTextField.value;
            var combine = Path.Combine(path, fileName);
            // 保存dialogGraphView 默认存储到Asset/Setting/下
            var path1 = $"{combine}-Editor.asset";
            DialogGraphData graphData = graphView.GetGraphData(fileName);
            AssetUtil.SaveScriptableObject(graphData, path1);

            var path2 = $"{combine}-Runtime.asset";
            DialogData dialogData = graphData.ConvertToRunTime();
            AssetUtil.SaveScriptableObject(dialogData, path2);
        }

        private void Load()
        {
            var combine = Path.Combine(pathTextField.value, fileNameTextField.value);
            var loadPath = $"{combine}-Editor.asset";
            Debug.Log(loadPath);
            DialogGraphData data = AssetDatabase.LoadAssetAtPath<DialogGraphData>(loadPath);
            if (data == null)
            {
                Debug.LogError("加载失败");
                return;
            }
            //首先清空GraphView中的所有元素
            graphView.ClearGraphView();
            //然后加载数据
            graphView.LoadGraphData(data);
        }


        private void AddGraphView()
        {
            graphView = new DialogGraphView(this, graphViewSheet);
            graphView.StretchToParentSize(); //修改GraphView的大小
            rootVisualElement.Add(graphView);
        }

        public void EnableSaving()
        {
            saveButton.SetEnabled(true);
        }

        public void DisableSaving()
        {
            saveButton.SetEnabled(false);
        }
    }
}