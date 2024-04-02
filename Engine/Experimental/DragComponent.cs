using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace UnityToolkit
{
    public class DragComponent : UIBehaviour, IDragHandler, IBeginDragHandler, IEndDragHandler
    {
        private static readonly Vector2 HalfOne = Vector2.one / 2;

        private RectTransform _rectTransform;

        // private Vector2 _prePos;
        private SlotComponent _curSlot;
        private SlotComponent _preSlot;

        protected override void Awake()
        {
            _rectTransform = transform as RectTransform;
            SlotComponent slot = GetComponentInParent<SlotComponent>();
            if (slot != null)
            {
                OnPlace(slot);
            }
        }

        public void OnBeginDrag(PointerEventData eventData)
        {
            GetComponent<Graphic>().raycastTarget = false;
            // 把自己放到parent一个层级最后一个 确保优先渲染
            if (_curSlot != null)
            {
                transform.SetParent(_curSlot.transform.parent);
                transform.SetAsLastSibling();
            }

            // _prePos = _rectTransform.anchoredPosition;
            _preSlot = _curSlot;
        }

        public void OnDrag(PointerEventData eventData)
        {
            _rectTransform.anchoredPosition += eventData.delta;
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            GetComponent<Graphic>().raycastTarget = true;
            // 如果Slot没有被修改 则回到原来的位置
            if (_curSlot == _preSlot)
            {
                _ReSetPos(_preSlot.transform);
            }
        }

        /// <summary>
        /// 在OnEndDrag前触发
        /// </summary>
        /// <param name="slot"></param>
        public void OnPlace(SlotComponent slot)
        {
            if (slot == _curSlot) return;
            if (_curSlot != null)
            {
                _curSlot.CancelPlace();
            }

            _curSlot = slot;

            _ReSetPos(slot.transform);
            // _rectTransform.anchorMax = slot.rectTransform.anchorMax;
            // _rectTransform.anchorMin = slot.rectTransform.anchorMin;
            // _rectTransform.pivot = slot.rectTransform.pivot;
            // _rectTransform.anchoredPosition = slot.rectTransform.anchoredPosition;
        }

        private void _ReSetPos(Transform parent)
        {
            _rectTransform.SetParent(parent);
            _rectTransform.anchorMax = HalfOne;
            _rectTransform.anchorMin = HalfOne;
            _rectTransform.pivot = HalfOne;
            _rectTransform.anchoredPosition = Vector2.zero;
        }
    }
}