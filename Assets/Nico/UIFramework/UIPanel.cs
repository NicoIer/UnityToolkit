using UnityEngine;

namespace Nico
{
    public interface IUIPanel
    {
        public void OnCreate();
        public bool DestroyOnHide();
        public void OnShow();
        public void OnHide();
        public int Priority();
        public UILayer Layer();
        public GameObject GetGameObject();
        
        public Transform GetTransform()
        {
            return GetGameObject().transform;
        }
    }

    // 为什么用MonoBehavior？ 因为这样直观，通常UI代码不会对游戏性能造成影响，UI上挂MonoBehavior没有啥问题
    public abstract partial class UIPanel : MonoBehaviour, IUIPanel
    {
        public virtual void OnCreate()
        {
            OnPartialBind();
        }

        public virtual bool DestroyOnHide() => false;

        protected virtual void OnPartialBind()
        {
        }


        public virtual void OnShow()
        {
        }

        public virtual void OnHide()
        {
        }

        public virtual int Priority() => 0;

        public virtual UILayer Layer() => UILayer.Middle;

        public GameObject GetGameObject()
        {
            return gameObject;
        }
    }
}