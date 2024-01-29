using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;

namespace UnityToolkit
{
    /// <summary>
    /// UI数据库
    /// 用于 存储 & 加载 UI
    /// </summary>
    [CreateAssetMenu(fileName = "UIDatabase", menuName = "Toolkit/UIDatabase", order = 0)]
    public class UIDatabase : ScriptableObject
    {
        // [Sirenix.OdinInspector.ShowInInspector]
        private Dictionary<int, GameObject> _panelDict;
        [SerializeField] private List<GameObject> _panels = new List<GameObject>();

        private void InitPanelDict()
        {
            _panelDict = new Dictionary<int, GameObject>();

            foreach (var panel in _panels)
            {
                int id = panel.GetComponent<IUIPanel>().GetType().GetHashCode();
                _panelDict.Add(id, panel);
            }
        }

        private void OnDestroy()
        {
            _panelDict = null;
        }

        /// <summary>
        /// 创建UI面板
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        /// <exception cref="KeyNotFoundException"></exception>
        public T CreatePanel<T>() where T : class, IUIPanel
        {
            if (_panelDict == null)
            {
                InitPanelDict();
            }

            int id = typeof(T).GetHashCode();
            if (!_panelDict.TryGetValue(id, out GameObject value))
                throw new KeyNotFoundException($"{typeof(T)} hasn't been register in ui database");
            if (value.GetComponent<T>() == null)
            {
                throw new ArgumentException(
                    $"UIPanel prefab:{value} doesn't contain UIPanel {typeof(T)} component");
            }

            GameObject panel = Instantiate(value);
            //修改RectTransform为填满的模式
            RectTransform rectTransform = panel.GetComponent<RectTransform>();
            rectTransform.anchorMin = Vector2.zero;
            rectTransform.anchorMax = Vector2.one;
            rectTransform.offsetMin = Vector2.zero;
            rectTransform.offsetMax = Vector2.zero;
            return panel.GetComponent<T>();
        }

        public IUIPanel CreatePanel(Type type)
        {
            if (_panelDict == null) InitPanelDict();
            int id = type.GetHashCode();
            if (_panelDict.TryGetValue(id, out GameObject value))
            {
                if (value.GetComponent(type) == null)
                {
                    throw new ArgumentException(
                        $"UIPanel prefab:{value} doesn't contain UIPanel {type} component");
                }

                GameObject panel = Instantiate(value);
                //修改RectTransform为填满的模式
                RectTransform rectTransform = panel.GetComponent<RectTransform>();
                rectTransform.anchorMin = Vector2.zero;
                rectTransform.anchorMax = Vector2.one;
                rectTransform.offsetMin = Vector2.zero;
                rectTransform.offsetMax = Vector2.zero;
                return (IUIPanel)panel.GetComponent(type);
            }

            throw new KeyNotFoundException($"{type} hasn't been register in ui database");
        }


#if UNITY_EDITOR
        // 编辑器下动态添加 校验使用
        private void OnValidate()
        {
            // 移除空的
            HashSet<int> idSet = HashSetPool<int>.Get();
            for (int i = _panels.Count - 1; i >= 0; i--)
            {
                var panelPrefab = _panels[i];
                if (panelPrefab == null)
                {
                    Debug.LogError($"UIDatabase中存在空的panel prefab");
                    _panels.RemoveAt(i);
                    continue;
                }

                if (!panelPrefab.TryGetComponent(out IUIPanel uiPanel))
                {
                    Debug.LogError($"UIDatabase中存在不包含IUIPanel的panel prefab:{panelPrefab}");
                    _panels.RemoveAt(i);
                    continue;
                }

                int id = uiPanel.GetType().GetHashCode();
                if (!idSet.Contains(id)) continue;
                Debug.LogError($"UIDatabase中存在重复的panel id:{id}");
                _panels.RemoveAt(i);
                continue;
            }
            HashSetPool<int>.Release(idSet);
        }
#endif

#if UNITY_EDITOR
        [Tooltip("刷新Database所在文件夹下的所有prefab")]
#if ODIN_INSPECTOR
        [Sirenix.OdinInspector.Button("Refresh")]
#else
        [ContextMenu("Refresh")]
#endif

        public void Refresh()
        {
            //搜索自己所在的文件夹下的所有prefab
            string path = UnityEditor.AssetDatabase.GetAssetPath(this);
            string dir = System.IO.Path.GetDirectoryName(path);
            string[] guids = UnityEditor.AssetDatabase.FindAssets("t:Prefab", new[] { dir });
            HashSet<int> ids = new HashSet<int>();
            foreach (var panel in _panels)
            {
                ids.Add(panel.GetComponent<IUIPanel>().GetType().GetHashCode());
            }

            foreach (string guid in guids)
            {
                string assetPath = UnityEditor.AssetDatabase.GUIDToAssetPath(guid);
                GameObject prefab = UnityEditor.AssetDatabase.LoadAssetAtPath<GameObject>(assetPath);
                if (prefab == null) continue; //不是prefab
                if (!prefab.TryGetComponent(out IUIPanel panel)) continue; //不包含IUIPanel
                if (_panels.Contains(prefab)) continue; //已经存在
                if (ids.Contains(panel.GetType().GetHashCode())) continue; //已经存在
                _panels.Add(prefab);
            }
        }
#endif
    }
}