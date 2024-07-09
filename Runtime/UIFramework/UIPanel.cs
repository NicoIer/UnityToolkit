using System.Collections.Generic;
using UnityEngine;

namespace UnityToolkit
{
    /// <summary>
    /// UI面板 任意一个UI面板同时只能存在一个实例
    /// Panel是唯一的
    /// </summary>
    [RequireComponent(typeof(Canvas))]
    [DisallowMultipleComponent]
    public abstract class UIPanel : MonoBehaviour, IUIPanel
    {
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

        public int sortingOrder;

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
            canvas.sortingOrder = sortingOrder;
        }

        public virtual void OnOpened()
        {
            gameObject.SetActive(true);
        }

        public virtual void OnClosed()
        {
            gameObject.SetActive(false);
        }

        public virtual void OnDispose()
        {
            Destroy(gameObject);
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

        private void Reset()
        {
            sortingOrder = canvas.sortingOrder;
            canvas.overrideSorting = true;
        }

        private void OnValidate()
        {
            canvas.overrideSorting = true;
            canvas.sortingOrder = sortingOrder;
        }
        
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