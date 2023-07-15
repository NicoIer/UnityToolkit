using Nico.Editor;
using UnityEditor.Experimental.GraphView;

namespace Nico.DialogSystem.Editor
{
    public class SingleDialogNode : BaseDialogNode
    {
        public override void Init()
        {
            base.Init();
            dialogTypeEnum = DialogTypeEnum.SingleChoice;
        }

        protected override void DrawOutputContainer()
        {
            Port outputPort = this.CreatePort<bool>("out");
            outputContainer.Add(outputPort);
        }
    }
}