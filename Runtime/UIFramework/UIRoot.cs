using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Pool;
using UnityEngine.Rendering.Universal;
using UnityEngine.UI;

namespace UnityToolkit
{
    /// <summary>
    /// UIRoot
    /// Unity Toolkit的UI框架的根节点
    /// </summary>
    public partial class UIRoot : MonoSingleton<UIRoot>
    {
        [field: SerializeField] public Camera UICamera { get; private set; } //UI相机

        [field: SerializeField] public Canvas rootCanvas { get; private set; } //UI根画布

        // [field: SerializeField] public CanvasScaler rootCanvasScaler { get; private set; } //UI根画布缩放器
        // [field: SerializeField] public GraphicRaycaster rootGraphicRaycaster { get; private set; } //UI根画布射线检测器
        [field: SerializeField] public UIDatabase UIDatabase { get; private set; } //


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

        protected override void OnInit()
        {
            rootCanvas.transform.localScale = Vector3.one;
            Camera main = Camera.main;
            if (main == null)
            {
                Debug.LogError("Main Camera is null");
                return;
            }

            if (UICamera == null)
            {
                Debug.LogError($"{nameof(UIRoot)}'s {nameof(UICamera)} is null");
                return;
            }

            var cameraData = main.GetUniversalAdditionalCameraData();
            if (!cameraData.cameraStack.Contains(UICamera))
            {
                cameraData.cameraStack.Add(UICamera);
            }
            
            // Delete the default canvas child
            for (int i = 0; i < rootCanvas.transform.childCount; i++)
            {
                Destroy(rootCanvas.transform.GetChild(i));
                Debug.LogWarning("Delete the default canvas child");
            }
            
        }

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
        public void Pop(IUIPanel panel)
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
        public IUIPanel CurTop() => Peek();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void CloseTop() => Pop();


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private T CheckPanelCache<T>() where T : IUIPanel
        {
            Type type = typeof(T);
            if (_openedPanelDict.TryGetValue(type, out IUIPanel panel) && panel is T tPanel)
            {
                // Debug.LogWarning("Panel already opened");
                return tPanel;
            }

            if (_closedPanelDict.Remove(type, out panel) && panel is T tPanel2)
            {
                Push(panel);
                _openedPanelDict.Add(type, panel);
                return tPanel2;
            }

            return default;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void BeforePanelLoaded<T>(T uiPanel) where T : IUIPanel
        {
            RectTransform rectTransform = uiPanel.GetRectTransform();
            rectTransform.SetParent(rootCanvas.transform, false); //这里的false很重要，不然会导致缩放不正确

            uiPanel.SetState(UIPanelState.Loaded);
            uiPanel.OnLoaded();
            Push(uiPanel);
        }

        public T OpenPanel<T>() where T : IUIPanel
        {
            T cache = CheckPanelCache<T>();
            if (cache != null)
            {
                return cache;
            }

            Type type = typeof(T);

            T uiPanel = UIDatabase.CreatePanel<T>();
            BeforePanelLoaded(uiPanel);
            _openedPanelDict.Add(type, uiPanel);
            return uiPanel;
        }

        public void OpenPanelAsync<T>(Action<T> callback = null) where T : IUIPanel
        {
            Type type = typeof(T);
            T cache = CheckPanelCache<T>();
            if (cache != null)
            {
                callback?.Invoke(cache);
                return;
            }

            // 或许是回调地狱 但是 不能用Task做异步 因为不一定是主线程 会GG
            UIDatabase.CreatePanelAsync<T>(uiPanel =>
            {
                BeforePanelLoaded(uiPanel);
                _openedPanelDict.Add(type, uiPanel);
                callback?.Invoke(uiPanel);
            });
        }

        public async Task<T> OpenPanelAsync<T>() where T : IUIPanel
        {
            Type type = typeof(T);
            T cache = CheckPanelCache<T>();
            if (cache != null)
            {
                return cache;
            }

            // 或许是回调地狱 但是 不能用Task做异步 因为不一定是主线程 会GG
            T uiPanel = await UIDatabase.CreatePanelAsync<T>();
            BeforePanelLoaded(uiPanel);
            _openedPanelDict.Add(type, uiPanel);
            return uiPanel;
        }


        public void ClosePanel<T>() where T : IUIPanel
        {
            Type type = typeof(T);
            ClosePanel(type);
        }

        public void CloseAllExcept<T>() where T : IUIPanel
        {
            Type type = typeof(T);
            List<Type> targets = ListPool<Type>.Get();
            foreach (var uiPanel in _openedPanelStack)
            {
                if (uiPanel.GetType() != type)
                {
                    targets.Add(uiPanel.GetType());
                }
            }

            foreach (var target in targets)
            {
                ClosePanel(target);
            }

            ListPool<Type>.Release(targets);
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
                Dispose(value.GetGameObject());
                return;
            }

            if (_closedPanelDict.TryGetValue(type, out value))
            {
                value.SetState(UIPanelState.Disposing);
                value.OnDispose();
                _closedPanelDict.Remove(type);
                Dispose(value.GetGameObject());
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
                Dispose(kvp.Value.GetGameObject());
            }

            _openedPanelDict.Clear();

            foreach (var kvp in _closedPanelDict)
            {
                kvp.Value.SetState(UIPanelState.Disposing);
                kvp.Value.OnDispose();
                kvp.Value.SetState(UIPanelState.Closed);
                Dispose(kvp.Value.GetGameObject());
            }

            _closedPanelDict.Clear();

            _openedPanelStack.Clear();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void Dispose(GameObject go)
        {
            UIDatabase.DisposePanel(go);
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
        private void OnValidate()
        {
            if (rootCanvas != null)
            {
                rootCanvas.transform.localScale = Vector3.one;
            }
        }

        //在Hierarchy面板中可以快速创建一个UIRoot
        [UnityEditor.MenuItem("GameObject/UI/UIRoot", false, 0)]
        private static void CreateUIRoot()
        {
            UIRoot root = GameObject.FindFirstObjectByType<UIRoot>();
            if (root != null)
            {
                UnityEditor.Selection.activeGameObject = root.gameObject;
                UnityEditor.EditorUtility.DisplayDialog("Create UIRoot", "UIRoot already exists", "OK");
                return;
            }

            GameObject prefab = Resources.Load<GameObject>("UnityToolkit/UIRoot");
            GameObject uiRoot = Instantiate(prefab, null, false);
            // UnityEditor.PrefabUtility.InstantiatePrefab(prefab) as GameObject;
            uiRoot.name = "UIRoot";
            uiRoot.transform.localPosition = Vector3.zero;
            uiRoot.transform.localRotation = Quaternion.identity;
            uiRoot.transform.localScale = Vector3.one;
            UnityEditor.Selection.activeGameObject = uiRoot;

            root = uiRoot.GetComponent<UIRoot>();
            if (root == null)
            {
                root = uiRoot.AddComponent<UIRoot>();
            }

            if (root.UIDatabase == null)
            {
                UIDatabase database = UnityEditor.AssetDatabase.LoadAssetAtPath<UIDatabase>("Assets/UIDatabase.asset");
                if (database == null)
                {
                    database = ScriptableObject.CreateInstance<UIDatabase>();
                    UnityEditor.AssetDatabase.CreateAsset(database, "Assets/UIDatabase.asset");
                    // 弹出MessageBox提醒用户
                    UnityEditor.EditorUtility.DisplayDialog("Create UIDatabase",
                        "Create UIDatabase.asset in Assets folder,You can move it to other folder or modify it in the editor.",
                        "OK");
                    // 选中创建的UIDatabase
                    UnityEditor.Selection.activeObject = database;
                }

                root.UIDatabase = database;
                if (UnityEditor.Selection.activeObject != database)
                {
                    UnityEditor.Selection.activeObject = root;
                }
            }

            if (root.rootCanvas == null)
            {
                Canvas canvas = uiRoot.GetComponentInChildren<Canvas>();
                root.rootCanvas = canvas;
            }

            if (root.UICamera == null)
            {
                Camera camera = uiRoot.GetComponentInChildren<Camera>();
                root.UICamera = camera;
            }
        }
    }
#endif
}