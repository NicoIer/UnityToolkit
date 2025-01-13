#if UNITY_5_6_OR_NEWER
using System;
using UnityEngine;
using UnityEngine.Serialization;

namespace UnityToolkit
{
    [RequireComponent(typeof(CanvasGroup))]
    public partial class UILayer : MonoBehaviour
    {
        public RectTransform rectTransform { get; private set; }
        public CanvasGroup canvasGroup { get; private set; }

        private void Awake()
        {
            canvasGroup = GetComponent<CanvasGroup>();
            rectTransform = GetComponent<RectTransform>();
        }
    }
}
#endif
