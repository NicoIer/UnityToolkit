using System;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace UnityToolkit
{
    public class StencilBufferRenderFeature : ScriptableRendererFeature
    {
        [Range(0, 255)] public int stencilRef;
        private StencilWritePass _passAfterTransparent;
        // private StencilWritePass _passAfterShadow;

        public Material material;

        internal RTHandle rtA;
        internal RTHandle rtB;
        public RTHandle thisFrameRT => Time.frameCount % 2 == 0 ? rtA : rtB;
        public RTHandle lastFrameRT => Time.frameCount % 2 == 0 ? rtB : rtA;

        private static readonly int _stencilRef = Shader.PropertyToID("_StencilRef");


        public override void Create()
        {
            // _passAfterShadow = new StencilWritePass(this);
            // _passAfterShadow.needClear = true;

            _passAfterTransparent = new StencilWritePass(this);
            _passAfterTransparent.needClear = false;
        }

        public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
        {
            ref CameraData cameraData = ref renderingData.cameraData;
            if (cameraData.cameraType == CameraType.Preview ||
                cameraData.cameraType == CameraType.Reflection)
                return;
            if (!cameraData.camera.CompareTag("MainCamera")) return;

            material.SetInt(_stencilRef, stencilRef);

            // _passAfterShadow.renderPassEvent = RenderPassEvent.AfterRenderingShadows;
            // _passAfterShadow.material = material;
            // renderer.EnqueuePass(_passAfterShadow);

            _passAfterTransparent.renderPassEvent = RenderPassEvent.AfterRenderingTransparents;
            _passAfterTransparent.material = material;
            renderer.EnqueuePass(_passAfterTransparent);
        }

        protected override void Dispose(bool disposing)
        {
            rtA?.Release();
            rtB?.Release();

            _passAfterTransparent.Dispose();
            // _passAfterShadow.Dispose();
        }

        private class StencilWritePass : ScriptableRenderPass, IDisposable
        {
            public bool needClear = true;
            public Material material;
            private readonly StencilBufferRenderFeature feature;

            public StencilWritePass(StencilBufferRenderFeature feature)
            {
                this.feature = feature;
            }


            public override void OnCameraSetup(CommandBuffer cmd, ref RenderingData renderingData)
            {
                var desc = renderingData.cameraData.cameraTargetDescriptor;
                ReAllocate(desc);
            }

            private void ReAllocate(RenderTextureDescriptor desc)
            {
                desc.msaaSamples = 1; // No MSAA
                desc.depthBufferBits = (int)DepthBits.None; // No depth buffer
                // 其实一个bit就够了 因为最终也只能记录对应像素位置的Stencil是不是和stencilRef一样
                desc.colorFormat = RenderTextureFormat.R8; // GraphicsFormat.R8_UInt; // 8-bit stencil buffer
                // desc.graphicsFormat = GraphicsFormat.R8_UInt;
                if (feature.thisFrameRT == feature.rtA)
                {
                    var rt = feature.thisFrameRT;
                    RenderingUtils.ReAllocateIfNeeded(ref rt, desc, name: "StencilRT_A");
                    feature.rtA = rt;
                }
                else
                {
                    var rt = feature.thisFrameRT;
                    RenderingUtils.ReAllocateIfNeeded(ref rt, desc, name: "StencilRT_B");
                    feature.rtB = rt;
                }
            }

            public override void OnCameraCleanup(CommandBuffer cmd)
            {
            }

            public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
            {
                if (feature.thisFrameRT == null) return;
                ref var cameraData = ref renderingData.cameraData;

                if (feature.thisFrameRT.rt.width != cameraData.cameraTargetDescriptor.width ||
                    feature.thisFrameRT.rt.height != cameraData.cameraTargetDescriptor.height)
                {
                    ReAllocate(cameraData.cameraTargetDescriptor);
                }

                var cmd = CommandBufferPool.Get();
                cmd.name = $"{nameof(StencilWritePass)}";

                if (needClear)
                {
                    // Clear the stencil buffer
                    CoreUtils.SetRenderTarget(cmd, feature.thisFrameRT);
                    cmd.ClearRenderTarget(true, true, Color.clear);
                }



                CoreUtils.SetRenderTarget(cmd, feature.thisFrameRT, cameraData.renderer.cameraDepthTargetHandle,
                    ClearFlag.None,
                    Color.clear);


                cmd.DrawProcedural(Matrix4x4.identity, material, 0, MeshTopology.Triangles, 3, 1);
                context.ExecuteCommandBuffer(cmd);
                cmd.Clear();
                CommandBufferPool.Release(cmd);
            }

            public void Dispose()
            {
            }
        }

        private class ShadowWritePass : ScriptableRenderPass, IDisposable
        {
            public void Dispose()
            {
                
            }

            public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
            {
                
            }
        }
    }
}