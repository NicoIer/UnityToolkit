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
        public List<GameObject> panelList = new List<GameObject>();

        [HideInInspector, SerializeField]
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
                GameObject panel = Instantiate(value);
                if (panel.TryGetComponent(out T component))
                {
                    return component;
                }
            }

            throw new KeyNotFoundException($"{typeof(T)} has't been register in ui database");
        }

        //运行时动态添加
        public void Add(GameObject gameObject)
        {
            if (!gameObject.TryGetComponent(out IUIPanel panel)) return;
            int id = panel.GetType().FullName.GetHashCode();
            _panelDict.Add(id, gameObject);
        }


#if UNITY_EDITOR
        private void OnValidate()
        {
            _panelDict.Clear();
            List<GameObject> validPrefabs = new List<GameObject>(panelList.Count);
            foreach (GameObject uiPrefab in panelList)
            {
                if (!uiPrefab.IsPrefab())
                {
                    Debug.LogWarning($"{uiPrefab} must be prefab");
                    continue;
                }

                if (!uiPrefab.TryGetComponent(out IUIPanel panel)) continue;
                int id = panel.GetType().FullName.GetHashCode();
                _panelDict.Add(id, uiPrefab);
                validPrefabs.Add(uiPrefab);
            }

            panelList = validPrefabs;
        }
#endif
    }
}