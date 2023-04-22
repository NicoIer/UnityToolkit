using System.Collections.Generic;
using Nico.Util;
using UnityEngine;

namespace Nico.Editor.DialogSystem
{
    public class DialogErrorData
    {
        public Color color { get; private set; }

        public DialogErrorData()
        {
            color = RandomUtil.GetRandomColor();
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