using System;
using Nico;
using UnityEngine;

namespace Nico.DialogSystem
{
    public class DialogManager : GlobalSingleton<DialogManager>
    {
        [SerializeField] private DialogData data;
        public DialogNode currentNode { get; private set; }

        public DialogNode GetNode(string nodeName, string groupName = null)
        {
            return data.Get(nodeName, groupName);
        }

        public DialogNode GetNode(DialogNode node, string choiceName = null)
        {
            if (node.dialogTypeEnum == DialogTypeEnum.MultipleChoice)
            {
            }

            if (node.dialogTypeEnum == DialogTypeEnum.SingleChoice)
            {
            }

            throw new NotImplementedException();
        }
    }
}