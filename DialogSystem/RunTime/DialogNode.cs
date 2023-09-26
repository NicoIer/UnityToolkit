using System;
using System.Collections.Generic;
using UnityEngine;

namespace UnityToolkit.DialogSystem
{
    [Serializable]
    public class DialogNode
    {
        public string name; //对话节点的名字
        [field: TextArea] public string content; //对话节点的内容
        public string group; //对话节点所在的组
        public bool InGroup;
        public DialogTypeEnum dialogTypeEnum; //对话节点的类型
        public SerializableDictionary<string, ConnectInfo> choices; //对话节点的选项 
    }
}