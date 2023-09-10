using System;

namespace Nico
{
    public struct ExponentialMovingAverage
    {
        private readonly double _alpha;
        private bool _initialized;

        public double value;
        public double variance;
        public double standardDeviation;


        public ExponentialMovingAverage(int n)
        {
            // standard N-day EMA alpha calculation
            _alpha = 2.0 / (n + 1);
            _initialized = false;
            value = 0;
            variance = 0;
            standardDeviation = 0;
        }

        public void Add(double newValue)
        {
            if (_initialized)
            {
                double delta = newValue - value;
                value += _alpha * delta;
                variance = (1 - _alpha) * (variance + _alpha * delta * delta);
                standardDeviation = Math.Sqrt(variance);
            }
            else
            {
                value = newValue;
                _initialized = true;
            }
        }

        public void Reset()
        {
            _initialized = false;
            value = 0;
            variance = 0;
        }
    }
}