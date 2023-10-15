using System;
using System.Collections.Generic;
using UnityEngine;

namespace UnityToolkit
{
    /// <summary>
    /// UI数据库
    /// 用于 存储 & 加载 UI
    /// </summary>
    [CreateAssetMenu(fileName = "UIDatabase", menuName = "Toolkit/UIDatabase", order = 0)]
    public class UIDatabase : ScriptableObject
    {
        [SerializeField]
        private SerializableDictionary<int, GameObject> _panelDict = new SerializableDictionary<int, GameObject>();

        /// <summary>
        /// 创建UI面板
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        /// <exception cref="KeyNotFoundException"></exception>
        public T CreatePanel<T>() where T : class, IUIPanel
        {
            int id = TypeId<T>.HashId;
            if (_panelDict.TryGetValue(id, out GameObject value))
            {
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

            throw new KeyNotFoundException($"{typeof(T)} hasn't been register in ui database");
        }


        //运行时动态添加
        public void Add(GameObject gameObject)
        {
            if (!gameObject.TryGetComponent(out IUIPanel panel)) return;
            int id = panel.GetType().FullName.GetHashCode();

            _panelDict[id] = gameObject;
        }

#if UNITY_EDITOR
        private readonly List<int> _removeIds = new List<int>();

        private readonly List<GameObject> _addGameObjects = new List<GameObject>();

        // 编辑器下动态添加 校验使用
        private void OnValidate()
        {
            _removeIds.Clear();
            _addGameObjects.Clear();
            foreach (var (key, uiPrefab) in _panelDict)
            {
                if (uiPrefab == null)
                {
                    _removeIds.Add(key);
                    continue;
                }

                if (uiPrefab.TryGetComponent(out IUIPanel panel))
                {
                    int id = panel.GetType().FullName.GetHashCode();
                    if (id != key)
                    {
                        _removeIds.Add(key);
                        _addGameObjects.Add(uiPrefab);
                    }
                }
            }

            foreach (var removeId in _removeIds)
            {
                _panelDict.Remove(removeId);
            }

            foreach (var gameObject in _addGameObjects)
            {
                Add(gameObject);
            }
        }
#endif

#if ODIN_INSPECTOR && UNITY_EDITOR
        [Tooltip("刷新Database所在文件夹下的所有prefab")]
        [Sirenix.OdinInspector.Button("Refresh")]
        public void Refresh()
        {
            //搜索自己所在的文件夹下的所有prefab
            string path = UnityEditor.AssetDatabase.GetAssetPath(this);
            string dir = System.IO.Path.GetDirectoryName(path);
            string[] guids = UnityEditor.AssetDatabase.FindAssets("t:Prefab", new[] { dir });
            foreach (string guid in guids)
            {
                string assetPath = UnityEditor.AssetDatabase.GUIDToAssetPath(guid);
                GameObject prefab = UnityEditor.AssetDatabase.LoadAssetAtPath<GameObject>(assetPath);
                if (prefab.TryGetComponent(out IUIPanel panel))
                {
                    _panelDict[panel.GetType().FullName.GetHashCode()] = prefab;
                }
            }
        }
#endif
    }
}