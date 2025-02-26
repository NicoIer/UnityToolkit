#if UNITY_5_4_OR_NEWER
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace UnityToolkit
{
    public class StencilBufferRenderFeature : ScriptableRendererFeature
    {
        private StencilWritePass _passAfterTransparent;
        public List<Material> materials;

        internal RTHandle rtA;
        internal RTHandle rtB;
        public RTHandle thisFrameRT => Time.frameCount % 2 == 0 ? rtA : rtB;
        public RTHandle lastFrameRT => Time.frameCount % 2 == 0 ? rtB : rtA;
        

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
            

            _passAfterTransparent.renderPassEvent = RenderPassEvent.AfterRenderingTransparents;
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
            public bool needClear;
            private readonly StencilBufferRenderFeature _feature;

            public StencilWritePass(StencilBufferRenderFeature feature)
            {
                _feature = feature;
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
                desc.colorFormat = RenderTextureFormat.R8; // GraphicsFormat.R8_UInt; // 8-bit stencil buffer
                // desc.graphicsFormat = GraphicsFormat.R8_UInt;
                if (_feature.thisFrameRT == _feature.rtA)
                {
                    var rt = _feature.thisFrameRT;
                    RenderingUtils.ReAllocateIfNeeded(ref rt, desc, name: "StencilRT_A");
                    _feature.rtA = rt;
                }
                else
                {
                    var rt = _feature.thisFrameRT;
                    RenderingUtils.ReAllocateIfNeeded(ref rt, desc, name: "StencilRT_B");
                    _feature.rtB = rt;
                }
            }

            public override void OnCameraCleanup(CommandBuffer cmd)
            {
            }

            public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
            {
                if (_feature.thisFrameRT == null) return;
                ref var cameraData = ref renderingData.cameraData;

                if (_feature.thisFrameRT.rt.width != cameraData.cameraTargetDescriptor.width ||
                    _feature.thisFrameRT.rt.height != cameraData.cameraTargetDescriptor.height)
                {
                    ReAllocate(cameraData.cameraTargetDescriptor);
                }

                var cmd = CommandBufferPool.Get();
                cmd.name = $"StencilWritePass-{renderPassEvent}";

                // Clear the stencil buffer
                CoreUtils.SetRenderTarget(cmd, _feature.thisFrameRT);
                cmd.ClearRenderTarget(true, true, Color.clear);


                CoreUtils.SetRenderTarget(cmd, _feature.thisFrameRT, cameraData.renderer.cameraDepthTargetHandle,
                    ClearFlag.None,
                    Color.clear);


                foreach (var material in _feature.materials)
                {
                    cmd.DrawProcedural(Matrix4x4.identity, material, 0, MeshTopology.Triangles, 3, 1);   
                }
                context.ExecuteCommandBuffer(cmd);
                cmd.Clear();
                CommandBufferPool.Release(cmd);
            }

            public void Dispose()
            {
            }
        }
    }
}
#endif