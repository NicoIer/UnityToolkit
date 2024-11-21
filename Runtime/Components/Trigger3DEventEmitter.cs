#if UNITY_2021_3_OR_NEWER

using System;
using UnityEngine;
using UnityEngine.Assertions;

namespace UnityToolkit
{
    [RequireComponent(typeof(Collider))]
    public class Trigger3DEventEmitter : MonoBehaviour
    {
        public event Action<Collider> TriggerEnter = delegate { };
        public event Action<Collider> TriggerExit = delegate { };
        public event Action<Collider> TriggerStay = delegate { };
        public Collider collider3D { get; private set; }
        public LayerMask layerMask;

        private void Awake()
        {
            collider3D = GetComponent<Collider>();
            Assert.IsNotNull(collider3D);
        }

        private void OnTriggerEnter(Collider other)
        {
            if (layerMask == (layerMask | (1 << other.gameObject.layer)))
            {
                TriggerEnter(other);
            }
        }
        
        private void OnTriggerExit(Collider other)
        {
            if (layerMask == (layerMask | (1 << other.gameObject.layer)))
            {
                TriggerExit(other);
            }
        }

        private void OnTriggerStay(Collider other)
        {
            if (layerMask == (layerMask | (1 << other.gameObject.layer)))
            {
                TriggerStay(other);
            }
        }
    }
}
#endif