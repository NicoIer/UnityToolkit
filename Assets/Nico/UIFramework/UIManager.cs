using System;
using System.Collections.Generic;
using UnityEngine;

namespace Nico
{
    public class UIManager : GlobalSingleton<UIManager>
    {
        [field: SerializeReference] private Dictionary<Type, IUIPanel> _openedIuiPanels = new Dictionary<Type, IUIPanel>();
        [field: SerializeReference] private Dictionary<Type, IUIPanel> _closedIuiPanels = new Dictionary<Type, IUIPanel>();
        [field: SerializeReference] private readonly Dictionary<Type, GameObject> _prefabs = new Dictionary<Type, GameObject>();

        [SerializeField] private Camera uiCamera;
        [SerializeField] private RectTransform bottomLayer;
        [SerializeField] private RectTransform middleLayer;
        [SerializeField] private RectTransform topLayer;

        [SerializeField] private LayerMask uiLayerMask;

        // [SerializeField] private string panelLayerName = "UI";
        // [SerializeField] private string hiddenLayerName="HiddenUI";
        private Dictionary<UILayer, UILayerManager> _layerManagers;

        protected override void Awake()
        {
            base.Awake();

            // int panelLayer = LayerMask.NameToLayer(panelLayerName);
            // int hiddenLayer = LayerMask.NameToLayer(hiddenLayerName);

            _layerManagers = new Dictionary<UILayer, UILayerManager>
            {
                [UILayer.Bottom] = new UILayerManager(bottomLayer), //,  panelLayer, hiddenLayer);
                [UILayer.Middle] = new UILayerManager(middleLayer), //,  panelLayer, hiddenLayer);
                [UILayer.Top] = new UILayerManager(topLayer) //,  panelLayer,  hiddenLayer);
            };

            _openedIuiPanels = new Dictionary<Type, IUIPanel>();
            _closedIuiPanels = new Dictionary<Type, IUIPanel>();

            // _uiCamera = transform.Find("UICamera").GetComponent<Camera>();
            uiCamera.cullingMask = uiLayerMask;
            
        }

        public void Register<T>(GameObject prefab) where T : MonoBehaviour, IUIPanel
        {
            prefab.AddComponent<T>();
            Register(prefab);
        }

        public void Register(GameObject prefab)
        {
            IUIPanel panel = prefab.GetComponent<IUIPanel>();
            if (panel == null)
            {
                Debug.LogError($"can't find IUIPanel in prefab:{prefab.name}");
                return;
            }

            _prefabs[panel.GetType()] = prefab;
        }

        public T OpenUI<T>() where T : class, IUIPanel
        {
            //已经打开了
            if (_openedIuiPanels.TryGetValue(typeof(T), out IUIPanel panel))
            {
                // panel.OnShow();
                _layerManagers[panel.Layer()].Push(panel);
                return panel as T;
            }

            //已经关闭了
            if (_closedIuiPanels.TryGetValue(typeof(T), out IUIPanel panel1))
            {
                Debug.Log("reopen ui");
                _openedIuiPanels[typeof(T)] = panel1;
                _closedIuiPanels.Remove(typeof(T));
                _layerManagers[panel1.Layer()].Push(panel1);
                return panel1 as T;
            }

            //创建
            if (Create(out T panel2))
            {
                _layerManagers[panel2.Layer()].Push(panel2);
                _openedIuiPanels.Add(typeof(T), panel2);
                return panel2;
            }

            throw new ArgumentException($"UIManager OpenUI<{typeof(T)}> Error");
        }

        public void CloseUI<T>() where T : IUIPanel
        {
            Type type = typeof(T);
            if (_openedIuiPanels.TryGetValue(type, out IUIPanel iuiPanel))
            {
                // Debug.Log("close ui");
                _layerManagers[iuiPanel.Layer()].Remove(iuiPanel);
                _openedIuiPanels.Remove(type);
                // 8.5 test func
                if (iuiPanel.DestroyOnHide())
                {
                    Destroy(iuiPanel);
                    return;
                }
                
                _closedIuiPanels.Add(type, iuiPanel);
                return;
            }

            Debug.LogWarning($"not such window:{type} need to close");
        }

        private bool Create<T>(out T panel) where T : IUIPanel
        {
            GameObject prefab = _prefabs[typeof(T)];
            UILayer layer = prefab.GetComponent<T>().Layer();
            Transform tar;
            switch (layer)
            {
                case UILayer.Top:
                    tar = topLayer;
                    break;
                case UILayer.Middle:
                    tar = middleLayer;
                    break;
                case UILayer.Bottom:
                    tar = bottomLayer;
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            GameObject uiObj = GameObject.Instantiate(prefab, tar, false); //UILayer layer
            panel = uiObj.GetComponent<T>();
            panel.OnCreate();
            return true;
        }

        private void Destroy(IUIPanel panel)
        {
            //销毁打开的窗口
            if (_openedIuiPanels.ContainsKey(panel.GetType()))
            {
                _layerManagers[panel.Layer()].Remove(panel);
                _openedIuiPanels.Remove(panel.GetType());
            }

            if (_closedIuiPanels.ContainsKey(panel.GetType()))
            {
                _closedIuiPanels.Remove(panel.GetType());
            }

            UnityEngine.GameObject.Destroy(panel.GetGameObject());
        }

        public void Destroy<T>() where T : IUIPanel
        {
            if (_openedIuiPanels.ContainsKey(typeof(T)))
            {
                Destroy(_openedIuiPanels[typeof(T)]);
                return;
            }

            if (_closedIuiPanels.ContainsKey(typeof(T)))
            {
                Destroy(_closedIuiPanels[typeof(T)]);
            }
        }

        public bool Opened<T>() where T : IUIPanel
        {
            return _openedIuiPanels.ContainsKey(typeof(T));
        }

        public bool Get<T>(out T panel) where T : class, IUIPanel
        {
            bool ans = _openedIuiPanels.TryGetValue(typeof(T), out IUIPanel p);
            panel = p as T;
            return ans;
        }

        public void Pop(UILayer layer)
        {
            if (_layerManagers[layer].Pop(out IUIPanel window))
            {
                _openedIuiPanels.Remove(window.GetType());
                _closedIuiPanels.Add(window.GetType(), window);
                return;
            }

            Debug.LogWarning("not window to pop");
        }

        public void CloseAll()
        {
            _layerManagers[UILayer.Bottom].RemoveAll();
            _layerManagers[UILayer.Middle].RemoveAll();
            _layerManagers[UILayer.Top].RemoveAll();
            foreach (var kvp in _openedIuiPanels)
            {
                _closedIuiPanels.Add(kvp.Key, kvp.Value);
            }

            _openedIuiPanels.Clear();
        }

        public void DestroyAll()
        {
            _layerManagers[UILayer.Bottom].RemoveAll();
            _layerManagers[UILayer.Middle].RemoveAll();
            _layerManagers[UILayer.Top].RemoveAll();
            _openedIuiPanels.Clear();
            _closedIuiPanels.Clear();
        }
    }
}