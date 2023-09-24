using System.Collections.Generic;
using Nico;
using Nico.Editor;
using UnityEngine;

namespace UnityToolkit
{
    /// <summary>
    /// UI数据库
    /// 用于 存储 & 加载 UI
    /// </summary>
    [CreateAssetMenu(fileName = "UIPanelDatabase", menuName = "Toolkit/UIPanelDatabase", order = 0)]
    public class UIPanelDatabase : ScriptableObject
    {
        [HideInInspector, SerializeField]
        private SerializableDictionary<int, GameObject> _uiPrefabs = new SerializableDictionary<int, GameObject>();

        /// <summary>
        /// 创建UI面板
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        /// <exception cref="KeyNotFoundException"></exception>
        public T CreatePanel<T>() where T : class, IUIPanel
        {
            int id = TypeId<T>.HashId;
            if (_uiPrefabs.TryGetValue(id, out GameObject value))
            {
                GameObject panel = Instantiate(value);
                if (panel.TryGetComponent(out T component))
                {
                    return component;
                }
            }

            throw new KeyNotFoundException($"{typeof(T)} has't been register in ui database");
        }

#if UNITY_EDITOR
        [SerializeField] private List<GameObject> _panelPrefabs = new List<GameObject>();

        private void OnValidate()
        {
            _uiPrefabs.Clear();
            List<GameObject> validPrefabs = new List<GameObject>(_panelPrefabs.Count);
            foreach (GameObject uiPrefab in _panelPrefabs)
            {
                if (!uiPrefab.IsPrefab())
                {
                    Debug.LogWarning($"{uiPrefab} must be prefab");
                    continue;
                }

                if (!uiPrefab.TryGetComponent(out IUIPanel panel)) continue;
                int id = panel.GetType().FullName.GetHashCode();
                _uiPrefabs.Add(id, uiPrefab);
                validPrefabs.Add(uiPrefab);
            }

            _panelPrefabs = validPrefabs;
        }
#endif
    }
}