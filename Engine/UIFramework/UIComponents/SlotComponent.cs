using UnityEngine;
using UnityEngine.EventSystems;

namespace UnityToolkit
{
    /// <summary>
    /// 用于放置卡牌的卡槽
    /// </summary>
    public class SlotComponent : UIBehaviour, IDropHandler
    {
        private DragComponent _dragComponent;
        public RectTransform rectTransform { get; private set; }

        protected override void Awake()
        {
            rectTransform = transform as RectTransform;
        }

        public void OnDrop(PointerEventData eventData)
        {
            if (_dragComponent!=null) return;
            _dragComponent = eventData.pointerDrag.GetComponent<DragComponent>();
            _dragComponent.OnPlace(this);
        }

        public void CancelPlace()
        {
            _dragComponent = null;
        }
    }
}