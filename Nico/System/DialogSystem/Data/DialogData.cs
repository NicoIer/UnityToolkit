using UnityEngine;

namespace Nico.DialogSystem
{
    public class DialogData : ScriptableObject
    {
        public SerializableDictionary<string, DialogNode> globalNodes;//全局节点
        public SerializableDictionary<string, SerializableDictionary<string, DialogNode>> groupNodes;//组节点
    }

}