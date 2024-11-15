#if UNITY_5_6_OR_NEWER
using System.Collections.Generic;
using System.Reflection;
using UnityEngine.Assertions;
using UnityEngine.Rendering.Universal;

namespace UnityToolkit
{
    public static class ScriptableRendererExtension
    {
        [Experimental]
        public static List<ScriptableRendererFeature> GetFeatureList(this ScriptableRenderer renderer)
        {
            Assert.IsNotNull(renderer);
            var propertyInfo = renderer.GetType()
                .GetProperty("rendererFeatures", BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.IsNotNull(propertyInfo);
            var rendererFeatures = (List<ScriptableRendererFeature>)propertyInfo.GetValue(renderer);
            Assert.IsNotNull(rendererFeatures);
            return rendererFeatures;
        }

        public static bool GetFirstFeature<TFeature>(this ScriptableRenderer renderer, out TFeature feature)
        {
            Assert.IsNotNull(renderer);
            var rendererFeatures = renderer.GetFeatureList();
            for (var i = 0; i < rendererFeatures.Count; i++)
            {
                if (rendererFeatures[i] is TFeature tFeature)
                {
                    feature = tFeature;
                    return true;
                }
            }

            feature = default;
            return false;
        }

        [Experimental]
        public static bool GetFeature(this ScriptableRenderer renderer, string name,
            out ScriptableRendererFeature feature)
        {
            Assert.IsNotNull(renderer);
            var rendererFeatures = renderer.GetFeatureList();
            for (var i = 0; i < rendererFeatures.Count; i++)
            {
                feature = rendererFeatures[i];
                if (feature.name == name)
                    return true;
            }

            feature = null;
            return false;
        }

        [Experimental]
        public static bool GetFeature(this ScriptableRendererData data, string name,
            out ScriptableRendererFeature feature)
        {
            Assert.IsNotNull(data);
            for (var i = 0; i < data.rendererFeatures.Count; i++)
            {
                feature = data.rendererFeatures[i];
                if (feature.name == name)
                    return true;
            }

            feature = null;
            return false;
        }

        [Experimental]
        public static void AddFeature(this ScriptableRenderer renderer, ScriptableRendererFeature feature)
        {
            Assert.IsNotNull(renderer);
            // renderer.rendererFeatures
            var propertyInfo = renderer.GetType()
                .GetProperty("rendererFeatures", BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.IsNotNull(propertyInfo);
            var rendererFeatures = propertyInfo.GetValue(renderer) as List<ScriptableRendererFeature>;
            Assert.IsNotNull(rendererFeatures);
            rendererFeatures.Add(feature);
        }

        [Experimental]
        public static void RemoveFeature(this ScriptableRenderer renderer, ScriptableRendererFeature feature)
        {
            Assert.IsNotNull(renderer);
            // renderer.rendererFeatures
            var propertyInfo = renderer.GetType()
                .GetProperty("rendererFeatures", BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.IsNotNull(propertyInfo);
            var rendererFeatures = propertyInfo.GetValue(renderer) as List<ScriptableRendererFeature>;
            Assert.IsNotNull(rendererFeatures);
            rendererFeatures.Remove(feature);
        }
    }
}

#endif