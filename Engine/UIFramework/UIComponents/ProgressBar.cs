using System;
using TMPro;
using UnityEngine;
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
        public TextMeshProUGUI title;
        public Image bar;
        [SerializeField] private bool usingTitle = false;
        [SerializeField] private int max = 10;
        [SerializeField] private int value = 5;
        [SerializeField] private int min = 0;
        public ProgressTitleType titleType = ProgressTitleType.Value;
        public bool tween = true;

        public bool UsingTitle
        {
            get => usingTitle;
            set
            {
                usingTitle = value;
                UpdateVisualDirect();
            }
        }

        public int Value
        {
            get => value;
            set => SetValue(value);
        }

        public int Max
        {
            get => max;
            set
            {
                max = value;
                UpdateVisualDirect();
            }
        }

        public int Min
        {
            get => min;
            set
            {
                min = value;
                UpdateVisualDirect();
            }
        }


        public event Action<int> OnValueChanged;

        private void UpdateVisualDirect()
        {
            UpdateTitle();
            UpdateProgress();
        }

        private void UpdateProgress()
        {
            float percent = ((float)value - min) / (max - min);
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
                    float percent = ((float)value - min) / (max - min);
                    title.text = $"{percent:P}";
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }


        public void SetValue(int value)
        {
            if (value > max) value = max;
            if (value < min) value = min;

            if (this.value != value)
            {
                this.value = value;
                UpdateVisualDirect();
                OnValueChanged?.Invoke(value);
            }
        }
        public void SetProgress(int value, int maxValue)
        {
            this.value = value;
            max = maxValue;
            UpdateVisualDirect();
        }
        
#if UNITY_EDITOR
        private void OnValidate()
        {
            bar.type = Image.Type.Filled;
            if (min > max) min = max;
            if (value > max) value = max;
            if (value < min) value = min;

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