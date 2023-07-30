using System;
using System.Collections.Generic;
using Nico;
using UnityEngine;

namespace Nico
{
    public class UIManager : GlobalSingleton<UIManager>
    {
        [field: SerializeReference] public Dictionary<Type, UIPanel> openedUIPanels = new Dictionary<Type, UIPanel>();
        [field: SerializeReference] public Dictionary<Type, UIPanel> closedUIPanels = new Dictionary<Type, UIPanel>();
        [field: SerializeReference] public Dictionary<Type, GameObject> prefabs = new Dictionary<Type, GameObject>();
        [SerializeField] private Canvas _canvas;
        [SerializeField] private Camera _uiCamera;
        private RectTransform bottomLayer;
        private RectTransform middleLayer;
        private RectTransform topLayer;
        private RectTransform _hiddenLayer;
        private LayerMask uiLayerMask;
        private Dictionary<UILayer, UILayerManager> _layerManagers;

        protected override void Awake()
        {
            base.Awake();
            uiLayerMask = LayerMask.NameToLayer("UI");
            _layerManagers = new Dictionary<UILayer, UILayerManager>();
            bottomLayer = transform.Find("Canvas/BottomLayer").GetComponent<RectTransform>();
            middleLayer = transform.Find("Canvas/MiddleLayer").GetComponent<RectTransform>();
            topLayer = transform.Find("Canvas/TopLayer").GetComponent<RectTransform>();
            _hiddenLayer = transform.Find("Canvas/HiddenLayer").GetComponent<RectTransform>();

            _layerManagers[UILayer.Bottom] = new UILayerManager(bottomLayer, _hiddenLayer);
            _layerManagers[UILayer.Middle] = new UILayerManager(middleLayer, _hiddenLayer);
            _layerManagers[UILayer.Top] = new UILayerManager(topLayer, _hiddenLayer);

            openedUIPanels = new Dictionary<Type, UIPanel>();
            closedUIPanels = new Dictionary<Type, UIPanel>();

            _uiCamera = transform.Find("UICamera").GetComponent<Camera>();
            _uiCamera.cullingMask = 1 << uiLayerMask;
        }

        public void Register<T>(GameObject prefab) where T : UIPanel
        {
            prefab.AddComponent<T>();
            Register(prefab);
        }

        public void Register(GameObject prefab)
        {
            UIPanel panel = prefab.GetComponent<UIPanel>();
            if (panel == null)
            {
                Debug.LogError($"can't find UIPanel in prefab:{prefab.name}");
                return;                
            }
            prefabs[panel.GetType()] = prefab;

        }

        public T OpenUI<T>() where T : UIPanel
        {
            //已经打开了
            if (openedUIPanels.TryGetValue(typeof(T), out UIPanel panel))
            {
                // panel.OnShow();
                _layerManagers[panel.Layer()].Push(panel);
                return panel as T;
            }

            //已经关闭了
            if (closedUIPanels.TryGetValue(typeof(T), out UIPanel panel1))
            {
                openedUIPanels[typeof(T)] = panel1;
                closedUIPanels.Remove(typeof(T));
                _layerManagers[panel1.Layer()].Push(panel1);
                return panel1 as T;
            }

            //创建
            if (Create<T>(out T panel2))
            {
                _layerManagers[panel2.Layer()].Push(panel2);
                openedUIPanels.Add(typeof(T), panel2);
                return panel2;
            }

            throw new ArgumentException($"UIManager OpenUI<{typeof(T)}> Error");
        }

        public void CloseUI<T>() where T : UIPanel
        {
            Type type = typeof(T);
            if (openedUIPanels.TryGetValue(type, out var uiPanel))
            {
                Debug.Log("close ui");
                _layerManagers[uiPanel.Layer()].Remove(uiPanel);
                openedUIPanels.Remove(type);
                closedUIPanels.Add(type, uiPanel);
                return;
            }

            Debug.LogWarning($"not such window:{type} need to close");
        }

        private bool Create<T>(out T panel) where T : UIPanel
        {
            GameObject prefab = prefabs[typeof(T)];
            GameObject uiObj = GameObject.Instantiate(prefab, _canvas.transform);
            panel = uiObj.GetComponent<T>();
            panel.OnCreate();
            return true;
        }

        public void Destroy(UIPanel panel)
        {
            //销毁打开的窗口
            if (openedUIPanels.ContainsKey(panel.GetType()))
            {
                _layerManagers[panel.Layer()].Remove(panel);
                openedUIPanels.Remove(panel.GetType());
            }

            if (closedUIPanels.ContainsKey(panel.GetType()))
            {
                closedUIPanels.Remove(panel.GetType());
            }

            UnityEngine.GameObject.Destroy(panel.gameObject);
        }

        public void Destroy<T>() where T : UIPanel
        {
            if (openedUIPanels.ContainsKey(typeof(T)))
            {
                Destroy(openedUIPanels[typeof(T)]);
                return;
            }

            if (closedUIPanels.ContainsKey(typeof(T)))
            {
                Destroy(closedUIPanels[typeof(T)]);
                return;
            }
        }

        public bool Opened<T>() where T : UIPanel
        {
            return openedUIPanels.ContainsKey(typeof(T));
        }

        public bool Get<T>(out T panel) where T : UIPanel
        {
            bool ans = openedUIPanels.TryGetValue(typeof(T), out UIPanel p);
            panel = p as T;
            return ans;
        }

        public void Pop(UILayer layer)
        {
            if (_layerManagers[layer].Pop(out UIPanel window))
            {
                openedUIPanels.Remove(window.GetType());
                closedUIPanels.Add(window.GetType(), window);
                return;
            }

            Debug.LogWarning("not window to pop");
        }

        public void CloseAll()
        {
            _layerManagers[UILayer.Bottom].RemoveAll();
            _layerManagers[UILayer.Middle].RemoveAll();
            _layerManagers[UILayer.Top].RemoveAll();
            openedUIPanels.Clear();
            closedUIPanels.Clear();
        }
    }
}