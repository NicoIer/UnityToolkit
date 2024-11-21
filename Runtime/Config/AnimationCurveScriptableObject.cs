#if UNITY_2021_3_OR_NEWER

using UnityEngine;

namespace UnityToolkit
{
    [CreateAssetMenu(fileName = "AnimationCurve", menuName = "UnityToolkit/AnimationCurve")]
    public class AnimationCurveScriptableObject : ScriptableObject
    {
        public AnimationCurve curve;
    }
}
#endif
