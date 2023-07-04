using NUnit.Framework;
using UnityEngine;

namespace Nico.Tests
{
    [TestFixture]
    public class EventTest
    {
        public struct XEvent : IEvent
        {
        }

        public struct YEvent : IEvent
        {
        }

        public class XYClass : IEventListener<XEvent>, IEventListener<YEvent>
        {
            public void OnReceiveEvent(XEvent e)
            {
                Debug.Log("x");
            }

            public void OnReceiveEvent(YEvent e)
            {
                Debug.Log("y");
            }
        }

        [Test]
        public void EventExist()
        {
            var listener = new XYClass();
            var listener2 = new XYClass();
            EventManager.Listen<XEvent>(listener);
            EventManager.Listen<XEvent>(listener2);
            EventManager.Trigger<XEvent>();
        }
    }
}