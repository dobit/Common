using System;
using System.Collections.Generic;
using System.Linq;

namespace LFNet.Common.Extensions
{
    public static class DictionaryExtensions
    {
        public static void Add<TKey, TValue>(this Dictionary<TKey, TValue> map, IEnumerable<TKey> keys, IEnumerable<TValue> values)
        {
            if (keys.Count<TKey>() != values.Count<TValue>())
            {
                throw new ArgumentException("Keys and values must be matching length.");
            }
            IEnumerator<TKey> enumerator = keys.GetEnumerator();
            IEnumerator<TValue> enumerator2 = values.GetEnumerator();
            while (enumerator.MoveNext() && enumerator2.MoveNext())
            {
                if (!map.ContainsKey(enumerator.Current))
                {
                    map.Add(enumerator.Current, enumerator2.Current);
                }
            }
        }

        public static void ApplyIf<TKey, TValue>(this Dictionary<TKey, TValue> map1, Dictionary<TKey, TValue> map2)
        {
            foreach (KeyValuePair<TKey, TValue> pair in map2)
            {
                if (!map1.ContainsKey(pair.Key))
                {
                    map1.Add(pair.Key, pair.Value);
                }
            }
        }

        public static bool ContainsKeyAndValueIsNullOrEmtpy(this Dictionary<string, object> map, string key)
        {
            if (!map.ContainsKey(key))
            {
                return false;
            }
            if (map[key] != null)
            {
                return map[key].ToString().IsNullOrEmpty();
            }
            return true;
        }
    }
}

