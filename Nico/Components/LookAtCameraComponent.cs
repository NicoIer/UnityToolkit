using System;
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
            if (LookAtCameraSystem.Instance == null) return;
            LookAtCameraSystem.Instance.UnRegister(transform);
        }
    }
}