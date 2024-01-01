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
        [SerializeField] private TextMeshProUGUI title;
        [SerializeField] private Image bar;
        [SerializeField] private bool usingTitle;
        [SerializeField] private float _max = 100;
        [SerializeField] private float _value = 50;
        [SerializeField] private float _min = 0;
        [SerializeField] private ProgressTitleType _titleType = ProgressTitleType.Value;
        [SerializeField] private Image.FillMethod _fillMethod = Image.FillMethod.Horizontal; //默认水平填充
        // public bool tween = true;

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
            get => _value;
            set => SetValue(value);
        }

        public float Max
        {
            get => _max;
            set
            {
                _max = value;
                UpdateVisualDirect();
            }
        }

        public float Min
        {
            get => _min;
            set
            {
                _min = value;
                UpdateVisualDirect();
            }
        }

        public ProgressTitleType TitleType
        {
            get => _titleType;
            set
            {
                _titleType = value;
                UpdateVisualDirect();
            }
        }

        public Image.FillMethod FillMethod
        {
            get => _fillMethod;
            set
            {
                _fillMethod = value;
                bar.fillMethod = value;
            }
        }


        public event Action<float> OnValueChanged;

        private void UpdateVisualDirect()
        {
            bar.fillMethod = _fillMethod;
            UpdateTitle();
            UpdateProgress();
        }

        private void UpdateProgress()
        {
            float percent = (_value - _min) / (_max - _min);
            bar.fillAmount = percent;
        }

        private void UpdateTitle()
        {
            title.enabled = usingTitle;
            if (!usingTitle) return;
            switch (_titleType)
            {
                case ProgressTitleType.Value:
                    title.text = $"{_value}/{_max}";
                    break;
                case ProgressTitleType.Percent:
                    float percent = (_value - _min) / (_max - _min);
                    title.text = $"{percent:P}";
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private const float Tolerance = 0.0001f;

        public void SetValue(float value)
        {
            if (value > _max) value = _max;
            if (value < _min) value = _min;

            if (Math.Abs(this._value - value) < Tolerance) return;
            this._value = value;
            UpdateVisualDirect();
            OnValueChanged?.Invoke(value);
        }

        public void Init(float value,float minValue, float maxValue)
        {
            this._value = value;
            _max = maxValue;
            _min = minValue;
            SetValue(value);
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            bar.type = Image.Type.Filled;
            if (_min > _max) _min = _max;
            if (_value > _max) _value = _max;
            if (_value < _min) _value = _min;

            UpdateVisualDirect();
        }

        [UnityEditor.MenuItem("GameObject/UI/ProgressBar")]
        private static void CreateProgressBar()
        {
            GameObject progressBarPrefab = Resources.Load<GameObject>("ProgressBar");
            GameObject progressBar = UnityEditor.PrefabUtility.InstantiatePrefab(progressBarPrefab) as GameObject;
            progressBar.transform.SetParent(UnityEditor.Selection.activeTransform, false);
        }
#endif
    }
}