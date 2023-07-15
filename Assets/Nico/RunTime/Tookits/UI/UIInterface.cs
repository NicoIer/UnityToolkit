using System;
using UnityEngine;

namespace Nico.UI
{
    public interface UIInterface
    {
        public GameObject gameObject { get; set; }
        public Transform transform => gameObject.transform;

        public void OnInit();

        internal void Bind(GameObject gameObject)
        {
            this.gameObject = gameObject;
        }
    }

    public interface IUIComponent : UIInterface
    {
        public T InitComponent<T>(string path) where T : IUIComponent;
    }

    public interface IUIPanel : IUIComponent
    {
        
    }

    public interface IUIWindow : IUIComponent
    {
        UGUILayer Layer(); //层级
        int Priority(); //优先级
        // bool Modal(); //是否模态
        void OnOpen();
        void OnClose();
    }
}