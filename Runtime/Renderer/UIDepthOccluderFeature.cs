using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
#if UNITY_6000_0_OR_NEWER
using UnityEngine.Rendering.RenderGraphModule;
#endif

namespace UnityToolkit
{
    public class UIDepthOccluderFeature : ScriptableRendererFeature
    {
        public Shader occluderShader;


        Material _material;
        UIDepthOccluderPass _pass;

        public override void Create()
        {
            
            if (occluderShader == null)
            {
                Debug.LogError("[UIDepthOccluderFeature] Shader not assigned.");
                return;
            }

            _material = CoreUtils.CreateEngineMaterial(occluderShader);
            _pass = new UIDepthOccluderPass(_material)
            {
                renderPassEvent = RenderPassEvent.BeforeRenderingOpaques
            };
        }

        public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
        {
            if (_material == null || _pass == null) return;

            ref var cameraData = ref renderingData.cameraData;
            if (cameraData.cameraType == CameraType.Preview ||
                cameraData.cameraType == CameraType.Reflection)
                return;
            if (!cameraData.camera.CompareTag("MainCamera")) return;
            if (UIDepthOccluder.s_ActiveOccluders.Count == 0) return;
            
            renderer.EnqueuePass(_pass);
        }

        protected override void Dispose(bool disposing)
        {
            _pass?.Dispose();
            CoreUtils.Destroy(_material);
        }
        
        class UIDepthOccluderPass : ScriptableRenderPass, System.IDisposable
        {
            readonly Material _material;

            // Fallback quad for occluders without a custom mesh
            Mesh _defaultQuad;

            public UIDepthOccluderPass(Material material)
            {
                _material = material;
            }

            Mesh GetDefaultQuad()
            {
                if (_defaultQuad != null) return _defaultQuad;
                _defaultQuad = new Mesh
                {
                    name = "UIDepthOccluder_DefaultQuad",
                    hideFlags = HideFlags.DontSave,
                    vertices = new[]
                    {
                        new Vector3(0, 0, 0),
                        new Vector3(0, 1, 0),
                        new Vector3(1, 1, 0),
                        new Vector3(1, 0, 0)
                    },
                    triangles = new[] { 0, 1, 2, 0, 2, 3 }
                };
                _defaultQuad.UploadMeshData(true);
                return _defaultQuad;
            }

#if URP_COMPATIBILITY_MODE
            public override void Configure(CommandBuffer cmd, RenderTextureDescriptor cameraTextureDescriptor)
            {
                ConfigureTarget(BuiltinRenderTextureType.CameraTarget);
            }

            [System.Obsolete]
            public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
            {
                var occluders = UIDepthOccluder.ActiveOccluders;
                if (occluders.Count == 0) return;

                var cam = renderingData.cameraData.camera;
                var cmd = CommandBufferPool.Get();
                cmd.name = "UIDepthOccluder";

                foreach (var occluder in occluders)
                {
                    if (occluder == null) continue;
                    var mesh = occluder.occluderMesh != null ? occluder.occluderMesh : GetDefaultQuad();
                    var matrix = occluder.GetLocalToViewportMatrix(cam);
                    cmd.DrawMesh(mesh, matrix, _material, 0, 0);
                }

                context.ExecuteCommandBuffer(cmd);
                cmd.Clear();
                CommandBufferPool.Release(cmd);
            }
#else
            class PassData
            {
                internal Material material;
                internal List<(Mesh mesh, Matrix4x4 matrix)> drawCalls;
            }

            // Cached to avoid GC every frame
            readonly PassData _passData = new PassData
            {
                drawCalls = new List<(Mesh, Matrix4x4)>(8)
            };

            public override void RecordRenderGraph(RenderGraph renderGraph, ContextContainer frameData)
            {
                var occluders = UIDepthOccluder.s_ActiveOccluders;
                if (occluders.Count == 0) return;

                var resourceData = frameData.Get<UniversalResourceData>();
                var cameraData = frameData.Get<UniversalCameraData>();
                var cam = cameraData.camera;

                // Reuse cached list
                _passData.drawCalls.Clear();
                _passData.material = _material;

                for (int i = 0; i < occluders.Count; i++)
                {
                    var occluder = occluders[i];
                    if (occluder == null) continue;
                    var mesh = occluder.occluderMesh != null ? occluder.occluderMesh : GetDefaultQuad();
                    var matrix = occluder.GetLocalToViewportMatrix(cam);
                    _passData.drawCalls.Add((mesh, matrix));
                }

                if (_passData.drawCalls.Count == 0) return;

                using (var builder = renderGraph.AddRasterRenderPass<PassData>("UIDepthOccluder", out var passData))
                {
                    builder.SetRenderAttachmentDepth(resourceData.activeDepthTexture, AccessFlags.Write);
                    builder.AllowPassCulling(false);

                    passData.material = _passData.material;
                    passData.drawCalls = _passData.drawCalls;

                    builder.SetRenderFunc((PassData data, RasterGraphContext context) =>
                    {
                        var cmd = context.cmd;
                        for (int i = 0; i < data.drawCalls.Count; i++)
                        {
                            var (mesh, matrix) = data.drawCalls[i];
                            cmd.DrawMesh(mesh, matrix, data.material, 0, 0);
                        }
                    });
                }
            }
#endif

            public void Dispose()
            {
                if (_defaultQuad != null)
                {
                    CoreUtils.Destroy(_defaultQuad);
                    _defaultQuad = null;
                }
            }
        }
    }
}
