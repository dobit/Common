using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Threading.Tasks;

namespace System.Threading
{
    /// <summary>Caches asynchronously retrieved data.</summary>
    /// <typeparam name="TKey">Specifies the type of the cache's keys.</typeparam>
    /// <typeparam name="TValue">Specifies the type of the cache's values.</typeparam>
    [DebuggerTypeProxy(typeof(AsyncCache_DebugView<,>)), DebuggerDisplay("Count={Count}")]
    public class AsyncCache<TKey, TValue> : ICollection<KeyValuePair<TKey, Task<TValue>>>, IEnumerable<KeyValuePair<TKey, Task<TValue>>>, IEnumerable
    {
        /// <summary>The dictionary to store all of the tasks.</summary>
        private readonly ConcurrentDictionary<TKey, Lazy<Task<TValue>>> _map;
        /// <summary>The factory to use to create tasks.</summary>
        private readonly Func<TKey, Task<TValue>> _valueFactory;

        /// <summary>Initializes the cache.</summary>
        /// <param name="valueFactory">A factory for producing the cache's values.</param>
        public AsyncCache(Func<TKey, Task<TValue>> valueFactory)
        {
            if (valueFactory == null)
            {
                throw new ArgumentNullException("loader");
            }
            this._valueFactory = valueFactory;
            this._map = new ConcurrentDictionary<TKey, Lazy<Task<TValue>>>();
        }

        /// <summary>Empties the cache.</summary>
        public void Clear()
        {
            this._map.Clear();
        }

        /// <summary>Gets an enumerator for the contents of the cache.</summary>
        /// <returns>An enumerator for the contents of the cache.</returns>
        public IEnumerator<KeyValuePair<TKey, Task<TValue>>> GetEnumerator()
        {
            return (from p in this._map select new KeyValuePair<TKey, Task<TValue>>(p.Key, p.Value.Value)).GetEnumerator();
        }

        /// <summary>Gets a Task to retrieve the value for the specified key.</summary>
        /// <param name="key">The key whose value should be retrieved.</param>
        /// <returns>A Task for the value of the specified key.</returns>
        public Task<TValue> GetValue(TKey key)
        {
            if (key == null)
            {
                throw new ArgumentNullException("key");
            }
            Lazy<Task<TValue>> lazy = new Lazy<Task<TValue>>(() => ((AsyncCache<TKey, TValue>) this)._valueFactory(key));
            return this._map.GetOrAdd(key, lazy).Value;
        }

        /// <summary>Sets the value for the specified key.</summary>
        /// <param name="key">The key whose value should be set.</param>
        /// <param name="value">The value to which the key should be set.</param>
        public void SetValue(TKey key, TValue value)
        {
            this.SetValue(key, Task.Factory.FromResult<TValue>(value));
        }

        /// <summary>Sets the value for the specified key.</summary>
        /// <param name="key">The key whose value should be set.</param>
        /// <param name="value">The value to which the key should be set.</param>
        public void SetValue(TKey key, Task<TValue> value)
        {
            if (key == null)
            {
                throw new ArgumentNullException("key");
            }
            this._map[key] = LazyExtensions.Create<Task<TValue>>(value);
        }

        void ICollection<KeyValuePair<TKey, Task<TValue>>>.Add(KeyValuePair<TKey, Task<TValue>> item)
        {
            this[item.Key] = item.Value;
        }

        bool ICollection<KeyValuePair<TKey, Task<TValue>>>.Contains(KeyValuePair<TKey, Task<TValue>> item)
        {
            return this._map.ContainsKey(item.Key);
        }

        void ICollection<KeyValuePair<TKey, Task<TValue>>>.CopyTo(KeyValuePair<TKey, Task<TValue>>[] array, int arrayIndex)
        {
            ((ICollection<KeyValuePair<TKey, Task<TValue>>>) this._map).CopyTo(array, arrayIndex);
        }

        bool ICollection<KeyValuePair<TKey, Task<TValue>>>.Remove(KeyValuePair<TKey, Task<TValue>> item)
        {
            Lazy<Task<TValue>> lazy;
            return this._map.TryRemove(item.Key, out lazy);
        }

        /// <summary>Gets an enumerator for the contents of the cache.</summary>
        /// <returns>An enumerator for the contents of the cache.</returns>
        IEnumerator IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
        }

        /// <summary>Gets the number of items in the cache.</summary>
        public int Count
        {
            get
            {
                return this._map.Count;
            }
        }

        /// <summary>Gets a Task to retrieve the value for the specified key.</summary>
        /// <param name="key">The key whose value should be retrieved.</param>
        /// <returns>A Task for the value of the specified key.</returns>
        public Task<TValue> this[TKey key]
        {
            get
            {
                return this.GetValue(key);
            }
            set
            {
                this.SetValue(key, value);
            }
        }

        bool ICollection<KeyValuePair<TKey, Task<TValue>>>.IsReadOnly
        {
            get
            {
                return false;
            }
        }
    }
}

