using System.Collections.Generic;
using UnityToolkit;
using UnityToolkit.Editor;
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
        public List<GameObject> panelList = new List<GameObject>();

        private void OnValidate()
        {
            _panelDict.Clear();
            foreach (GameObject uiPrefab in panelList)
            {
                if (!uiPrefab.IsPrefab())
                {
                    Debug.LogWarning($"{uiPrefab} must be prefab");
                    continue;
                }

                if (!uiPrefab.TryGetComponent(out IUIPanel panel)) continue;
                int id = panel.GetType().FullName.GetHashCode();
                if (_panelDict.ContainsKey(id))
                {
                    Debug.LogWarning($"{panel.GetType()} has been register in ui database now it will be override");
                    _panelDict[id] = uiPrefab;
                    continue;
                }

                _panelDict.Add(id, uiPrefab);
            }

            panelList.Clear();

            foreach (var kvp in _panelDict)
            {
                if (kvp.Value.TryGetComponent(out IUIPanel panel))
                {
                    panelList.Add(kvp.Value);
                }
            }
        }
#endif
    }
}