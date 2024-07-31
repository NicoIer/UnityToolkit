using UnityEngine;

namespace UnityToolkit
{
    //用于标记的接口 表示UI组件
    public interface IUIComponent
    {
    }
    
    public interface IUIView : IUIComponent
    {
        bool IsOpen();
        void Open();
        void Close();
    }

    public interface IUIPanel : IUIComponent
    {
        internal GameObject GetGameObject();
        internal RectTransform GetRectTransform();
        internal void SetState(UIPanelState state);
        public int GetSortingOrder();
        public void OnLoaded(); //加载面板时调用
        public void OnOpened(); //打开面板时调用
        public void OnClosed(); //关闭面板时调用
        public void OnDispose(); //销毁面板时调用
    }
}