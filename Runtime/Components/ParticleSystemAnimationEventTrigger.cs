// Copyright (c) 2023 NicoIer and Contributors.
// Licensed under the MIT License (MIT). See LICENSE in the repository root for more information.
#if UNITY_2021_3_OR_NEWER
using UnityEngine;

namespace UnityToolkit
{
    internal class ParticleSystemAnimationEventTrigger : MonoBehaviour
    {
        [field: SerializeField] public ParticleSystem particleSystem { get; private set; }

        public void Play()
        {
            if (particleSystem != null)
            {
                particleSystem.Play();
            }
        }
    }
}
#endif