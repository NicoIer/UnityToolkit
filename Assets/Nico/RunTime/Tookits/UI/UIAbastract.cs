using System;
using UnityEngine;

namespace Nico.UI
{
    public abstract class UIComponent : IUIComponent
    {
        public Transform transform => gameObject.transform;
        public GameObject gameObject { get; set; }
        public abstract void OnInit();

        /// <summary>
        /// 初始化对应path下的UI组件
        /// </summary>
        /// <param name="path"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public T InitComponent<T>(string path) where T : IUIComponent
        {
            Transform child = transform.Find(path); //找到对应的子物体
            T t = UIFactory<T>.createCs(); //创建对应的控制组件
            t.Bind(child.gameObject); //绑定
            t.OnInit(); //初始化
            return t;
        }
    }

    public abstract class UIWindow : UIComponent, IUIWindow
    {
        public event Action onClosed;
        public event Action onOpened;

        public virtual UGUILayer Layer() => UGUILayer.Middle;


        public virtual int Priority()
        {
            // if (Modal())//模态窗口优先级必须是最高的
            // {
            //     return Int32.MaxValue;
            // }

            return 0;
        }

        // public virtual bool Modal() => false;

        public virtual void OnOpen()
        {
            onClosed?.Invoke();
        }

        public virtual void OnClose()
        {
            onOpened?.Invoke();
        }
    }
}