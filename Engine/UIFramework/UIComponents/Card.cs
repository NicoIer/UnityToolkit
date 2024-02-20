using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace UnityToolkit
{
    [DisallowMultipleComponent]
    public class Card : MonoBehaviour, IDragHandler, IBeginDragHandler, IEndDragHandler
    {
        public static readonly Vector2 HalfOne = Vector2.one / 2;
        public CardSlot slot => _curSlot;
        private CardSlot _curSlot;
        private CardSlot _prevSlot;
        private RectTransform _rectTransform;
        protected bool canDrag = true;
        public bool canSetToOtherSlot = true;

        public RectTransform rectTransform
        {
            get
            {
                if (_rectTransform == null)
                {
                    _rectTransform = transform as RectTransform;
                }

                return _rectTransform;
            }
        }

        private Graphic _graphics;

        public Graphic graphic
        {
            get
            {
                if (_graphics == null)
                {
                    _graphics = GetComponent<Graphic>();
                }

                return _graphics;
            }
        }

        protected virtual void Awake()
        {
            _curSlot = GetComponentInParent<CardSlot>();
            _prevSlot = _curSlot;
            if (_curSlot == null)
            {
                Debug.LogError($"卡片:{this} 必须是一个 {nameof(CardSlot)} 的子物体");
            }

            _curSlot.PlaceCard(this);
        }


        public virtual void OnDrag(PointerEventData eventData)
        {
            if (!canDrag) return;
            rectTransform.anchoredPosition += eventData.delta;
        }

        public virtual void OnBeginDrag(PointerEventData eventData)
        {
            _prevSlot = _curSlot;
            graphic.raycastTarget = false;
            transform.SetParent(_curSlot.transform.parent);
            transform.SetAsLastSibling();
        }

        public virtual void OnEndDrag(PointerEventData eventData)
        {
            if (_prevSlot != _curSlot)
            {
                _prevSlot.RemoveCard();
                _prevSlot = _curSlot; // 不需要修改位置了  因为 修改卡槽时 会自动调用 SetNewSlot 从而修改位置
            }
            else
            {
                // Debug.Log("恢复位置".Green());
                _curSlot.ReutrnCard();
                // 恢复位置
                _ResetPos();
            }
        }

        private void _ResetPos()
        {
            graphic.raycastTarget = true;
            rectTransform.SetParent(_curSlot.transform);
            rectTransform.anchorMax = HalfOne;
            rectTransform.anchorMin = HalfOne;
            rectTransform.pivot = HalfOne;
            rectTransform.anchoredPosition = Vector2.zero;
        }


#if ODIN_INSPECTOR && UNITY_EDITOR
        [Sirenix.OdinInspector.Button]
        private void ResetPos()
        {
            _curSlot = GetComponentInParent<CardSlot>();
            _curSlot.RemoveCard();
            _curSlot.PlaceCard(this);
        }

#endif
        public void SetNewSlot(CardSlot cardSlot)
        {
            if (canSetToOtherSlot)
            {
                _curSlot = cardSlot;
            }

            _ResetPos();
        }
    }
}