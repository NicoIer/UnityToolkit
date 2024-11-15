using System;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace UnityToolkit
{
    public class StencilBufferRenderFeature : ScriptableRendererFeature
    {
        [Range(0, 255)] public int stencilRef;
        public RenderPassEvent passEvent = RenderPassEvent.AfterRenderingTransparents;
        private StencilWritePass _pass;
        public Material material;

        public RTHandle thisFrameRT => _pass.thisFrameRT;
        public RTHandle lastFrameRT => _pass.lastFrameRT;

        private static readonly int _stencilRef = Shader.PropertyToID("_StencilRef");

        
        public override void Create()
        {
            _pass = new StencilWritePass();
        }

        public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
        {
            ref CameraData cameraData = ref renderingData.cameraData;
            if (cameraData.cameraType == CameraType.Preview ||
                cameraData.cameraType == CameraType.Reflection)
                return;
            if (!cameraData.camera.CompareTag("MainCamera")) return;
            material.SetInt(_stencilRef, stencilRef);
            _pass.renderPassEvent = passEvent;
            _pass.material = material;
            renderer.EnqueuePass(_pass);
        }

        protected override void Dispose(bool disposing)
        {
            _pass.Dispose();
        }
        private class StencilWritePass : ScriptableRenderPass, IDisposable
        {
            public Material material;
            private RTHandle _rtA;
            private RTHandle _rtB;
            public RTHandle thisFrameRT => Time.frameCount % 2 == 0 ? _rtA : _rtB;
            public RTHandle lastFrameRT => Time.frameCount % 2 == 0 ? _rtB : _rtA;

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
                if (thisFrameRT == _rtA)
                {
                    RenderingUtils.ReAllocateIfNeeded(ref _rtA, desc, name: "StencilRT_A");
                }
                else
                {
                    RenderingUtils.ReAllocateIfNeeded(ref _rtB, desc, name: "StencilRT_B");
                }
            }

            public override void OnCameraCleanup(CommandBuffer cmd)
            {
            }

            public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
            {
                if (thisFrameRT == null) return;
                ref var cameraData = ref renderingData.cameraData;

                if (thisFrameRT.rt.width != cameraData.cameraTargetDescriptor.width ||
                    thisFrameRT.rt.height != cameraData.cameraTargetDescriptor.height)
                {
                    ReAllocate(cameraData.cameraTargetDescriptor);
                }

                var cmd = CommandBufferPool.Get();
                cmd.name = "StencilBuffer";
                
                // Clear the stencil buffer
                CoreUtils.SetRenderTarget(cmd, thisFrameRT);
                cmd.ClearRenderTarget(true, true, Color.clear);
                
                CoreUtils.SetRenderTarget(cmd, thisFrameRT, cameraData.renderer.cameraDepthTargetHandle, ClearFlag.None,
                    Color.clear);
                cmd.DrawProcedural(Matrix4x4.identity, material, 0, MeshTopology.Triangles, 3, 1);
                context.ExecuteCommandBuffer(cmd);
                cmd.Clear();
                CommandBufferPool.Release(cmd);
            }

            public void Dispose()
            {
                _rtA?.Release();
                _rtB?.Release();
            }
        }
    }
}