#if UNITY_5_6_OR_NEWER

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace UnityToolkit
{
    [Serializable]
    public class SerializableDictionary<TKey, TValue> : IDictionary<TKey, TValue>,
        ISerializationCallbackReceiver
    {
        [SerializeField] private List<SerializableKeyValuePair> list = new List<SerializableKeyValuePair>();

        [Serializable]
        public struct SerializableKeyValuePair
        {
            public TKey key;
            public TValue value;

            public SerializableKeyValuePair(TKey key, TValue value)
            {
                this.key = key;
                this.value = value;
            }

            public void SetValue(TValue value)
            {
                this.value = value;
            }
        }

        private Dictionary<TKey, uint> KeyPositions => _keyPositions.Value;
        private Lazy<Dictionary<TKey, uint>> _keyPositions;

        public SerializableDictionary()
        {
            _keyPositions = new Lazy<Dictionary<TKey, uint>>(MakeKeyPositions);
        }

        public SerializableDictionary(IDictionary<TKey, TValue> dictionary)
        {
            _keyPositions = new Lazy<Dictionary<TKey, uint>>(MakeKeyPositions);

            if (dictionary == null)
            {
                throw new ArgumentException("The passed dictionary is null.");
            }

            foreach (KeyValuePair<TKey, TValue> pair in dictionary)
            {
                Add(pair.Key, pair.Value);
            }
        }


        private Dictionary<TKey, uint> MakeKeyPositions()
        {
            int numEntries = list.Count;

            Dictionary<TKey, uint> result = new Dictionary<TKey, uint>(numEntries);

            for (int i = 0; i < numEntries; ++i)
            {
                result[list[i].key] = (uint)i;
            }

            return result;
        }

        public void OnBeforeSerialize()
        {
        }

        public void OnAfterDeserialize()
        {
            // After deserialization, the key positions might be changed
            _keyPositions = new Lazy<Dictionary<TKey, uint>>(MakeKeyPositions);
        }

        #region IDictionary

        public TValue this[TKey key]
        {
            get => list[(int)KeyPositions[key]].value;
            set
            {
                if (KeyPositions.TryGetValue(key, out _))
                {
                    Remove(key);
                    Add(key,value);
                }
                else
                {
                    KeyPositions[key] = (uint)list.Count;
                    list.Add(new SerializableKeyValuePair(key, value));
                }
            }
        }

        public ICollection<TKey> Keys => list.Select(tuple => tuple.key).ToArray();
        public ICollection<TValue> Values => list.Select(tuple => tuple.value).ToArray();

        public void Add(TKey key, TValue value)
        {
            if (KeyPositions.ContainsKey(key))
            {
                throw new ArgumentException("An element with the same key already exists in the dictionary.");
            }
            else
            {
                KeyPositions[key] = (uint)list.Count;

                list.Add(new SerializableKeyValuePair(key, value));
            }
        }

        public bool ContainsKey(TKey key) => KeyPositions.ContainsKey(key);

        public bool Remove(TKey key)
        {
            if (KeyPositions.TryGetValue(key, out uint index))
            {
                Dictionary<TKey, uint> kp = KeyPositions;

                kp.Remove(key);

                list.RemoveAt((int)index);

                int numEntries = list.Count;

                for (uint i = index; i < numEntries; i++)
                {
                    kp[list[(int)i].key] = i;
                }

                return true;
            }

            return false;
        }

        public bool TryGetValue(TKey key, out TValue value)
        {
            if (KeyPositions.TryGetValue(key, out uint index))
            {
                value = list[(int)index].value;

                return true;
            }

            value = default;

            return false;
        }

        #endregion

        #region ICollection

        public int Count => list.Count;
        public bool IsReadOnly => false;

        public void Add(KeyValuePair<TKey, TValue> kvp) => Add(kvp.Key, kvp.Value);

        public void Clear()
        {
            list.Clear();
            KeyPositions.Clear();
        }

        public bool Contains(KeyValuePair<TKey, TValue> kvp) => KeyPositions.ContainsKey(kvp.Key);

        public void CopyTo(KeyValuePair<TKey, TValue>[] array, int arrayIndex)
        {
            int numKeys = list.Count;

            if (array.Length - arrayIndex < numKeys)
            {
                throw new ArgumentException("arrayIndex");
            }

            for (int i = 0; i < numKeys; ++i, ++arrayIndex)
            {
                SerializableKeyValuePair entry = list[i];

                array[arrayIndex] = new KeyValuePair<TKey, TValue>(entry.key, entry.value);
            }
        }

        public bool Remove(KeyValuePair<TKey, TValue> kvp) => Remove(kvp.Key);

        #endregion

        #region IEnumerable

        public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator()
        {
            return list.Select(ToKeyValuePair).GetEnumerator();

            KeyValuePair<TKey, TValue> ToKeyValuePair(SerializableKeyValuePair skvp)
            {
                return new KeyValuePair<TKey, TValue>(skvp.key, skvp.value);
            }
        }

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        #endregion
    }
}

#endif