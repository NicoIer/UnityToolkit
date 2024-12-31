#if UNITY_5_6_OR_NEWER
using System;
using System.Collections.Generic;
using UnityEngine;

namespace UnityToolkit
{
    public enum DefaultLayerConfig
    {
        Bottom = 0,
        Middle = 1,
        Default = 2,
        Popup = 3,
        Tip = 4,
        Top = 5,
    }

    /// <summary>
    /// UI面板 任意一个UI面板同时只能存在一个实例
    /// Panel是唯一的
    /// </summary>
    [RequireComponent(typeof(Canvas))]
    [DisallowMultipleComponent]
    public abstract class UIPanel : MonoBehaviour, IUIPanel
    {
        public static event Action<UIPanel> OnPanelLoaded = delegate { };
        public static event Action<UIPanel> OnPanelOpened = delegate { };
        public static event Action<UIPanel> OnPanelClosed = delegate { };
        public static event Action<UIPanel> OnPanelDisposed = delegate { };


        public Canvas canvas
        {
            get
            {
                if (_panelCanvas == null)
                {
                    _panelCanvas = GetComponent<Canvas>();
                }

                return _panelCanvas;
            }
        }

        private Canvas _panelCanvas; //用于显示面板的Canvas
#if ODIN_INSPECTOR
        [field: SerializeField, Sirenix.OdinInspector.ReadOnly]
#else
        [field: SerializeField]
#endif

        public UIPanelState state { get; internal set; } = UIPanelState.None;

        [field: SerializeField]
        public DefaultLayerConfig layerConfig { get; private set; } = DefaultLayerConfig.Default;

        [Tooltip("面板的排序顺序 越大越优先显示")]
        [field: SerializeField]
        public int sortingOrder { get; private set; }

        // protected List<IUISubPanel> _subPanels;

        GameObject IUIPanel.GetGameObject()
        {
            return gameObject;
        }

        RectTransform IUIPanel.GetRectTransform()
        {
            return transform as RectTransform;
        }

        void IUIPanel.SetState(UIPanelState state)
        {
            this.state = state;
        }

        int IUIPanel.GetSortingOrder()
        {
            return sortingOrder;
        }

        public virtual void OnLoaded()
        {
            OnPanelLoaded(this);
            // canvas.sortingOrder = sortingOrder;
        }

        public virtual void OnOpened()
        {
            gameObject.SetActive(true);
            OnPanelOpened(this);
        }

        public virtual void OnClosed()
        {
            gameObject.SetActive(false);
            OnPanelClosed(this);
        }

        public virtual void OnDispose()
        {
            Destroy(gameObject);
            OnPanelDisposed(this);
        }

        public sbyte GetLayer()
        {
            return (sbyte)layerConfig;
        }

        protected void CloseSelf()
        {
            UIRoot.Singleton.ClosePanel(GetType());
        }


        public bool IsOpened()
        {
            return state == UIPanelState.Opened;
        }

        public bool IsClosed()
        {
            return state == UIPanelState.Closed;
        }


#if UNITY_EDITOR

        // private void Reset()
        // {
        //     // sortingOrder = canvas.sortingOrder;
        //     // canvas.overrideSorting = false;
        // }
        //
        // private void OnValidate()
        // {
        //     // if (canvas.overrideSorting)
        //     // {
        //     //     UnityEditor.EditorUtility.DisplayDialog("警告", "请勿勾选Canvas的Override Sorting", "确定");
        //     // }
        //     // canvas.overrideSorting = false;
        //     // canvas.sortingOrder = sortingOrder;
        // }

#if ODIN_INSPECTOR
        [Sirenix.OdinInspector.Button]
#else
        [UnityEngine.ContextMenu("ReNamePanel")]
#endif
        public void ReNamePanel()
        {
            if (Application.isPlaying)
            {
                return;
            }

            //找到预制体的位置
            string prefabPath = UnityEditor.AssetDatabase.GetAssetPath(gameObject);
            //修改预制体的名字为类名
            gameObject.name = GetType().Name;
            UnityEditor.AssetDatabase.RenameAsset(prefabPath, GetType().Name);
        }

#endif
    }
}
#endif