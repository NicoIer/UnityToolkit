using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace UnityToolkit
{
    public class UIRoot : MonoSingleton<UIRoot>
    {
        [field: SerializeField] public Camera UICamera { get; private set; } //UI相机
        [field: SerializeField] public Canvas rootCanvas { get; private set; } //UI根画布
        [field: SerializeField] public CanvasScaler rootCanvasScaler { get; private set; } //UI根画布缩放器
        [field: SerializeField] public GraphicRaycaster rootGraphicRaycaster { get; private set; } //UI根画布射线检测器
        [field: SerializeField] public UIPanelDatabase panelDatabase { get; private set; } //UI数据库
        private readonly Dictionary<Type, IUIPanel> _openedPanelDict = new Dictionary<Type, IUIPanel>(); //已打开面板字典
        private readonly Dictionary<Type, IUIPanel> _closedPanelDict = new Dictionary<Type, IUIPanel>(); //已关闭面板字典
        private readonly Stack<IUIPanel> _openedPanelStack = new Stack<IUIPanel>(); //已打开面板栈
        private readonly Stack<IUIPanel> _helpStack = new Stack<IUIPanel>();
        public override bool dontDestroyOnLoad => true;
        /// <summary>
        /// 将面板推入栈顶
        /// </summary>
        /// <param name="panel"></param>
        private void Push(IUIPanel panel)
        {
            panel.SetState(UIPanelState.Opening);
            if (_openedPanelStack.Count > 0)
            {
                IUIPanel top = _openedPanelStack.Peek();
                //找到当前面板的显示层级
                while (top.GetSortingOrder() > panel.GetSortingOrder() && _openedPanelStack.Count > 0)
                {
                    _helpStack.Push(top);
                    top = _openedPanelStack.Pop();
                }

                _openedPanelStack.Push(panel);
                while (_helpStack.Count > 0)
                {
                    _openedPanelStack.Push(_helpStack.Pop());
                }
            }
            else
            {
                _openedPanelStack.Push(panel);
            }


            panel.SetState(UIPanelState.Opened);
            panel.OnOpened();
        }

        /// <summary>
        /// 弹出栈顶面板
        /// </summary>
        private void Pop()
        {
            if (_openedPanelStack.Count == 0)
            {
                return;
            }

            IUIPanel panel = _openedPanelStack.Pop();
            panel.SetState(UIPanelState.Closing);
            _openedPanelDict.Remove(panel.GetType());
            _closedPanelDict.Add(panel.GetType(), panel);
            panel.SetState(UIPanelState.Closed);
            panel.OnClosed();
        }

        /// <summary>
        /// 将面板弹出
        /// </summary>
        /// <param name="panel"></param>
        private void Pop(IUIPanel panel)
        {
            panel.SetState(UIPanelState.Closing);

            IUIPanel top = _openedPanelStack.Peek();
            while (top != panel && _openedPanelStack.Count > 0)
            {
                _helpStack.Push(top);
                top = _openedPanelStack.Pop();
            }

            while (_helpStack.Count > 0)
            {
                _openedPanelStack.Push(_helpStack.Pop());
            }

            panel.OnClosed();
            panel.SetState(UIPanelState.Closed);
        }

        private IUIPanel Peek()
        {
            return _openedPanelStack.Peek();
        }

        public IUIPanel CurTop()
        {
            return Peek();
        }

        public void CloseTop()
        {
            Pop();
        }

        public T OpenPanel<T>() where T : class, IUIPanel
        {
            Type type = typeof(T);
            if (_openedPanelDict.TryGetValue(type, out IUIPanel panel))
            {
                return (T)panel;
            }

            if (_closedPanelDict.TryGetValue(type, out panel))
            {
                _closedPanelDict.Remove(type);
                Push(panel);
                _openedPanelDict.Add(type, panel);
                return (T)panel;
            }

            panel = panelDatabase.CreatePanel<T>();
            RectTransform rectTransform = panel.GetRectTransform();
            //重置rectTransform
            rectTransform.SetParent(rootCanvas.transform);
            rectTransform.localPosition = Vector3.zero;
            rectTransform.localScale = Vector3.one;
            rectTransform.localRotation = Quaternion.identity;

            panel.SetState(UIPanelState.Loaded);
            panel.OnLoaded();
            Push(panel);
            _openedPanelDict.Add(type, panel);
            return (T)panel;
        }

        public void ClosePanel<T>() where T : class, IUIPanel
        {
            Type type = typeof(T);
            if (_openedPanelDict.TryGetValue(type, out IUIPanel panel))
            {
                Pop(panel);
                _openedPanelDict.Remove(type);
                _closedPanelDict.Add(type, panel);
            }
        }

        public void CloseAll()
        {
            foreach (var kvp in _openedPanelDict)
            {
                kvp.Value.SetState(UIPanelState.Closing);
                kvp.Value.OnClosed();
                kvp.Value.SetState(UIPanelState.Closed);
                _closedPanelDict.Add(kvp.Key, kvp.Value);
            }

            _openedPanelDict.Clear();
            _openedPanelStack.Clear();
        }

        public void Dispose<T>()
        {
            Type type = typeof(T);
            if (_openedPanelDict.TryGetValue(type, out var value))
            {
                Pop(value);
                value.SetState(UIPanelState.Disposing);
                value.OnDispose();
                _openedPanelDict.Remove(type);
                return;
            }

            if (_closedPanelDict.TryGetValue(type, out value))
            {
                value.SetState(UIPanelState.Disposing);
                value.OnDispose();
                _closedPanelDict.Remove(type);
            }
        }

        public void DisposeAll()
        {
            foreach (var kvp in _openedPanelDict)
            {
                kvp.Value.SetState(UIPanelState.Closing);
                kvp.Value.OnClosed();
                kvp.Value.SetState(UIPanelState.Closed);
                kvp.Value.SetState(UIPanelState.Disposing);
                kvp.Value.OnDispose();
            }

            _openedPanelDict.Clear();

            foreach (var kvp in _closedPanelDict)
            {
                kvp.Value.SetState(UIPanelState.Disposing);
                kvp.Value.OnDispose();
                kvp.Value.SetState(UIPanelState.Closed);
            }

            _closedPanelDict.Clear();

            _openedPanelStack.Clear();
        }
        
        
        #if UNITY_EDITOR
        //在Hierarchy面板中可以快速创建一个UIRoot
        [UnityEditor.MenuItem("GameObject/UI/UIRoot", false, 0)]
        public static void CreateUIRoot()
        {
            
            GameObject prefab = Resources.Load<GameObject>("UIRoot");
            GameObject uiRoot = GameObject.Instantiate(prefab, null);
            uiRoot.name = "UIRoot";
            UnityEditor.Selection.activeGameObject = uiRoot;

        }
        #endif
    }
}