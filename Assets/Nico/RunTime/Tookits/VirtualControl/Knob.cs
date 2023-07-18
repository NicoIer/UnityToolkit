using System;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Nico.VirtualControl
{
    public class Knob : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IDragHandler
    {
        internal Vector3 startPos;
        private RectTransform _rectTransform;
        internal event Action<bool> ActiveEvent;
        internal event Action<Vector2> onDragEvent; 
        public bool isDown { get; private set; }

        private void Awake()
        {
            _rectTransform = GetComponent<RectTransform>();
            startPos = _rectTransform.anchoredPosition;
        }

        public void SetSize(float percent, float parentRadius)
        {
            if (_rectTransform == null)
            {
                Awake();
            }
            Vector2 size = _rectTransform.sizeDelta;
            size.x = size.y = percent * parentRadius;
            _rectTransform.sizeDelta = size;
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            isDown = true;
            ActiveEvent?.Invoke(isDown);
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            isDown = false;
            ActiveEvent?.Invoke(isDown);
            _rectTransform.anchoredPosition = startPos;
        }

        public void OnDrag(PointerEventData eventData)
        {
            transform.position = eventData.position;
            onDragEvent?.Invoke(eventData.position);
        }
    }
}