using System;
using UnityEngine;

namespace Nico.VirtualControl
{
    public class VirtualStick : MonoBehaviour
    {
        private RectTransform _rectTransform;

        [SerializeField] private Knob knob;
        [SerializeField] [Range(0.1f, 0.9f)] private float knobSizePercent = 0.48f;//Knob size is a percentage of the stick size
        [SerializeField] [Range(0.0f, 1.0f)] private float knobDistance = 1;//Knob distance is a percentage of the stick size
        private float _radius;
        private float _requireDistance;
        public bool isDown => knob.isDown;
        public event Action<bool> ActiveEvent;
        public event Action<Vector2> OnAxisEvent;
        public event Action<float> OnAngelEvent;
        [SerializeField] [Range(0.1f, 1.0f)] private float deltaThreshold = 0.2f;

        private void Awake()
        {
            knob = GetComponentInChildren<Knob>();

            _rectTransform = GetComponent<RectTransform>();
            _radius = _rectTransform.sizeDelta.x;
            _requireDistance = _radius * knobDistance * deltaThreshold * knobSizePercent;

            knob.SetSize(knobSizePercent, _radius);
            knob.ActiveEvent += value => ActiveEvent?.Invoke(value);
            knob.onDragEvent += OnDragEvent;
        }
#if UNITY_EDITOR
        private void OnValidate()
        {
            knob = GetComponentInChildren<Knob>();

            _rectTransform = GetComponent<RectTransform>();
            _radius = _rectTransform.sizeDelta.x;
            _requireDistance = _radius * knobDistance * deltaThreshold * knobSizePercent;

            knob.SetSize(knobSizePercent, _radius);
        }
#endif
        private void OnEnable()
        {
            knob.gameObject.SetActive(true);
        }

        private void OnDisable()
        {
            knob.gameObject.SetActive(false);
        }

        public Vector2 ReadAxis() => (knob.transform.position - transform.position).normalized;
        public float ReadAngel() => Vector2.Angle(Vector2.up, ReadAxis());

        private void OnDragEvent(Vector2 obj)
        {
            Vector3 direction = ReadAxis();
        
            float distance = Vector3.Distance(knob.transform.position, transform.position);
        
            //Only allow the knob to go so far
            if (distance > _requireDistance)
            {
                knob.transform.position = transform.position + direction * _requireDistance;
            }
        
            // axis update
            OnAxisEvent?.Invoke(direction);
            // angel update
            float angel = Vector2.Angle(Vector2.up, direction);
            angel = direction.x < 0 ? 360 - angel : angel;
            OnAngelEvent?.Invoke(angel);
        }
    }
}