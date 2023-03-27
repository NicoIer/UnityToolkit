using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace Nico.Editor
{
    public class TableImporter : EditorWindow
    {
        [SerializeField] private VisualTreeAsset m_VisualTreeAsset = default;

        [MenuItem("Window/UI Toolkit/TableImporter")]
        public static void ShowExample()
        {
            TableImporter wnd = GetWindow<TableImporter>();
            wnd.titleContent = new GUIContent("TableImporter");
        }

        public void CreateGUI()
        {
            // Each editor window contains a root VisualElement object
            VisualElement root = rootVisualElement;

            // VisualElements objects can contain other VisualElement following a tree hierarchy.
            VisualElement label = new Label("Hello World! From C#");
            root.Add(label);

            // Instantiate UXML
            VisualElement labelFromUXML = m_VisualTreeAsset.Instantiate();
            root.Add(labelFromUXML);
        }
    }
}