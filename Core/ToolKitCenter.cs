namespace UnityToolkit
{
    /// <summary>
    ///  工具包中心
    /// </summary>
    public sealed class ToolKitCenter
    {
        public ModelCenter model { get; private set; } = new ModelCenter();//数据中心
        public TypeEventSystem typeEvent { get; private set; } = new TypeEventSystem(); //事件中心
        public EventRepository eventRepository { get; private set; } = new EventRepository(); //事件仓库
    }
}