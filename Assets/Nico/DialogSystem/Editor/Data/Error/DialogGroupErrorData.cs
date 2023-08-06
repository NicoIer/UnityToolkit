using System.Collections.Generic;

namespace Nico.DialogSystem.Editor
{
    public class DialogGroupErrorData
    {
        public DialogErrorData errorData { get; }
        public List<DialogGroup> groupList { get; }

        public DialogGroupErrorData()
        {
            errorData = new DialogErrorData();
            groupList = new List<DialogGroup>();
        }
    }
}