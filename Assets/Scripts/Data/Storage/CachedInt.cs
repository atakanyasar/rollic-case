using System.Collections.Generic;
using UnityEngine;
namespace Data.Storage
{
    public class CachedInt
    {
        private static readonly Dictionary<string, CachedInt> _values = new();
        private static bool _isDirty;

        private readonly string _key;
        private int _value;

        private CachedInt(string key, int defaultValue)
        {
            _key = key;
            _value = PlayerPrefs.GetInt(key, defaultValue);
        }

        public int Value
        {
            get => _value;
            set
            {
                if (_value == value)
                {
                    return;
                }

                _value = value;
                PlayerPrefs.SetInt(_key, value);
                _isDirty = true;
            }
        }

        public static CachedInt Get(string key, int defaultValue = 0)
        {
            if (!_values.TryGetValue(key, out var value))
            {
                value = new CachedInt(key, defaultValue);
                _values.Add(key, value);
            }

            return value;
        }

        public static void Save()
        {
            if (!_isDirty)
            {
                return;
            }

            PlayerPrefs.Save();
            _isDirty = false;
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetCache()
        {
            _values.Clear();
            _isDirty = false;
        }
    }
}
