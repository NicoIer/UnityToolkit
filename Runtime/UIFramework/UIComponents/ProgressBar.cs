#if UNITY_5_6_OR_NEWER
using System;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace UnityToolkit
{
    public enum ProgressTitleType
    {
        Value,
        Percent,
    }

    public class ProgressBar : MonoBehaviour, IUIComponent
    {
        [field: SerializeField] public TextMeshProUGUI title { get; private set; }
        [field: SerializeField] public Image bar { get; private set; }
        [SerializeField] private bool usingTitle;
        [SerializeField] private float max = 100;
        [SerializeField] private float value = 50;
        [SerializeField] private float min = 0;
        [SerializeField] private ProgressTitleType titleType = ProgressTitleType.Value;

        [SerializeField] private Image.FillMethod fillMethod = Image.FillMethod.Horizontal; //默认水平填充

        // public bool tween = true;
        private void Awake()
        {
            if (title == null)
            {
                title = transform.Find("Title").GetComponent<TextMeshProUGUI>();
            }

            if (bar == null)
            {
                bar = transform.Find("Mask/Bar").GetComponent<Image>();
            }
        }

        public bool UsingTitle
        {
            get => usingTitle;
            set
            {
                usingTitle = value;
                UpdateVisualDirect();
            }
        }

        public float Value
        {
            get => value;
            set => SetValue(value);
        }

        public float Max
        {
            get => max;
            set
            {
                max = value;
                UpdateVisualDirect();
            }
        }

        public float Min
        {
            get => min;
            set
            {
                min = value;
                UpdateVisualDirect();
            }
        }

        public ProgressTitleType TitleType
        {
            get => titleType;
            set
            {
                titleType = value;
                UpdateVisualDirect();
            }
        }

        public Image.FillMethod FillMethod
        {
            get => fillMethod;
            set
            {
                fillMethod = value;
                bar.fillMethod = value;
            }
        }


        public event Action<float> OnValueChanged;

        private void UpdateVisualDirect()
        {
            bar.fillMethod = fillMethod;
            UpdateTitle();
            UpdateProgress();
        }

        private void UpdateProgress()
        {
            float percent = (value - min) / (max - min);
            bar.fillAmount = percent;
        }

        private void UpdateTitle()
        {
            title.enabled = usingTitle;
            if (!usingTitle) return;
            switch (titleType)
            {
                case ProgressTitleType.Value:
                    title.text = $"{value}/{max}";
                    break;
                case ProgressTitleType.Percent:
                    float percent = (value - min) / (max - min);
                    title.text = $"{percent:P}";
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private const float Tolerance = 0.0001f;

        public void SetPercent(float percent)
        {
            percent = Mathf.Clamp01(percent);
            SetValue(min + (max - min) * percent);
        }

        public void SetPercentWithoutNotify(float percent)
        {
            percent = Mathf.Clamp01(percent);
            value = min + (max - min) * percent;
            UpdateVisualDirect();
        }

        public void SetValue(float value)
        {
            if (value > max) value = max;
            if (value < min) value = min;

            if (Math.Abs(this.value - value) < Tolerance) return;
            this.value = value;
            UpdateVisualDirect();
            OnValueChanged?.Invoke(value);
        }

        public void SetWithoutNotify(float value, float minValue, float maxValue)
        {
            this.value = value;
            max = maxValue;
            min = minValue;
            UpdateVisualDirect();
        }


#if UNITY_EDITOR
        private void OnValidate()
        {
            if (title == null) Awake();
            bar.type = Image.Type.Filled;
            if (min > max) min = max;
            if (value > max) value = max;
            if (value < min) value = min;

            UpdateVisualDirect();
        }

        [UnityEditor.MenuItem("GameObject/UI/UnityToolkit/ProgressBar")]
        private static void CreateProgressBar()
        {
            GameObject progressBarPrefab = Resources.Load<GameObject>("UnityToolkit/ProgressBar");
            // UnityEditor.PrefabUtility.InstantiatePrefab(progressBarPrefab) as GameObject;
            GameObject progressBar = Instantiate(progressBarPrefab, UnityEditor.Selection.activeTransform, false);
            UnityEditor.Selection.activeGameObject = progressBar;
        }
#endif
    }
}
#endif