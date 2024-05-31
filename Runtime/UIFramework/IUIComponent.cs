namespace UnityToolkit
{
    //用于标记的接口 表示UI组件
    public interface IUIComponent
    {
        
    }
    
    public interface IUISubPanel : IUIComponent
    {
        bool IsOpen();
        void Open();
        void Close();
    }
}