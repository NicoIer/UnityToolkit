using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.RenderGraphModule;
using UnityEngine.Rendering.RenderGraphModule.Util;
using UnityEngine.Rendering.Universal;

#if UNITY_6000_0_OR_NEWER

namespace UnityToolkit
{
    public class DitherEffectRendererFeature : ScriptableRendererFeature
    {
        public Material material;
        public RenderPassEvent passEvent = RenderPassEvent.AfterRenderingPostProcessing;
        private DitherEffectPass _pass;

        public override void Create()
        {
            _pass = new DitherEffectPass();
            _pass.renderPassEvent = passEvent;
        }

        public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
        {
            if (material == null) return;
            _pass.Setup(material);
            renderer.EnqueuePass(_pass);
        }

        public class DitherEffectPass : ScriptableRenderPass
        {
            private Material _material;

            public void Setup(Material material)
            {
                this._material = material;
            }

            public override void RecordRenderGraph(RenderGraph renderGraph, ContextContainer frameData)
            {
                // Debug.Log("Record RenderGraph");
                // var stack = VolumeManager.instance.stack;
                // var customEffect = stack.GetComponent<SphereVolumeComponent>();
                var resData = frameData.Get<UniversalResourceData>();
                if (resData.isActiveTargetBackBuffer) return;
                // Debug.Log(2);
                var source = resData.activeColorTexture;
                var destDesc = renderGraph.GetTextureDesc(source);
                destDesc.name = "DitherEffect"; 
                destDesc.clearBuffer = false;
                var dest = renderGraph.CreateTexture(destDesc);
                // Debug.Log(dest);
                RenderGraphUtils.BlitMaterialParameters parameters = new (source, dest, _material, 0);
                // Debug.Log("3");
                renderGraph.AddBlitPass(parameters, passName: "DitherEffect Blit");
                resData.cameraColor = dest;
            }
        }
    }
}
#endif