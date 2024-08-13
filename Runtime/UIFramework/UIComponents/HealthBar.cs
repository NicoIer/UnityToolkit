#if UNITY_5_6_OR_NEWER
using System;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace UnityToolkit
{
    public class HealthBar : MonoBehaviour, IUIComponent
    {
        public float dropSpeed = 0.5f;

        [SerializeField] private float maxHealth;
        [SerializeField] private float currentHealth;

        private float _dropEffectPercentage = 1;

        private Image _healthBar;
        private Image _dropEffect;

        public event Action<float> OnDropEffectPercentageChanged;
        public event Action OnDropEffectEnd;

        public float GetMaxHealth()
        {
            return maxHealth;
        }

        public float GetCurrentHealth()
        {
            return currentHealth;
        }

        public void SetMaxHealth(float maxHealth)
        {
            this.maxHealth = maxHealth;
        }

        public void SetCurrentHealth(float currentHealth)
        {
            this.currentHealth = currentHealth;
        }

        private void Start()
        {
            _healthBar = transform.Find("Health").GetComponent<Image>();
            _dropEffect = transform.Find("DropEffect").GetComponent<Image>();
        }

        private void Update()
        {
            float healthPercentage = Mathf.Min(Mathf.Max(0, currentHealth / maxHealth), 1);

            /* Update health bar */
            _healthBar.fillAmount = healthPercentage;

            /* Handle drop effect */
            if (_dropEffectPercentage > healthPercentage)
            {
                _dropEffectPercentage -= Time.deltaTime * dropSpeed;
                _dropEffect.fillAmount = _dropEffectPercentage;
                OnDropEffectPercentageChanged?.Invoke(_dropEffectPercentage);
            }
            else
            {
                _dropEffectPercentage = healthPercentage;
                OnDropEffectEnd?.Invoke();
            }
        }


        public void ClearEvents()
        {
            OnDropEffectPercentageChanged = null;
            OnDropEffectEnd = null;
        }
    }
}
#endif