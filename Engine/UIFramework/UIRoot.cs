using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.UI;

namespace UnityToolkit
{
    public partial class UIRoot : MonoSingleton<UIRoot>
    {
        [field: SerializeField] public Camera UICamera { get; private set; } //UI相机
        [field: SerializeField] public Canvas rootCanvas { get; private set; } //UI根画布
        [field: SerializeField] public CanvasScaler rootCanvasScaler { get; private set; } //UI根画布缩放器
        [field: SerializeField] public GraphicRaycaster rootGraphicRaycaster { get; private set; } //UI根画布射线检测器
        [field: SerializeField] public UIDatabase UIDatabase { get; private set; } //UI数据库
#if ODIN_INSPECTOR
        [Sirenix.OdinInspector.ShowInInspector]
        private readonly Dictionary<Type, IUIPanel> _openedPanelDict = new Dictionary<Type, IUIPanel>(); //已打开面板字典

        [Sirenix.OdinInspector.ShowInInspector]
        private readonly Dictionary<Type, IUIPanel> _closedPanelDict = new Dictionary<Type, IUIPanel>(); //已关闭面板字典

        [Sirenix.OdinInspector.ShowInInspector]
        private readonly Stack<IUIPanel> _openedPanelStack = new Stack<IUIPanel>(); //已打开面板栈
#else
        private readonly Dictionary<Type, IUIPanel> _openedPanelDict = new Dictionary<Type, IUIPanel>(); //已打开面板字典
        private readonly Dictionary<Type, IUIPanel> _closedPanelDict = new Dictionary<Type, IUIPanel>(); //已关闭面板字典
        private readonly Stack<IUIPanel> _openedPanelStack = new Stack<IUIPanel>(); //已打开面板栈
#endif

        private readonly Stack<IUIPanel> _helpStack = new Stack<IUIPanel>();
        protected override bool DontDestroyOnLoad() => true;

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

