// Copyright (c) 2023 NicoIer and Contributors.
// Licensed under the MIT License (MIT). See LICENSE in the repository root for more information.
#if UNITY_2021_3_OR_NEWER

using UnityEngine;

namespace UnityToolkit
{
    [CreateAssetMenu(fileName = "AnimationCurve", menuName = "Toolkit/AnimationCurve")]
    public class AnimationCurveScriptableObject : ScriptableObject
    {
        public AnimationCurve curve;
    }
}
#endif
