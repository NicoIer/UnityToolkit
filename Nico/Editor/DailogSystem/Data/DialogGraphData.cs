using System.Collections.Generic;
using UnityEngine;

namespace Nico.Editor.DialogSystem
{
    //Editor中的保存数据
    public partial class DialogGraphData : ScriptableObject
    {
        public string fileName;
        public SerializableDictionary<string, NodeData> globalNodeData;
        public SerializableDictionary<string, SerializableDictionary<string, NodeData>> groupNodeData;
        public List<GroupData> groupDatas;

        public NodeData GetGlobalNode(string name)
        {
            return globalNodeData[name];
        }

        public NodeData GetGroupNode(string groupName, string nodeName)
        {
            return groupNodeData[groupName][nodeName];
        }
    }


}