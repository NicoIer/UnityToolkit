using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;

namespace UnityToolkit
{
    // TODO 删除UIDatabase 使用Json配置 路径 然后用自定义加载路径进行加载
    /// <summary>
    /// UI数据库
    /// 用于 存储 & 加载 UI
    /// </summary>
    [CreateAssetMenu(fileName = "UIDatabase", menuName = "Toolkit/UIDatabase", order = 0)]
    public sealed class UIDatabase : ScriptableObject
    {
        public Func<Type,GameObject> LoadFunc = (type) => Resources.Load<GameObject>(type.Name);
        
        public Action<GameObject> DisposeFunc = GameObject.DestroyImmediate;

        /// <summary>
        /// 创建UI面板
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        /// <exception cref="KeyNotFoundException"></exception>
        public  T CreatePanel<T>() where T : IUIPanel
        {
            GameObject prefab = LoadFunc(typeof(T));
            // 这里是一个加载方法
            if (prefab.GetComponent<T>() == null)
            {
                throw new ArgumentException(
                    $"{nameof(UIDatabase)} doesn't contain {typeof(T)}");
            }

            GameObject panel = Instantiate(prefab);
            //修改RectTransform为填满的模式
            RectTransform rectTransform = panel.GetComponent<RectTransform>();
            rectTransform.anchorMin = Vector2.zero;
            rectTransform.anchorMax = Vector2.one;
            rectTransform.offsetMin = Vector2.zero;
            rectTransform.offsetMax = Vector2.zero;
            return panel.GetComponent<T>();
        }

        public  void DisposePanel(GameObject panel)
        { 
            DisposeFunc(panel);
        }
    }
}