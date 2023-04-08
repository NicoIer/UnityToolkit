using Nico.Design;
using UnityEngine;

namespace Nico.Components
{
    public class LookAtCameraComponent : MonoBehaviour
    {
        private void OnEnable()
        {
            LookAtCameraSystem.Instance.Register(transform);
        }

        private void OnDisable()
        {
            try
            {
                LookAtCameraSystem.Instance.UnRegister(transform);
            }
            catch (DesignException)
            {
            }
        }
    }
}