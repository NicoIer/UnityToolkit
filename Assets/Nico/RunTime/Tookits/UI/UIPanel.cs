using UnityEngine;

namespace Nico
{
    public abstract class UIPanel: MonoBehaviour
    {
        public virtual void OnCreate()
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
    }
}