using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

namespace UnityToolkit.DialogSystem.Editor
{
    public class DialogSearchWindow : ScriptableObject, ISearchWindowProvider
    {
        private DialogGraphView graphView;
        private Texture2D indentIcon;

        public void SetGraphView(DialogGraphView graphView)
        {
            this.graphView = graphView;

            indentIcon = new Texture2D(1, 1);
            indentIcon.SetPixel(0, 0, new Color(0, 0, 0, 0));
            indentIcon.Apply();
        }

        public List<SearchTreeEntry> CreateSearchTree(SearchWindowContext context)
        {
            List<SearchTreeEntry> searchTreeEntries = new List<SearchTreeEntry>()
            {
                new SearchTreeGroupEntry(new GUIContent("Create Element")),
                new SearchTreeGroupEntry(new GUIContent("Dialog Node"), 1),
                new SearchTreeEntry(new GUIContent("Single Choice", indentIcon))
                {
                    level = 2,
                    userData = DialogTypeEnum.SingleChoice
                },
                new SearchTreeEntry(new GUIContent("Mul Choice", indentIcon))
                {
                    level = 2,
                    userData = DialogTypeEnum.MultipleChoice
                },
                new SearchTreeGroupEntry(new GUIContent("Group"), 1),
                new SearchTreeEntry(new GUIContent("Single Group", indentIcon))
                {
                    level = 2,
                    userData = new Group()
                },
            };
            return searchTreeEntries;
        }

        public bool OnSelectEntry(SearchTreeEntry SearchTreeEntry, SearchWindowContext context)
        {
            var position = graphView.GetLocalMousePosition(context.screenMousePosition, true);
            switch (SearchTreeEntry.userData)
            {
                case DialogTypeEnum.SingleChoice:
                {
                    graphView.CreateNode<SingleDialogNode>("DialogName",position);

                    break;
                }
                case DialogTypeEnum.MultipleChoice:
                {
                    graphView.CreateNode<MulDialogNode>("DialogName",position);
                    break;
                }
                case Group:
                {
                    graphView.CreateGroup("Group", position);
                    break;
                }
                default:
                    return false;
            }

            return true;
        }
    }
}