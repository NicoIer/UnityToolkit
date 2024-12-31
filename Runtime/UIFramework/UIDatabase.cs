#if UNITY_5_6_OR_NEWER
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Pool;
using UnityEngine.Serialization;

namespace UnityToolkit
{
    public partial interface IUILoader
    {
        public GameObject Load<T>() where T : IUIPanel;

        public void LoadAsync<T>(Action<GameObject> callback) where T : IUIPanel;

        public Task<GameObject> LoadAsync<T>() where T : IUIPanel;

        public void Dispose(GameObject panel);
    }

    [Serializable]
    public struct UILayerConfig
    {
        /// <summary>
        /// 这个层的名字
        /// </summary>
        public string name;

        /// <summary>
        /// 越大 离相机越近 在Hierarchy中越靠后 越后画 越优先显示
        /// </summary>
        public sbyte order;
        
        public UILayerConfig(string name, sbyte order)
        {
            this.name = name;
            this.order = order;
        }
    }

    /// <summary>
    /// UI数据库
    /// 用于 存储 & 加载 UI
    /// </summary>
    [CreateAssetMenu(fileName = "UIDatabase", menuName = "Toolkit/UIDatabase", order = 0)]
    public partial class UIDatabase : ScriptableObject
    {
        public float distanceBetweenLayers = 100;

        public List<UILayerConfig> LayerConfig = new List<UILayerConfig>()
        {
            new UILayerConfig("Bottom", 0),
            new UILayerConfig("Middle", 1),
            new UILayerConfig("Default", 2),
            new UILayerConfig("Popup", 3),
            new UILayerConfig("Tip", 4),
            new UILayerConfig("Top", 5),
        };
        private struct DefaultLoader : IUILoader
        {
            public GameObject Load<T>() where T : IUIPanel
            {
                return Resources.Load<GameObject>(typeof(T).Name);
            }

            public void LoadAsync<T>(Action<GameObject> callback) where T : IUIPanel
            {
                var handle = Resources.LoadAsync<GameObject>(typeof(T).Name);
                handle.completed += operation => { callback(handle.asset as GameObject); };
            }

            public async Task<GameObject> LoadAsync<T>() where T : IUIPanel
            {
                TaskCompletionSource<GameObject> tcs = new TaskCompletionSource<GameObject>();
                var handle = Resources.LoadAsync<GameObject>(typeof(T).Name);
                tcs.SetResult(handle.asset as GameObject);
                await tcs.Task;
                return tcs.Task.Result;
            }

            public void Dispose(GameObject panel)
            {
                Destroy(panel);
            }
        }

        public IUILoader Loader = new DefaultLoader();


        /// <summary>
        /// 创建UI面板
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        /// <exception cref="KeyNotFoundException"></exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public T CreatePanel<T>() where T : IUIPanel
        {
            GameObject prefab = Loader.Load<T>();
            return Modify<T>(prefab);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void CreatePanelAsync<T>(Action<T> callback) where T : IUIPanel
        {
            Loader.LoadAsync<T>(prefab => { callback?.Invoke(Modify<T>(prefab)); });
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public async Task<T> CreatePanelAsync<T>() where T : IUIPanel
        {
            GameObject prefab = await Loader.LoadAsync<T>();
            return Modify<T>(prefab);
        }

        // [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private T Modify<T>(GameObject prefab) where T : IUIPanel
        {
            if (prefab.GetComponent<T>() == null)
            {
                throw new ArgumentException(
                    $"{nameof(UIDatabase)} doesn't contain {typeof(T)}");
            }

            GameObject go = Instantiate(prefab);

            //修改RectTransform为填满的模式
            RectTransform rectTransform = go.GetComponent<RectTransform>();
            rectTransform.anchorMin = Vector2.zero;
            rectTransform.anchorMax = Vector2.one;
            rectTransform.offsetMin = Vector2.zero;
            rectTransform.offsetMax = Vector2.zero;
            return go.GetComponent<T>();
        }

        public void DisposePanel(GameObject panel)
        {
            Loader.Dispose(panel);
        }

#if UNITY_EDITOR
#if ODIN_INSPECTOR_3
        [Sirenix.OdinInspector.Button]
        public void ResetToDefault()
        {
            distanceBetweenLayers = 100;
            LayerConfig = new List<UILayerConfig>()
            {
                new UILayerConfig("Bottom", 0),
                new UILayerConfig("Middle", 1),
                new UILayerConfig("Default", 2),
                new UILayerConfig("Popup", 3),
                new UILayerConfig("Tip", 4),
                new UILayerConfig("Top", 5),
            };
            UnityEditor.EditorUtility.SetDirty(this);
        }
#endif
#endif
    }
}
#endif