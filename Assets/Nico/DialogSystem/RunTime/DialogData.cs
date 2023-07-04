using UnityEngine;

namespace Nico.DialogSystem
{
    public class DialogData : ScriptableObject
    {
        [SerializeField] private SerializableDictionary<string, DialogNode> _globalNodes; //全局节点

        [SerializeField]
        private SerializableDictionary<string, SerializableDictionary<string, DialogNode>> _groupNodes; //组节点

        public void Init(SerializableDictionary<string, DialogNode> globalNodes,
            SerializableDictionary<string, SerializableDictionary<string, DialogNode>> groupNodes)
        {
            this._globalNodes = globalNodes;
            this._groupNodes = groupNodes;
        }

        public DialogNode Get(string nodeName,string groupName)
        {
            if (groupName == null)
            {
                return _globalNodes[nodeName];
            }

            return _groupNodes[groupName][nodeName];

        }
    }
}