#if UNITY_2021_3_OR_NEWER

using System;
using UnityEngine;
using UnityEngine.Assertions;

namespace UnityToolkit
{
    [RequireComponent(typeof(Collider2D))]
    public class Trigger2DEventEmitter : MonoBehaviour
    {
        public event Action<Collider2D> TriggerEnter = delegate { };
        public event Action<Collider2D> TriggerExit = delegate { };
        public event Action<Collider2D> TriggerStay = delegate { };
        public new Collider2D collider2D { get; private set; }
        public LayerMask layerMask;

        private void Awake()
        {
            collider2D = GetComponent<Collider2D>();
            Assert.IsNotNull(collider2D);
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (layerMask == (layerMask | (1 << other.gameObject.layer)))
            {
                TriggerEnter(other);
            }
        }

        private void OnTriggerExit2D(Collider2D other)
        {
            if (layerMask == (layerMask | (1 << other.gameObject.layer)))
            {
                TriggerExit(other);
            }
        }

        private void OnTriggerStay2D(Collider2D other)
        {
            if (layerMask == (layerMask | (1 << other.gameObject.layer)))
            {
                TriggerStay(other);
            }
        }
    }
}
#endif
