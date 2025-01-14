#if UNITY_5_4_OR_NEWER
using Vector3 = UnityEngine.Vector3;
#else
using Vector3 = System.Numerics.Vector3;
#endif

namespace Network
{
    public struct ExponentialMovingAverageVector3
    {
        readonly float alpha;
        bool initialized;

        public Vector3 Value { get; private set; }

        public ExponentialMovingAverageVector3(int n)
        {
            // standard N-day EMA alpha calculation
            alpha = 2.0f / (n + 1);
            initialized = false;
            Value = new Vector3(0, 0, 0);
        }

        public void Add(Vector3 newValue)
        {
            // simple algorithm for EMA described here:
            // https://en.wikipedia.org/wiki/Moving_average#Exponentially_weighted_moving_variance_and_standard_deviation
            if (initialized)
            {
                Vector3 delta = newValue - Value;
                Value += (Vector3)(alpha * delta);
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
            Value = new Vector3(0, 0, 0);
        }
    }
}