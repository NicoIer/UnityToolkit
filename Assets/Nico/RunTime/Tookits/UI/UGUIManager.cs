using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.Serialization;

namespace Nico.UI
{
    public sealed class UGUIManager : GlobalSingleton<UGUIManager>
    {
        public Camera UICamera { get; private set; }
        private Dictionary<Type, IUIWindow> closedWindows; //已经关闭的窗口
        private Dictionary<Type, IUIWindow> openedWindows; //已经打开的窗口
        private Dictionary<Type, Func<IUIWindow>> createFuncs;
        private LayerMask uiLayerMask;
        private RectTransform bottomLayer;
        private RectTransform middleLayer;
        private RectTransform topLayer;
        private RectTransform _hiddenLayer;


        internal Dictionary<UGUILayer, UGUILayerManager> layerManagers;

        protected override void Awake()
        {
            base.Awake();
            uiLayerMask = LayerMask.NameToLayer("UI");
            layerManagers = new Dictionary<UGUILayer, UGUILayerManager>();
            bottomLayer = transform.Find("Canvas/BottomLayer").GetComponent<RectTransform>();
            middleLayer = transform.Find("Canvas/MiddleLayer").GetComponent<RectTransform>();
            topLayer = transform.Find("Canvas/TopLayer").GetComponent<RectTransform>();
            _hiddenLayer = transform.Find("Canvas/HiddenLayer").GetComponent<RectTransform>();
            layerManagers[UGUILayer.Bottom] = new UGUILayerManager(bottomLayer, _hiddenLayer);
            layerManagers[UGUILayer.Middle] = new UGUILayerManager(middleLayer, _hiddenLayer);
            layerManagers[UGUILayer.Top] = new UGUILayerManager(topLayer, _hiddenLayer);


            openedWindows = new Dictionary<Type, IUIWindow>();
            closedWindows = new Dictionary<Type, IUIWindow>();

            UICamera = transform.Find("UICamera").GetComponent<Camera>();
            //设置UI摄像机的渲染层级
            UICamera.cullingMask = 1 << uiLayerMask;
            
            
        }

        /// <summary>
        /// 将UI注册到UI管理器中
        /// </summary>
        /// <param name="prefab">UI对应的预制体</param>
        /// <typeparam name="T">UI对应的控制类</typeparam>
        public void Register<T>(GameObject prefab) where T : IUIComponent, new()
        {
            UIFactory<T>.createObj = () =>
            {
                GameObject obj = GameObject.Instantiate(prefab);
                obj.layer = uiLayerMask;
                return obj;
            };
            UIFactory<T>.createCs = () => new T();

        }

        /// <summary>
        /// 实例化UI
        /// </summary>
        /// <param name="t"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        internal bool Create<T>(out T t) where T : IUIWindow
        {
            if (UIFactory<T>.createObj == null)
            {
                t = default;
                return false;
            }

            GameObject obj = UIFactory<T>.createObj();
            obj.name = typeof(T).Name;
            t = UIFactory<T>.createCs();
            t.Bind(obj);
            t.OnInit();
            return true;
        }

        /// <summary>
        /// 销毁UI
        /// </summary>
        /// <param name="window"></param>
        /// <returns></returns>
        internal void Destroy(IUIWindow window)
        {
            //销毁打开的窗口
            if (openedWindows.ContainsKey(window.GetType()))
            {
                layerManagers[window.Layer()].Remove(window);
                openedWindows.Remove(window.GetType());
            }

            if (closedWindows.ContainsKey(window.GetType()))
            {
                closedWindows.Remove(window.GetType());
            }

            UnityEngine.GameObject.Destroy(window.gameObject);
        }

        public void Destroy<T>() where T : IUIWindow
        {
            if (openedWindows.ContainsKey(typeof(T)))
            {
                Destroy(openedWindows[typeof(T)]);
                return;
            }

            if (closedWindows.ContainsKey(typeof(T)))
            {
                Destroy(closedWindows[typeof(T)]);
                return;
            }
        }

        public bool Opened<T>() where T : IUIWindow
        {
            return openedWindows.ContainsKey(typeof(T));
        }

        public bool Get<T>(out T window) where T : IUIWindow
        {
            bool ans = openedWindows.TryGetValue(typeof(T), out IUIWindow value);

            window = value is T iuiWindow ? iuiWindow : default;
            return ans;
        }

        public WindowState GetState<T>() where T : IUIWindow
        {
            Type type = typeof(T);
            if (openedWindows.ContainsKey(type))
            {
                return WindowState.Opened;
            }

            if (closedWindows.ContainsKey(type))
            {
                return WindowState.None;
            }

            //有对应的创建方法,但是没有打开过,那么就是没有加载过
            if (UIFactory<T>.createObj != null)
            {
                return WindowState.UnLoaded;
            }

            //
            return WindowState.None;
        }

        public void Open<T>() where T : IUIWindow
        {
            if (openedWindows.ContainsKey(typeof(T)))
            {
                Debug.LogWarning($"you have opened this window: {typeof(T)}");
                return;
            }

            //如果没有打开,且这个窗口是创建过的,但是被关闭了,那么就从关闭的窗口中取出来
            if (closedWindows.TryGetValue(typeof(T), out IUIWindow window))
            {
                //Push到对应的Layer中
                layerManagers[window.Layer()].Push(window);
                openedWindows.Add(typeof(T), window);
                closedWindows.Remove(typeof(T));
                return;
            }

            //既没有打开也没有关闭,那么就创建一个
            if (Create<T>(out T newWindow))
            {
                layerManagers[newWindow.Layer()].Push(newWindow);
                openedWindows.Add(typeof(T), newWindow);
                return;
            }

            throw new ArgumentException($"window:{typeof(T).Name} must be register first!");
        }

        public void Close<T>() where T : IUIWindow
        {
            Type type = typeof(T);
            if (openedWindows.TryGetValue(type, out var window))
            {
                layerManagers[window.Layer()].Remove(window);
                openedWindows.Remove(type);
                closedWindows.Add(type, window);
                return;
            }

            Debug.LogWarning($"not such window:{type} need to close");
        }

        public void Pop()
        {
            if (layerManagers[UGUILayer.Top].HasWindow)
            {
                Pop(UGUILayer.Top);
                return;
            }

            if (layerManagers[UGUILayer.Middle].HasWindow)
            {
                Pop(UGUILayer.Middle);
                return;
            }

            if (layerManagers[UGUILayer.Bottom].HasWindow)
            {
                Pop(UGUILayer.Bottom);
                return;
            }
        }

        public void Pop(UGUILayer layer)
        {
            if (layerManagers[layer].Pop(out IUIWindow window))
            {
                openedWindows.Remove(window.GetType());
                closedWindows.Add(window.GetType(), window);
                return;
            }

            Debug.LogWarning("not window to pop");
        }
    }
}