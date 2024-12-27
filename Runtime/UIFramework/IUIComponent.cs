#if UNITY_5_6_OR_NEWER
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
        
        /// <summary>
        /// 获得面板的排序顺序 越大越优先显示
        /// 在Hierarchy中的顺序越末尾(因为末尾后画 所以越优先显示)
        /// </summary>
        /// <returns></returns>
        public int GetSortingOrder();
        public void OnLoaded(); //加载面板时调用
        public void OnOpened(); //打开面板时调用
        public void OnClosed(); //关闭面板时调用
        public void OnDispose(); //销毁面板时调用
        
        /// <summary>
        /// 获得面板所在的层级
        /// </summary>
        /// <returns></returns>
        public sbyte GetLayer();
    }
}
#endif