using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Pool;

namespace UnityToolkit
{
    public partial interface IUILoader
    {
        public GameObject Load<T>() where T : IUIPanel;

        public void LoadAsync<T>(Action<GameObject> callback) where T : IUIPanel;
    }

    // TODO 删除UIDatabase 使用Json配置 路径 然后用自定义加载路径进行加载
    /// <summary>
    /// UI数据库
    /// 用于 存储 & 加载 UI
    /// </summary>
    [CreateAssetMenu(fileName = "UIDatabase", menuName = "Toolkit/UIDatabase", order = 0)]
    public partial class UIDatabase : ScriptableObject
    {
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
        }

        public IUILoader Loader = new DefaultLoader();

        public Action<GameObject> DisposeFunc = GameObject.DestroyImmediate;

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
            Loader.LoadAsync<T>(prefab =>
            {
                callback?.Invoke(Modify<T>(prefab));
            });
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private T Modify<T>(GameObject panel) where T : IUIPanel
        {
            if (panel.GetComponent<T>() == null)
            {
                throw new ArgumentException(
                    $"{nameof(UIDatabase)} doesn't contain {typeof(T)}");
            }

            //修改RectTransform为填满的模式
            RectTransform rectTransform = panel.GetComponent<RectTransform>();
            rectTransform.anchorMin = Vector2.zero;
            rectTransform.anchorMax = Vector2.one;
            rectTransform.offsetMin = Vector2.zero;
            rectTransform.offsetMax = Vector2.zero;
            return panel.GetComponent<T>();
        }

        public void DisposePanel(GameObject panel)
        {
            DisposeFunc(panel);
        }
    }
}