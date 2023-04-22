using System;

namespace Nico.DialogSystem
{
    [Serializable]
    public class ConnectInfo
    {
        public bool isNextInGroup;
        public string choiceName;
        public string nextGroupName;
        public string nextNodeName;
    }
}