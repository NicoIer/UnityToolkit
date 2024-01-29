using System;
using UnityEngine;
using UnityEngine.UI;

namespace UnityToolkit
{
    public class HealthBar : MonoBehaviour , IUIComponent
    {
        public float dropSpeed = 0.5f;

        private float maxHealth;
        private float currentHealth;

        private float dropEffectPercentage = 1;

        private Image healthBar;
        private Image dropEffect;
        
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
            healthBar = transform.Find("Health").GetComponent<Image>();
            dropEffect = transform.Find("DropEffect").GetComponent<Image>();
        }

        private void Update()
        {
            float healthPercentage = Mathf.Min(Mathf.Max(0, currentHealth / maxHealth), 1);

            /* Update health bar */
            healthBar.fillAmount = healthPercentage;

            /* Handle drop effect */
            if (dropEffectPercentage > healthPercentage)
            {
                dropEffectPercentage -= Time.deltaTime * dropSpeed;
                dropEffect.fillAmount = dropEffectPercentage;
                OnDropEffectPercentageChanged?.Invoke(dropEffectPercentage);
            }
            else
            {
                dropEffectPercentage = healthPercentage;
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