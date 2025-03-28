#if UNITY_5_6_OR_NEWER
using System;
using TMPro;
using UnityEngine;

namespace UnityToolkit
{
    [ExecuteAlways]
    public class ProgressText : MonoBehaviour, IUIComponent
    {
        [SerializeField] private TextMeshProUGUI value;
        [SerializeField] private TextMeshProUGUI maxValue;

        [SerializeField] private ProgressBar _progressBar;

        private void Update()
        {
            value.text = _progressBar.Value.ToString("F0");
            maxValue.text = _progressBar.Max.ToString("F0");
        }
    }
}
#endif