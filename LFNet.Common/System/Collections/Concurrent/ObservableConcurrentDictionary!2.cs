using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Diagnostics;
using System.Threading;

namespace System.Collections.Concurrent
{
    /// <summary>
    /// Provides a thread-safe dictionary for use with data binding.
    /// </summary>
    /// <typeparam name="TKey">Specifies the type of the keys in this collection.</typeparam>
    /// <typeparam name="TValue">Specifies the type of the values in this collection.</typeparam>
    [DebuggerDisplay("Count={Count}")]
    public class ObservableConcurrentDictionary<TKey, TValue> : IDictionary<TKey, TValue>, ICollection<KeyValuePair<TKey, TValue>>, IEnumerable<KeyValuePair<TKey, TValue>>, IEnumerable, INotifyCollectionChanged, INotifyPropertyChanged
    {
        private readonly SynchronizationContext _context;
        private readonly ConcurrentDictionary<TKey, TValue> _dictionary;

        /// <summary>Event raised when the collection changes.</summary>
        public event NotifyCollectionChangedEventHandler CollectionChanged;

        /// <summary>Event raised when a property on the collection changes.</summary>
        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// Initializes an instance of the ObservableConcurrentDictionary class.
        /// </summary>
        public ObservableConcurrentDictionary()
        {
            this._context = AsyncOperationManager.SynchronizationContext;
            this._dictionary = new ConcurrentDictionary<TKey, TValue>();
        }

        /// <summary>
        /// Initializes an instance of the ObservableConcurrentDictionary class.
        /// </summary>
        public ObservableConcurrentDictionary(IEqualityComparer<TKey> comparer)
        {
            this._context = AsyncOperationManager.SynchronizationContext;
            this._dictionary = new ConcurrentDictionary<TKey, TValue>(comparer);
        }

        public void Add(TKey key, TValue value)
        {
            this.TryAddWithNotification(key, value);
        }

        public bool ContainsKey(TKey key)
        {
            return this._dictionary.ContainsKey(key);
        }

        /// <summary>
        /// Notifies observers of CollectionChanged or PropertyChanged of an update to the dictionary.
        /// </summary>
        private void NotifyObserversOfChange()
        {
            SendOrPostCallback d = null;
            NotifyCollectionChangedEventHandler collectionHandler = this.CollectionChanged;
            PropertyChangedEventHandler propertyHandler = this.PropertyChanged;
            if ((collectionHandler != null) || (propertyHandler != null))
            {
                if (d == null)
                {
                    d = delegate (object s) {
                        if (collectionHandler != null)
                        {
                            collectionHandler((ObservableConcurrentDictionary<TKey, TValue>) this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
                        }
                        if (propertyHandler != null)
                        {
                            propertyHandler((ObservableConcurrentDictionary<TKey, TValue>) this, new PropertyChangedEventArgs("Count"));
                            propertyHandler((ObservableConcurrentDictionary<TKey, TValue>) this, new PropertyChangedEventArgs("Keys"));
                            propertyHandler((ObservableConcurrentDictionary<TKey, TValue>) this, new PropertyChangedEventArgs("Values"));
                        }
                    };
                }
                this._context.Post(d, null);
            }
        }

        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChangedEventHandler propertyChanged = this.PropertyChanged;
            if (propertyChanged != null)
            {
                propertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        public bool Remove(TKey key)
        {
            TValue local;
            return this.TryRemoveWithNotification(key, out local);
        }

        void ICollection<KeyValuePair<TKey, TValue>>.Add(KeyValuePair<TKey, TValue> item)
        {
            this.TryAddWithNotification(item);
        }

        void ICollection<KeyValuePair<TKey, TValue>>.Clear()
        {
            ((ICollection<KeyValuePair<TKey, TValue>>)this._dictionary).Clear();
            this.NotifyObserversOfChange();
        }

        bool ICollection<KeyValuePair<TKey, TValue>>.Contains(KeyValuePair<TKey, TValue> item)
        {
            return ((ICollection<KeyValuePair<TKey, TValue>>)this._dictionary).Contains(item);
        }

        void ICollection<KeyValuePair<TKey, TValue>>.CopyTo(KeyValuePair<TKey, TValue>[] array, int arrayIndex)
        {
            ((ICollection<KeyValuePair<TKey, TValue>>)this._dictionary).CopyTo(array, arrayIndex);
        }

        bool ICollection<KeyValuePair<TKey, TValue>>.Remove(KeyValuePair<TKey, TValue> item)
        {
            TValue local;
            return this.TryRemoveWithNotification(item.Key, out local);
        }

        IEnumerator<KeyValuePair<TKey, TValue>> IEnumerable<KeyValuePair<TKey, TValue>>.GetEnumerator()
        {
            return ((IEnumerable<KeyValuePair<TKey, TValue>>)this._dictionary).GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return ((ICollection<KeyValuePair<TKey, TValue>>)this._dictionary).GetEnumerator();
        }

        /// <summary>Attempts to add an item to the dictionary, notifying observers of any changes.</summary>
        /// <param name="item">The item to be added.</param>
        /// <returns>Whether the add was successful.</returns>
        private bool TryAddWithNotification(KeyValuePair<TKey, TValue> item)
        {
            return this.TryAddWithNotification(item.Key, item.Value);
        }

        /// <summary>Attempts to add an item to the dictionary, notifying observers of any changes.</summary>
        /// <param name="key">The key of the item to be added.</param>
        /// <param name="value">The value of the item to be added.</param>
        /// <returns>Whether the add was successful.</returns>
        private bool TryAddWithNotification(TKey key, TValue value)
        {
            bool flag = this._dictionary.TryAdd(key, value);
            if (flag)
            {
                this.NotifyObserversOfChange();
            }
            return flag;
        }

        public bool TryGetValue(TKey key, out TValue value)
        {
            return this._dictionary.TryGetValue(key, out value);
        }

        /// <summary>Attempts to remove an item from the dictionary, notifying observers of any changes.</summary>
        /// <param name="key">The key of the item to be removed.</param>
        /// <param name="value">The value of the item removed.</param>
        /// <returns>Whether the removal was successful.</returns>
        private bool TryRemoveWithNotification(TKey key, out TValue value)
        {
            bool flag = this._dictionary.TryRemove(key, out value);
            if (flag)
            {
                this.NotifyObserversOfChange();
            }
            return flag;
        }

        /// <summary>Attempts to add or update an item in the dictionary, notifying observers of any changes.</summary>
        /// <param name="key">The key of the item to be updated.</param>
        /// <param name="value">The new value to set for the item.</param>
        /// <returns>Whether the update was successful.</returns>
        private void UpdateWithNotification(TKey key, TValue value)
        {
            this._dictionary[key] = value;
            this.NotifyObserversOfChange();
        }

        public TValue this[TKey key]
        {
            get
            {
                return this._dictionary[key];
            }
            set
            {
                this.UpdateWithNotification(key, value);
            }
        }

        public ICollection<TKey> Keys
        {
            get
            {
                return this._dictionary.Keys;
            }
        }

        int ICollection<KeyValuePair<TKey, TValue>>.Count
        {
            get
            {
                return ((ICollection<KeyValuePair<TKey, TValue>>)this._dictionary).Count;
            }
        }

        bool ICollection<KeyValuePair<TKey, TValue>>.IsReadOnly
        {
            get
            {
                return ((ICollection<KeyValuePair<TKey, TValue>>)this._dictionary).IsReadOnly;
            }
        }

        public ICollection<TValue> Values
        {
            get
            {
                return this._dictionary.Values;
            }
        }
    }
}

