using System;
using System.Collections.Generic;
using Nico.DialogSystem;
using UnityEngine;

namespace Nico.DialogSystem.Editor
{
    public partial class DialogGraphEditorData
    {
        [Serializable]
        public class NodeData
        {
            public string name;
            [field: TextArea] public string content;
            public bool inGroup;
            public string groupName;
            public DialogTypeEnum dialogTypeEnum;
            public List<ConnectInfo> choices;
            public Vector2 position;
        }

        [Serializable]
        public class GroupData
        {
            public string name;
            public Vector2 position;
        }
    }
}