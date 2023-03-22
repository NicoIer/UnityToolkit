using System;
using UnityEngine;
using UnityEngine.UI;

namespace Nico
{
    public class ProgressBarUI : MonoBehaviour
    {
        private Image _image;
        public event Action onSetProgress;

        private void Awake()
        {
            _image = transform.Find("Bar").GetComponent<Image>();
            Hide();
        }

        public void SetProgress(float progress)
        {
            var target = Mathf.Clamp01(progress);
            if (target == 0 || Math.Abs(target - 1) < Mathf.Epsilon)
            {
                Hide();
                return;
            }

            Show();
            _image.fillAmount = target;
            onSetProgress?.Invoke();
        }

        public void Hide()
        {
            //如果此时游戏直接退出了 会出现一点BUG
            //但是这个BUG不影响游戏 还是处理一下
            try
            {
                gameObject.SetActive(false);
            }
            catch (Exception)
            {
                //ignore
            }
        }

        public void Show()
        {
            gameObject.SetActive(true);
        }
    }
}