#if UNITY_5_4_OR_NEWER
using UnityEngine;

namespace Network
{
    public struct ExponentialMovingAverageQuaternion
    {
        readonly float alpha;
        bool initialized;
        public Quaternion Value;
        
        public ExponentialMovingAverageQuaternion(int n)
        {
            // standard N-day EMA alpha calculation
            alpha = 2.0f / (n + 1);
            initialized = false;
            Value = default;
        }
        
        public void Add(Quaternion newValue)
        {
            // simple algorithm for EMA described here:
            // https://en.wikipedia.org/wiki/Moving_average#Exponentially_weighted_moving_variance_and_standard_deviation
            if (initialized)
            {
                Quaternion delta = newValue * UnityEngine.Quaternion.Inverse(Value);
                Value = Value * UnityEngine.Quaternion.Slerp(Quaternion.identity, delta, alpha);
            }
            else
            {
                Value = newValue;
                initialized = true;
            }
        }
        
        public void Reset()
        {
            initialized = false;
            Value = Quaternion.identity;
        }
    }
}
#endif
