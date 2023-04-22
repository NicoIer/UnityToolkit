using UnityEditor.Experimental.GraphView;

namespace Nico.Editor.DialogSystem
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