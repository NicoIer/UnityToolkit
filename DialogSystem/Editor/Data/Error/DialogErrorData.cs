using System.Collections.Generic;
using UnityEngine;

namespace UnityToolkit.DialogSystem.Editor
{
    public class DialogErrorData
    {
        public Color color { get; private set; }

        public DialogErrorData()
        {
            color = RandomUtil.Random();
        }
    }

    public class NodeErrorData
    {
        public DialogErrorData errorData { get; }
        public HashSet<BaseDialogNode> nodes { get; }

        public NodeErrorData()
        {
            errorData = new DialogErrorData();
            nodes = new HashSet<BaseDialogNode>();
        }
    }
}