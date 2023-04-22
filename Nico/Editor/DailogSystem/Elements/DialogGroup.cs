using UnityEditor.Experimental.GraphView;
using UnityEngine;

namespace Nico.Editor.DialogSystem
{
    public class DialogGroup : Group
    {
        public string oldTitle;
        private readonly Color defaultBorderColor;
        private readonly float defaultBorderWidth;

        public DialogGroup(string title, Vector2 position)
        {
            base.title = title;
            oldTitle = title;
            base.SetPosition(new Rect(position, Vector2.zero));
            defaultBorderColor = base.contentContainer.style.borderBottomColor.value;
            defaultBorderWidth = base.contentContainer.style.borderBottomWidth.value;
        }

        public void SetErrorColor(Color color)
        {
            contentContainer.style.borderBottomColor = color;
            contentContainer.style.borderBottomWidth = 2;
        }

        public void ResetColor()
        {
            contentContainer.style.borderBottomColor = defaultBorderColor;
            contentContainer.style.borderBottomWidth = defaultBorderWidth;
        }
    }
}