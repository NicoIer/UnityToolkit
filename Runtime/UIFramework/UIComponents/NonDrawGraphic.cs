#if UNITY_5_6_OR_NEWER

using UnityEngine;
using UnityEngine.UI;

namespace UnityToolkit
{
    [RequireComponent(typeof(CanvasRenderer))]
    [AddComponentMenu("Layout/Extensions/NonDrawingGraphic")]
    public class NonDrawGraphic : MaskableGraphic, IUIComponent
    {
        public override void SetMaterialDirty()
        {
            return;
        }

        public override void SetVerticesDirty()
        {
            return;
        }

        protected override void OnPopulateMesh(VertexHelper vh)
        {
            vh.Clear();
        }
    }
}

#endif