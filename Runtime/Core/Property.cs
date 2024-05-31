using System;

namespace UnityToolkit
{
    [Serializable]
    public class Property<T> where T : IComparable
    {
#if UNITY_EDITOR
        [UnityEngine.SerializeField] private T _maxValue;
        [UnityEngine.SerializeField] private T _minValue;
        [UnityEngine.SerializeField] private T _value;
#else
        private T _maxValue;
        private T _minValue;
        private T _value;
#endif


        private Action<Property<T>> _onValueChanged = _ => { };

        public T MaxValue
        {
            get => _maxValue;
            set
            {
                if (value == null && _maxValue == null) return;
                if (value != null && value.Equals(_maxValue)) return;
                _maxValue = value;
                _onValueChanged?.Invoke(this);
            }
        }

        public T MinValue
        {
            get => _minValue;
            set
            {
                if (value == null && _minValue == null) return;
                if (value != null && value.Equals(_minValue)) return;
                _minValue = value;
                _onValueChanged?.Invoke(this);
            }
        }

        public T Value
        {
            get => _value;
            set
            {
                if (value == null && _value == null) return;
                if (value != null && value.Equals(_value)) return;

                if (value == null)
                {
                    _value = _minValue;
                    _onValueChanged?.Invoke(this);
                    return;
                }

                if (value.CompareTo(_maxValue) > 0)
                {
                    _value = _maxValue;
                    _onValueChanged?.Invoke(this);
                    return;
                }

                if (value.CompareTo(_minValue) < 0)
                {
                    _value = _minValue;
                    _onValueChanged?.Invoke(this);
                    return;
                }

                _value = value;
                _onValueChanged?.Invoke(this);
            }
        }


        public T ValueWithoutNotify => _value;

        public Property(T value = default, T minValue = default, T maxValue = default)
        {
            _value = value;
            _minValue = minValue;
            _maxValue = maxValue;
        }


        public ICommand Register(Action<Property<T>> onValueChanged)
        {
            _onValueChanged += onValueChanged;
            return new BindablePropertyUnRegister(() => _onValueChanged -= onValueChanged);
        }

        public void UnRegister(Action<Property<T>> onValueChanged)
        {
            _onValueChanged -= onValueChanged;
        }

        public override string ToString()
        {
            return Value.ToString();
        }


        public void Invoke()
        {
            _onValueChanged(this);
        }

        // 实现隐式类型转换
        public static implicit operator T(Property<T> property)
        {
            return property.Value;
        }
    }
}