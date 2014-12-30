using System;
using System.Collections.Concurrent;

namespace LFNet.Common.Extensions
{
    /// <summary>
    /// http://msdn.microsoft.com/en-us/library/dd997369.aspx
    /// http://kozmic.pl/2010/08/06/concurrentdictionary-in-net-4-not-what-you-would-expect/
    /// http://codereview.stackexchange.com/questions/2025/extension-methods-to-make-concurrentdictionary-getoradd-and-addorupdate-thread-sa
    /// </summary>
    public static class ConcurrentDictionaryExtensions
    {
        public static K AddOrUpdateSafe<T, K>(this ConcurrentDictionary<T, Lazy<K>> dictionary, T key, Func<T, K> addValueFactory, Func<T, K, K> updateValueFactory)
        {
            return dictionary.AddOrUpdate(key, new Lazy<K>(() => addValueFactory(key)), (k, oldValue) => new Lazy<K>(() => updateValueFactory(k, oldValue.Value))).Value;
        }

        public static K GetOrAddSafe<T, K>(this ConcurrentDictionary<T, Lazy<K>> dictionary, T key, Func<T, K> valueFactory)
        {
            return dictionary.GetOrAdd(key, new Lazy<K>(() => valueFactory(key))).Value;
        }
    }
}

