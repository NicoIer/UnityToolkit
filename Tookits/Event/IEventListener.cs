namespace Nico
{
    public interface IEventListener<in TEvent> where TEvent : IEvent
    {
        void OnReceiveEvent(TEvent e);
    }
}