                panel.GetRectTransform().SetAsLastSibling();
                _openedPanelStack.Push(panel);
                while (_helpStack.Count > 0)
                {
                    IUIPanel origin = _helpStack.Pop();
                    origin.GetRectTransform().SetAsLastSibling();
                    _openedPanelStack.Push(origin);
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
        public void Pop()
        {
            if (_openedPanelStack.Count == 0)
            {
                return;
            }

            IUIPanel panel = _openedPanelStack.Pop();
            // Debug.Log("Pop " + panel.GetType().Name);
            panel.SetState(UIPanelState.Closing);
            _openedPanelDict.Remove(panel.GetType());
            _closedPanelDict.Add(panel.GetType(), panel);
            panel.SetState(UIPanelState.Closed);
            panel.OnClosed();
            // 放到第一个 最先 保证不会遮挡其他面板
            panel.GetRectTransform().SetAsFirstSibling();
        }

        /// <summary>
        /// 将面板弹出
        /// </summary>
        /// <param name="panel"></param>
        private void Pop(IUIPanel panel)
        {
            panel.SetState(UIPanelState.Closing);

            IUIPanel top = _openedPanelStack.Peek();
            if (top != panel)
            {
                _helpStack.Push(top);
                top = _openedPanelStack.Pop();
                // 找到目标面板
                while (top != panel && _openedPanelStack.Count > 0)
                {
                    _helpStack.Push(top);
                    top = _openedPanelStack.Pop();
                }

                while (_helpStack.Count > 0)
                {
                    _openedPanelStack.Push(_helpStack.Pop());
                }
            }
            else
            {
                _openedPanelStack.Pop();
            }

            panel.OnClosed();
            panel.SetState(UIPanelState.Closed);
            panel.GetRectTransform().SetAsFirstSibling();
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public IUIPanel Peek() => _openedPanelStack.Peek();
        

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public T OpenPanel<T>() where T : class, IUIPanel
        {
            Type type = typeof(T);
            return OpenPanel(type) as T;
        }

        public IUIPanel OpenPanel(Type type)
        {
            if (_openedPanelDict.TryGetValue(type, out IUIPanel panel))
            {
                // Debug.LogWarning("Panel already opened");
                return panel;
            }

            if (_closedPanelDict.TryGetValue(type, out panel))
            {
                _closedPanelDict.Remove(type);
                Push(panel);
                _openedPanelDict.Add(type, panel);
                return panel;
            }

            panel = UIDatabase.CreatePanel(type);
            RectTransform rectTransform = panel.GetRectTransform();
            rectTransform.SetParent(rootCanvas.transform, false); //这里的false很重要，不然会导致缩放不正确

            panel.SetState(UIPanelState.Loaded);
            panel.OnLoaded();
            Push(panel);
            _openedPanelDict.Add(type, panel);
            return panel;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void ClosePanel<T>() where T : class, IUIPanel
        {
            Type type = typeof(T);
            ClosePanel(type);
        }

        public void ClosePanel(Type type)
        {
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
                // Debug.Log("Destroy " + value.GetGameObject().name);
                Destroy(value.GetGameObject());
                return;
            }

            if (_closedPanelDict.TryGetValue(type, out value))
            {
                value.SetState(UIPanelState.Disposing);
                value.OnDispose();
                _closedPanelDict.Remove(type);
                Destroy(value.GetGameObject());
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
                Destroy(kvp.Value.GetGameObject());
            }

            _openedPanelDict.Clear();

            foreach (var kvp in _closedPanelDict)
            {
                kvp.Value.SetState(UIPanelState.Disposing);
                kvp.Value.OnDispose();
                kvp.Value.SetState(UIPanelState.Closed);
                Destroy(kvp.Value.GetGameObject());
            }

            _closedPanelDict.Clear();

            _openedPanelStack.Clear();
        }


        public bool GetOpenedPanel<T>(out T panel) where T : class, IUIPanel
        {
            Type type = typeof(T);
            if (_openedPanelDict.TryGetValue(type, out IUIPanel value))
            {
                panel = (T)value;
                return true;
            }

            panel = null;
            return false;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool IsOpen<T>() where T : class, IUIPanel
        {
            Type type = typeof(T);
            return _openedPanelDict.ContainsKey(type);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool IsOpen(Type type)
        {
            return _openedPanelDict.ContainsKey(type);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool IsClosed<T>() where T : class, IUIPanel
        {
            Type type = typeof(T);
            return _closedPanelDict.ContainsKey(type);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool IsClosed(Type type)
        {
            return _closedPanelDict.ContainsKey(type);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool IsDisposed<T>() where T : class, IUIPanel
        {
            return !IsClosed<T>() && !IsOpen<T>(); //既不在打开列表也不在关闭列表
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool IsDisposed(Type type)
        {
            return !IsClosed(type) && !IsOpen(type); //既不在打开列表也不在关闭列表
        }
    }

#if UNITY_EDITOR
    public partial class UIRoot
    {
        //在Hierarchy面板中可以快速创建一个UIRoot
        [UnityEditor.MenuItem("GameObject/UI/UIRoot", false, 0)]
        private static void CreateUIRoot()
        {
            GameObject prefab = Resources.Load<GameObject>("UIRoot");
            GameObject uiRoot = UnityEditor.PrefabUtility.InstantiatePrefab(prefab) as GameObject;
            uiRoot.name = "UIRoot";
            uiRoot.transform.SetParent(null);
            uiRoot.transform.localPosition = Vector3.zero;
            uiRoot.transform.localRotation = Quaternion.identity;
            uiRoot.transform.localScale = Vector3.one;
            UnityEditor.Selection.activeGameObject = uiRoot;

            UIRoot root = uiRoot.GetComponent<UIRoot>();
            if (root.UIDatabase == null)
            {
                root.UIDatabase = root.CreateDatabase();
                if (root.UIDatabase == null)
                {
                    Destroy(uiRoot);
                }
            }
        }

        private UIDatabase CreateDatabase()
        {
            //如果没有找到UIPanelDatabase，就创建一个
            UIDatabase = ScriptableObject.CreateInstance<UIDatabase>();
            //打开创建文件的窗口
            string path = UnityEditor.EditorUtility.SaveFilePanelInProject("Create UIPanelDatabase", "UIPanelDatabase",
                "asset", "Create UIPanelDatabase");
            UnityEditor.AssetDatabase.CreateAsset(UIDatabase, path);
            UnityEditor.AssetDatabase.SaveAssets();
            return UIDatabase;
        }


#if ODIN_INSPECTOR
        [Sirenix.OdinInspector.Button("RefreshDatabase")]
#else
        [ContextMenu("RefreshDatabase")]
#endif

        private void RefreshDatabase()
        {
            if (UIDatabase == null)
            {
                UIDatabase = CreateDatabase();
                if (UIDatabase == null)
                {
                    return;
                }
            }

            UIDatabase.Refresh();
        }
#if ODIN_INSPECTOR
        [Sirenix.OdinInspector.Button("OpenDatabase")]
#else
        [ContextMenu("OpenDatabase")]
#endif
        private void OpenDatabase() // TODO 做一个UI数据库的编辑器
        {
            throw new NotImplementedException();
        }
    }
#endif
}