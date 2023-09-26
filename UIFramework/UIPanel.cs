using UnityEngine;

namespace UnityToolkit
{
    public interface IUIPanel
    {
        internal GameObject GetGameObject();
        internal RectTransform GetRectTransform();
        internal void SetState(UIPanelState state);
        internal int GetSortingOrder();
        public void OnLoaded(); //加载面板时调用
        public void OnOpened(); //打开面板时调用
        public void OnClosed(); //关闭面板时调用
        public void OnDispose(); //销毁面板时调用
    }

    /// <summary>
    /// UI面板状态
    /// </summary>
    public enum UIPanelState
    {
        None,
        Loaded, // 已加载
        Opening, // 打开中
        Opened, // 已打开
        Closing, // 关闭中
        Closed, //  已关闭
        Disposing // 销毁中
    }


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
        public UIPanelState state = UIPanelState.None;
        public int sortingOrder;

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
#endif
    }
}