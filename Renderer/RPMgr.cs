#if UNITY_5_6_OR_NEWER
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace UnityToolkit
{

    public class VolumeMgr : MonoSingleton<VolumeMgr>,IAutoCreateSingleton
    {
        
        public Volume global { get; private set; }
        protected override void OnInit()
        {
            gameObject.hideFlags = HideFlags.HideInHierarchy;
            var volumes = VolumeManager.instance.GetVolumes(LayerMask.GetMask("Default"));
            foreach (var volume in volumes)
            {
                if (volume.isGlobal)
                {
                    global = volume;
                    break;
                }
            }

            if (global == null)
            {
                Debug.LogWarning($"nicoier  找不到Global Volume 是否存在时序问题");
            }
        }
    }
    public class RPMgr : MonoSingleton<RPMgr>, IAutoCreateSingleton
    {
        Dictionary<ScriptableRenderer, List<ScriptableRendererFeature>> _rfCache = new();


        protected override bool DontDestroyOnLoad()
        {
            return true;
        }

        public ScriptableRendererData[] RendererDataList { get; private set; }

        protected override void OnInit()
        {
            gameObject.hideFlags = HideFlags.HideInHierarchy;
            var asset = UniversalRenderPipeline.asset;
            var proInfo = typeof(UniversalRenderPipelineAsset).GetField("m_RendererDataList", BindingFlags.NonPublic | BindingFlags.Instance);
            if (proInfo == null) 
                return;
            
            RendererDataList = proInfo.GetValue(asset) as ScriptableRendererData[];
        }

        public static TRendererFeature Get<TRendererFeature>(int idx, string name) where TRendererFeature : ScriptableRendererFeature
        {
            var asset = UniversalRenderPipeline.asset;
            var renderer = asset.GetRenderer(idx);
            var features = GetFeatureList(renderer);
            foreach (var feature in features)
            {
                if(feature == null)
                {
                    continue;
                }
                if (feature is TRendererFeature tFeature && feature.name == name)
                {
                    return tFeature;
                }
            }

            return default;
        }


        private static List<ScriptableRendererFeature> GetFeatureList(ScriptableRenderer renderer)
        {
            if (Singleton._rfCache.TryGetValue(renderer, out var features))
            {
                return features;
            }

            features = renderer.GetFeatureList();
            Singleton._rfCache.Add(renderer, features);
            return features;
        }
    }
}
#endif