using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Runtime.Serialization;
using System.Security.Permissions;
using System.Threading;

namespace LFNet.Common.Collections
{
    /// <summary>
    /// Represents a thread-safe collection of keys and values. 
    /// </summary> 
    /// <typeparam name="TKey">The type of the keys in the dictionary.</typeparam>
    /// <typeparam name="TValue">The type of the values in the dictionary.</typeparam> 
    /// <remarks>
    /// All public and protected members of <see cref="T:LFNet.Common.Collections.ConcurrentDictionary`2" /> are thread-safe and may be used
    /// concurrently from multiple threads.
    /// </remarks> 
    [DebuggerDisplay("Count = {Count}"), ComVisible(false)]
    [HostProtection(SecurityAction.LinkDemand, Synchronization = true, ExternalThreading = true)]
    [Serializable]
    public class ConcurrentDictionary<TKey, TValue> : IDictionary<TKey, TValue>, ICollection<KeyValuePair<TKey, TValue>>, IEnumerable<KeyValuePair<TKey, TValue>>, IDictionary, ICollection, IEnumerable, INotifyPropertyChanged
    {
        /// <summary>
        /// A node in a singly-linked list representing a particular hash table bucket. 
        /// </summary>
        private class Node
        {
            internal TKey m_key;
            internal TValue m_value;
            internal volatile ConcurrentDictionary<TKey, TValue>.Node m_next;
            internal int m_hashcode;
            internal Node(TKey key, TValue value, int hashcode)
                : this(key, value, hashcode, null)
            {
            }
            internal Node(TKey key, TValue value, int hashcode, ConcurrentDictionary<TKey, TValue>.Node next)
            {
                this.m_key = key;
                this.m_value = value;
                this.m_next = next;
                this.m_hashcode = hashcode;
            }
        }
        /// <summary>
        /// A private class to represent enumeration over the dictionary that implements the 
        /// IDictionaryEnumerator interface. 
        /// </summary>
        private class DictionaryEnumerator : IDictionaryEnumerator, IEnumerator
        {
            private IEnumerator<KeyValuePair<TKey, TValue>> m_enumerator;
            public DictionaryEntry Entry
            {
                get
                {
                    KeyValuePair<TKey, TValue> current = this.m_enumerator.Current;
                    object arg_30_0 = current.Key;
                    KeyValuePair<TKey, TValue> current2 = this.m_enumerator.Current;
                    return new DictionaryEntry(arg_30_0, current2.Value);
                }
            }
            public object Key
            {
                get
                {
                    KeyValuePair<TKey, TValue> current = this.m_enumerator.Current;
                    return current.Key;
                }
            }
            public object Value
            {
                get
                {
                    KeyValuePair<TKey, TValue> current = this.m_enumerator.Current;
                    return current.Value;
                }
            }
            public object Current
            {
                get
                {
                    return this.Entry;
                }
            }
            internal DictionaryEnumerator(ConcurrentDictionary<TKey, TValue> dictionary)
            {
                this.m_enumerator = dictionary.GetEnumerator();
            }
            public bool MoveNext()
            {
                return this.m_enumerator.MoveNext();
            }
            public void Reset()
            {
                this.m_enumerator.Reset();
            }
        }
        private const string COUNT_STRING = "Count";
        private const string INDEXER_NAME = "Item[]";
        private const int DEFAULT_CONCURRENCY_MULTIPLIER = 4;
        private const int DEFAULT_CAPACITY = 31;
        [NonSerialized]
        private volatile ConcurrentDictionary<TKey, TValue>.Node[] m_buckets;
        [NonSerialized]
        private object[] m_locks;
        [NonSerialized]
        private volatile int[] m_countPerLock;
        private IEqualityComparer<TKey> m_comparer;
        private KeyValuePair<TKey, TValue>[] m_serializationArray;
        private int m_serializationConcurrencyLevel;
        private int m_serializationCapacity;
        /// <summary>
        /// Occurs when a property value changed.
        /// </summary>
        [field: NonSerialized]
        public event PropertyChangedEventHandler PropertyChanged;
        /// <summary>
        /// Occurs when the collection changed.
        /// </summary>
        [field: NonSerialized]
        public event EventHandler CollectionChanged;
        /// <summary>
        /// Gets or sets the value associated with the specified key. 
        /// </summary> 
        /// <param name="key">The key of the value to get or set.</param>
        /// <value>The value associated with the specified key. If the specified key is not found, a get 
        /// operation throws a
        /// <see cref="T:Sytem.Collections.Generic.KeyNotFoundException" />, and a set operation creates a new
        /// element with the specified key.</value>
        /// <exception cref="T:System.ArgumentNullException"><paramref name="key" /> is a null reference 
        /// (Nothing in Visual Basic).</exception>
        /// <exception cref="T:System.Collections.Generic.KeyNotFoundException">The property is retrieved and 
        /// <paramref name="key" /> 
        /// does not exist in the collection.</exception>
        public TValue this[TKey key]
        {
            get
            {
                TValue result;
                if (!this.TryGetValue(key, out result))
                {
                    throw new KeyNotFoundException();
                }
                return result;
            }
            set
            {
                if (key == null)
                {
                    throw new ArgumentNullException("key");
                }
                TValue tValue;
                this.TryAddInternal(key, value, true, true, out tValue);
            }
        }
        /// <summary>
        /// Gets the number of key/value pairs contained in the <see cref="T:LFNet.Common.Collections.ConcurrentDictionary`2" />.
        /// </summary> 
        /// <exception cref="T:System.OverflowException">The dictionary contains too many
        /// elements.</exception> 
        /// <value>The number of key/value paris contained in the <see cref="T:LFNet.Common.Collections.ConcurrentDictionary`2" />.</value>
        /// <remarks>Count has snapshot semantics and represents the number of items in the <see cref="T:LFNet.Common.Collections.ConcurrentDictionary`2" />
        /// at the moment when Count was accessed.</remarks>
        public int Count
        {
            get
            {
                int num = 0;
                int toExclusive = 0;
                try
                {
                    this.AcquireAllLocks(ref toExclusive);
                    for (int i = 0; i < this.m_countPerLock.Length; i++)
                    {
                        num += this.m_countPerLock[i];
                    }
                }
                finally
                {
                    this.ReleaseLocks(0, toExclusive);
                }
                return num;
            }
        }
        /// <summary> 
        /// Gets a value that indicates whether the <see cref="T:LFNet.Common.Collections.ConcurrentDictionary`2" /> is empty. 
        /// </summary>
        /// <value>true if the <see cref="T:LFNet.Common.Collections.ConcurrentDictionary`2" /> is empty; otherwise, 
        /// false.</value>
        public bool IsEmpty
        {
            get
            {
                int toExclusive = 0;
                try
                {
                    this.AcquireAllLocks(ref toExclusive);
                    for (int i = 0; i < this.m_countPerLock.Length; i++)
                    {
                        if (this.m_countPerLock[i] != 0)
                        {
                            return false;
                        }
                    }
                }
                finally
                {
                    this.ReleaseLocks(0, toExclusive);
                }
                return true;
            }
        }
        /// <summary>
        /// Gets a collection containing the keys in the <see cref="T:System.Collections.Generic.Dictionary{TKey,TValue}" />.
        /// </summary>
        /// <value>An <see cref="T:System.Collections.Generic.ICollection{TKey}" /> containing the keys in the
        /// <see cref="T:System.Collections.Generic.Dictionary{TKey,TValue}" />.</value> 
        public ICollection<TKey> Keys
        {
            get
            {
                return this.GetKeys();
            }
        }
        /// <summary>
        /// Gets a collection containing the values in the <see cref="T:System.Collections.Generic.Dictionary{TKey,TValue}" />.
        /// </summary> 
        /// <value>An <see cref="T:System.Collections.Generic.ICollection{TValue}" /> containing the values in
        /// the 
        /// <see cref="T:System.Collections.Generic.Dictionary{TKey,TValue}" />.</value> 
        public ICollection<TValue> Values
        {
            get
            {
                return this.GetValues();
            }
        }
        bool ICollection<KeyValuePair<TKey, TValue>>.IsReadOnly
        {
            get
            {
                return false;
            }
        }
        /// <summary> 
        /// Gets a value indicating whether the <see cref="T:System.Collections.Generic.IDictionary{TKey,TValue}" /> has a fixed size.
        /// </summary> 
        /// <value>true if the <see cref="T:System.Collections.Generic.IDictionary{TKey,TValue}" /> has a
        /// fixed size; otherwise, false. For <see cref="T:System.Collections.Generic.ConcurrentDictionary{TKey,TValue}" />, this property always
        /// returns false.</value> 
        bool IDictionary.IsFixedSize
        {
            get
            {
                return false;
            }
        }
        /// <summary>
        /// Gets a value indicating whether the <see cref="T:System.Collections.Generic.IDictionary{TKey,TValue}" /> is read-only.
        /// </summary> 
        /// <value>true if the <see cref="T:System.Collections.Generic.IDictionary{TKey,TValue}" /> is
        /// read-only; otherwise, false. For <see cref="T:System.Collections.Generic.ConcurrentDictionary{TKey,TValue}" />, this property always 
        /// returns false.</value>
        bool IDictionary.IsReadOnly
        {
            get
            {
                return false;
            }
        }
        /// <summary>
        /// Gets an <see cref="T:System.Collections.ICollection" /> containing the keys of the <see cref="T:System.Collections.Generic.IDictionary{TKey,TValue}" />. 
        /// </summary>
        /// <value>An <see cref="T:System.Collections.ICollection" /> containing the keys of the <see cref="T:System.Collections.Generic.IDictionary{TKey,TValue}" />.</value>
        ICollection IDictionary.Keys
        {
            get
            {
                return this.GetKeys();
            }
        }
        /// <summary> 
        /// Gets an <see cref="T:System.Collections.ICollection" /> containing the values in the <see cref="T:System.Collections.IDictionary" />. 
        /// </summary>
        /// <value>An <see cref="T:System.Collections.ICollection" /> containing the values in the <see cref="T:System.Collections.IDictionary" />.</value>
        ICollection IDictionary.Values
        {
            get
            {
                return this.GetValues();
            }
        }
        /// <summary> 
        /// Gets or sets the value associated with the specified key.
        /// </summary>
        /// <param name="key">The key of the value to get or set.</param>
        /// <value>The value associated with the specified key, or a null reference (Nothing in Visual Basic) 
        /// if <paramref name="key" /> is not in the dictionary or <paramref name="key" /> is of a type that is
        /// not assignable to the key type <typeparamref name="TKey" /> of the <see cref="T:System.Collections.Generic.ConcurrentDictionary{TKey,TValue}" />.</value> 
        /// <exception cref="T:System.ArgumentNullException"><paramref name="key" /> is a null reference
        /// (Nothing in Visual Basic).</exception> 
        /// <exception cref="T:System.ArgumentException">
        /// A value is being assigned, and <paramref name="key" /> is of a type that is not assignable to the
        /// key type <typeparamref name="TKey" /> of the <see cref="T:System.Collections.Generic.ConcurrentDictionary{TKey,TValue}" />. -or- A value is being 
        /// assigned, and <paramref name="key" /> is of a type that is not assignable to the value type
        /// <typeparamref name="TValue" /> of the <see cref="T:System.Collections.Generic.ConcurrentDictionary{TKey,TValue}" /> 
        /// </exception>
        object IDictionary.this[object key]
        {
            get
            {
                if (key == null)
                {
                    throw new ArgumentNullException("key");
                }
                TValue tValue;
                if (key is TKey && this.TryGetValue((TKey)((object)key), out tValue))
                {
                    return tValue;
                }
                return null;
            }
            set
            {
                if (key == null)
                {
                    throw new ArgumentNullException("key");
                }
                if (!(key is TKey))
                {
                    throw new ArgumentException(this.GetResource("ConcurrentDictionary_TypeOfKeyIncorrect"));
                }
                if (!(value is TValue))
                {
                    throw new ArgumentException(this.GetResource("ConcurrentDictionary_TypeOfValueIncorrect"));
                }
                this[(TKey)((object)key)] = (TValue)((object)value);
            }
        }
        /// <summary>
        /// Gets a value indicating whether access to the <see cref="T:System.Collections.ICollection" /> is 
        /// synchronized with the SyncRoot. 
        /// </summary>
        /// <value>true if access to the <see cref="T:System.Collections.ICollection" /> is synchronized 
        /// (thread safe); otherwise, false. For <see cref="T:System.Collections.Concurrent.ConcurrentDictionary{TKey,TValue}" />, this property always
        /// returns false.</value>
        bool ICollection.IsSynchronized
        {
            get
            {
                return false;
            }
        }
        /// <summary> 
        /// Gets an object that can be used to synchronize access to the <see cref="T:System.Collections.ICollection" />. This property is not supported.
        /// </summary>
        /// <exception cref="T:System.NotSupportedException">The SyncRoot property is not supported.</exception> 
        object ICollection.SyncRoot
        {
            get
            {
                throw new NotSupportedException();
            }
        }
        /// <summary> 
        /// The number of concurrent writes for which to optimize by default.
        /// </summary>
        private static int DefaultConcurrencyLevel
        {
            get
            {
                return 4 * Environment.ProcessorCount;
            }
        }
        /// <summary>
        /// Raises the PropertyChanged event with the provided arguments.
        /// </summary>
        /// <param name="e">The <see cref="T:System.ComponentModel.PropertyChangedEventArgs" /> instance containing the event data.</param>
        protected virtual void OnPropertyChanged(PropertyChangedEventArgs e)
        {
            if (this.PropertyChanged != null)
            {
                this.PropertyChanged(this, e);
            }
        }
        /// <summary>
        /// Raises the PropertyChanged event with the provided arguments.
        /// </summary>
        /// <param name="propertyName">Name of the property.</param>
        protected virtual void OnPropertyChanged(string propertyName)
        {
            this.OnPropertyChanged(new PropertyChangedEventArgs(propertyName));
        }
        /// <summary>
        /// Raises the CollectionChanged event with the provided arguments.
        /// </summary>
        protected virtual void OnCollectionChanged()
        {
            if (this.CollectionChanged != null)
            {
                this.CollectionChanged(this, EventArgs.Empty);
            }
        }
        /// <summary>
        /// Initializes a new instance of the <see cref="T:LFNet.Common.Collections.ConcurrentDictionary`2" /> 
        /// class that is empty, has the default concurrency level, has the default initial capacity, and
        /// uses the default comparer for the key type. 
        /// </summary> 
        public ConcurrentDictionary()
            : this(ConcurrentDictionary<TKey, TValue>.DefaultConcurrencyLevel, 31)
        {
        }
        /// <summary>
        /// Initializes a new instance of the <see cref="T:LFNet.Common.Collections.ConcurrentDictionary`2" />
        /// class that is empty, has the specified concurrency level and capacity, and uses the default 
        /// comparer for the key type.
        /// </summary> 
        /// <param name="concurrencyLevel">The estimated number of threads that will update the 
        /// <see cref="T:LFNet.Common.Collections.ConcurrentDictionary`2" /> concurrently.</param>
        /// <param name="capacity">The initial number of elements that the <see cref="T:LFNet.Common.Collections.ConcurrentDictionary`2" />
        /// can contain.</param>
        /// <exception cref="T:System.ArgumentOutOfRangeException"><paramref name="concurrencyLevel" /> is
        /// less than 1.</exception> 
        /// <exception cref="T:System.ArgumentOutOfRangeException"> <paramref name="capacity" /> is less than
        /// 0.</exception> 
        public ConcurrentDictionary(int concurrencyLevel, int capacity)
            : this(concurrencyLevel, capacity, EqualityComparer<TKey>.Default)
        {
        }
        /// <summary> 
        /// Initializes a new instance of the <see cref="T:CodeSmith.Core.Collections.ConcurrentDictionary`2" />
        /// class that contains elements copied from the specified <see cref="T:System.Collections.IEnumerable{KeyValuePair{TKey,TValue}}" />, has the default concurrency
        /// level, has the default initial capacity, and uses the default comparer for the key type. 
        /// </summary>
        /// <param name="collection">The <see cref="T:System.Collections.IEnumerable{KeyValuePair{TKey,TValue}}" /> whose elements are copied to 
        /// the new
        /// <see cref="T:CodeSmith.Core.Collections.ConcurrentDictionary`2" />.</param> 
        /// <exception cref="T:System.ArgumentNullException"><paramref name="collection" /> is a null reference
        /// (Nothing in Visual Basic).</exception>
        /// <exception cref="T:System.ArgumentException"><paramref name="collection" /> contains one or more
        /// duplicate keys.</exception> 
        public ConcurrentDictionary(IEnumerable<KeyValuePair<TKey, TValue>> collection)
            : this(collection, EqualityComparer<TKey>.Default)
        {
        }
        /// <summary> 
        /// Initializes a new instance of the <see cref="T:CodeSmith.Core.Collections.ConcurrentDictionary`2" />
        /// class that is empty, has the specified concurrency level and capacity, and uses the specified 
        /// <see cref="T:System.Collections.Generic.IEqualityComparer{TKey}" />.
        /// </summary>
        /// <param name="comparer">The <see cref="T:System.Collections.Generic.IEqualityComparer{TKey}" />
        /// implementation to use when comparing keys.</param> 
        /// <exception cref="T:System.ArgumentNullException"><paramref name="comparer" /> is a null reference
        /// (Nothing in Visual Basic).</exception> 
        public ConcurrentDictionary(IEqualityComparer<TKey> comparer)
            : this(ConcurrentDictionary<TKey, TValue>.DefaultConcurrencyLevel, 31, comparer)
        {
        }
        /// <summary> 
        /// Initializes a new instance of the <see cref="T:CodeSmith.Core.Collections.ConcurrentDictionary`2" />
        /// class that contains elements copied from the specified <see cref="T:System.Collections.IEnumerable" />, has the default concurrency level, has the default
        /// initial capacity, and uses the specified 
        /// <see cref="T:System.Collections.Generic.IEqualityComparer{TKey}" />.
        /// </summary> 
        /// <param name="collection">The <see cref="T:System.Collections.IEnumerable{KeyValuePair{TKey,TValue}}" /> whose elements are copied to
        /// the new 
        /// <see cref="T:CodeSmith.Core.Collections.ConcurrentDictionary`2" />.</param>
        /// <param name="comparer">The <see cref="T:System.Collections.Generic.IEqualityComparer{TKey}" />
        /// implementation to use when comparing keys.</param>
        /// <exception cref="T:System.ArgumentNullException"><paramref name="collection" /> is a null reference 
        /// (Nothing in Visual Basic). -or-
        /// <paramref name="comparer" /> is a null reference (Nothing in Visual Basic). 
        /// </exception> 
        public ConcurrentDictionary(IEnumerable<KeyValuePair<TKey, TValue>> collection, IEqualityComparer<TKey> comparer)
            : this(ConcurrentDictionary<TKey, TValue>.DefaultConcurrencyLevel, collection, comparer)
        {
        }
        /// <summary>
        /// Initializes a new instance of the <see cref="T:CodeSmith.Core.Collections.ConcurrentDictionary`2" />
        /// class that contains elements copied from the specified <see cref="T:System.Collections.IEnumerable" />, 
        /// has the specified concurrency level, has the specified initial capacity, and uses the specified
        /// <see cref="T:System.Collections.Generic.IEqualityComparer{TKey}" />. 
        /// </summary> 
        /// <param name="concurrencyLevel">The estimated number of threads that will update the
        /// <see cref="T:CodeSmith.Core.Collections.ConcurrentDictionary`2" /> concurrently.</param> 
        /// <param name="collection">The <see cref="T:System.Collections.IEnumerable{KeyValuePair{TKey,TValue}}" /> whose elements are copied to the new
        /// <see cref="T:CodeSmith.Core.Collections.ConcurrentDictionary`2" />.</param>
        /// <param name="comparer">The <see cref="T:System.Collections.Generic.IEqualityComparer{TKey}" /> implementation to use
        /// when comparing keys.</param> 
        /// <exception cref="T:System.ArgumentNullException">
        /// <paramref name="collection" /> is a null reference (Nothing in Visual Basic). 
        /// -or- 
        /// <paramref name="comparer" /> is a null reference (Nothing in Visual Basic).
        /// </exception> 
        /// <exception cref="T:System.ArgumentOutOfRangeException">
        /// <paramref name="concurrencyLevel" /> is less than 1.
        /// </exception>
        /// <exception cref="T:System.ArgumentException"><paramref name="collection" /> contains one or more duplicate keys.</exception> 
        public ConcurrentDictionary(int concurrencyLevel, IEnumerable<KeyValuePair<TKey, TValue>> collection, IEqualityComparer<TKey> comparer)
            : this(concurrencyLevel, 31, comparer)
        {
            if (collection == null)
            {
                throw new ArgumentNullException("collection");
            }
            if (comparer == null)
            {
                throw new ArgumentNullException("comparer");
            }
            this.InitializeFromCollection(collection);
        }
        private void InitializeFromCollection(IEnumerable<KeyValuePair<TKey, TValue>> collection)
        {
            foreach (KeyValuePair<TKey, TValue> current in collection)
            {
                if (current.Key == null)
                {
                    throw new ArgumentNullException("key");
                }
                TValue tValue;
                if (!this.TryAddInternal(current.Key, current.Value, false, false, out tValue))
                {
                    throw new ArgumentException(this.GetResource("ConcurrentDictionary_SourceContainsDuplicateKeys"));
                }
            }
        }
        /// <summary>
        /// Initializes a new instance of the <see cref="T:CodeSmith.Core.Collections.ConcurrentDictionary`2" />
        /// class that is empty, has the specified concurrency level, has the specified initial capacity, and 
        /// uses the specified <see cref="T:System.Collections.Generic.IEqualityComparer{TKey}" />.
        /// </summary> 
        /// <param name="concurrencyLevel">The estimated number of threads that will update the 
        /// <see cref="T:CodeSmith.Core.Collections.ConcurrentDictionary`2" /> concurrently.</param>
        /// <param name="capacity">The initial number of elements that the <see cref="T:CodeSmith.Core.Collections.ConcurrentDictionary`2" />
        /// can contain.</param>
        /// <param name="comparer">The <see cref="T:System.Collections.Generic.IEqualityComparer{TKey}" />
        /// implementation to use when comparing keys.</param> 
        /// <exception cref="T:System.ArgumentOutOfRangeException">
        /// <paramref name="concurrencyLevel" /> is less than 1. -or- 
        /// <paramref name="capacity" /> is less than 0. 
        /// </exception>
        /// <exception cref="T:System.ArgumentNullException"><paramref name="comparer" /> is a null reference 
        /// (Nothing in Visual Basic).</exception>
        public ConcurrentDictionary(int concurrencyLevel, int capacity, IEqualityComparer<TKey> comparer)
        {
            if (concurrencyLevel < 1)
            {
                throw new ArgumentOutOfRangeException("concurrencyLevel", this.GetResource("ConcurrentDictionary_ConcurrencyLevelMustBePositive"));
            }
            if (capacity < 0)
            {
                throw new ArgumentOutOfRangeException("capacity", this.GetResource("ConcurrentDictionary_CapacityMustNotBeNegative"));
            }
            if (comparer == null)
            {
                throw new ArgumentNullException("comparer");
            }
            if (capacity < concurrencyLevel)
            {
                capacity = concurrencyLevel;
            }
            this.m_locks = new object[concurrencyLevel];
            for (int i = 0; i < this.m_locks.Length; i++)
            {
                this.m_locks[i] = new object();
            }
            this.m_countPerLock = new int[this.m_locks.Length];
            this.m_buckets = new ConcurrentDictionary<TKey, TValue>.Node[capacity];
            this.m_comparer = comparer;
        }
        /// <summary> 
        /// Attempts to add the specified key and value to the <see cref="T:LFNet.Common.Collections.ConcurrentDictionary`2" />.
        /// </summary> 
        /// <param name="key">The key of the element to add.</param>
        /// <param name="value">The value of the element to add. The value can be a null reference (Nothing
        /// in Visual Basic) for reference types.</param>
        /// <returns>true if the key/value pair was added to the <see cref="T:LFNet.Common.Collections.ConcurrentDictionary`2" />
        /// successfully; otherwise, false.</returns> 
        /// <exception cref="T:System.ArgumentNullException"><paramref name="key" /> is null reference 
        /// (Nothing in Visual Basic).</exception>
        /// <exception cref="T:System.OverflowException">The <see cref="T:LFNet.Common.Collections.ConcurrentDictionary`2" /> 
        /// contains too many elements.</exception>
        public bool TryAdd(TKey key, TValue value)
        {
            if (key == null)
            {
                throw new ArgumentNullException("key");
            }
            TValue tValue;
            return this.TryAddInternal(key, value, false, true, out tValue);
        }
        /// <summary> 
        /// Determines whether the <see cref="T:LFNet.Common.Collections.ConcurrentDictionary`2" /> contains the specified
        /// key.
        /// </summary>
        /// <param name="key">The key to locate in the <see cref="T:LFNet.Common.Collections.ConcurrentDictionary`2" />.</param>
        /// <returns>true if the <see cref="T:LFNet.Common.Collections.ConcurrentDictionary`2" /> contains an element with 
        /// the specified key; otherwise, false.</returns> 
        /// <exception cref="T:System.ArgumentNullException"><paramref name="key" /> is a null reference
        /// (Nothing in Visual Basic).</exception> 
        public bool ContainsKey(TKey key)
        {
            if (key == null)
            {
                throw new ArgumentNullException("key");
            }
            TValue tValue;
            return this.TryGetValue(key, out tValue);
        }
        /// <summary> 
        /// Attempts to remove and return the the value with the specified key from the
        /// <see cref="T:LFNet.Common.Collections.ConcurrentDictionary`2" />.
        /// </summary>
        /// <param name="key">The key of the element to remove and return.</param> 
        /// <param name="value">When this method returns, <paramref name="value" /> contains the object removed from the
        /// <see cref="T:LFNet.Common.Collections.ConcurrentDictionary`2" /> or the default value of <typeparamref name="TValue" /> 
        /// if the operation failed.</param>
        /// <returns>true if an object was removed successfully; otherwise, false.</returns> 
        /// <exception cref="T:System.ArgumentNullException"><paramref name="key" /> is a null reference
        /// (Nothing in Visual Basic).</exception>
        public bool TryRemove(TKey key, out TValue value)
        {
            if (key == null)
            {
                throw new ArgumentNullException("key");
            }
            return this.TryRemoveInternal(key, out value, false, default(TValue));
        }
        /// <summary>
        /// Removes the specified key from the dictionary if it exists and returns its associated value.
        /// If matchValue flag is set, the key will be removed only if is associated with a particular
        /// value. 
        /// </summary>
        /// <param name="key">The key to search for and remove if it exists.</param> 
        /// <param name="value">The variable into which the removed value, if found, is stored.</param> 
        /// <param name="matchValue">Whether removal of the key is conditional on its value.</param>
        /// <param name="oldValue">The conditional value to compare against if <paramref name="matchValue" /> is true</param> 
        /// <returns></returns>
        private bool TryRemoveInternal(TKey key, out TValue value, bool matchValue, TValue oldValue)
        {
            while (true)
            {
                ConcurrentDictionary<TKey, TValue>.Node[] buckets = this.m_buckets;
                int num;
                int num2;
                this.GetBucketAndLockNo(this.m_comparer.GetHashCode(key), out num, out num2, buckets.Length);
                lock (this.m_locks[num2])
                {
                    if (buckets != this.m_buckets)
                    {
                        continue;
                    }
                    ConcurrentDictionary<TKey, TValue>.Node node = null;
                    ConcurrentDictionary<TKey, TValue>.Node node2 = this.m_buckets[num];
                    while (node2 != null)
                    {
                        if (this.m_comparer.Equals(node2.m_key, key))
                        {
                            bool result;
                            if (matchValue && !EqualityComparer<TValue>.Default.Equals(oldValue, node2.m_value))
                            {
                                value = default(TValue);
                                result = false;
                                return result;
                            }
                            if (node == null)
                            {
                                this.m_buckets[num] = node2.m_next;
                            }
                            else
                            {
                                node.m_next = node2.m_next;
                            }
                            value = node2.m_value;
                            this.m_countPerLock[num2]--;
                            this.OnPropertyChanged("Count");
                            this.OnPropertyChanged("Item[]");
                            this.OnCollectionChanged();
                            result = true;
                            return result;
                        }
                        else
                        {
                            node = node2;
                            node2 = node2.m_next;
                        }
                    }
                }
                break;
            }
            value = default(TValue);
            return false;
        }
        /// <summary>
        /// Attempts to get the value associated with the specified key from the <see cref="T:LFNet.Common.Collections.ConcurrentDictionary`2" />. 
        /// </summary>
        /// <param name="key">The key of the value to get.</param> 
        /// <param name="value">When this method returns, <paramref name="value" /> contains the object from
        /// the
        /// <see cref="T:LFNet.Common.Collections.ConcurrentDictionary`2" /> with the spedified key or the default value of
        /// <typeparamref name="TValue" />, if the operation failed.</param> 
        /// <returns>true if the key was found in the <see cref="T:LFNet.Common.Collections.ConcurrentDictionary`2" />;
        /// otherwise, false.</returns> 
        /// <exception cref="T:System.ArgumentNullException"><paramref name="key" /> is a null reference 
        /// (Nothing in Visual Basic).</exception>
        public bool TryGetValue(TKey key, out TValue value)
        {
            if (key == null)
            {
                throw new ArgumentNullException("key");
            }
            ConcurrentDictionary<TKey, TValue>.Node[] buckets = this.m_buckets;
            int num;
            int num2;
            this.GetBucketAndLockNo(this.m_comparer.GetHashCode(key), out num, out num2, buckets.Length);
            ConcurrentDictionary<TKey, TValue>.Node node = buckets[num];
            Thread.MemoryBarrier();
            while (node != null)
            {
                if (this.m_comparer.Equals(node.m_key, key))
                {
                    value = node.m_value;
                    return true;
                }
                node = node.m_next;
            }
            value = default(TValue);
            return false;
        }
        /// <summary>
        /// Compares the existing value for the specified key with a specified value, and if they’re equal,
        /// updates the key with a third value. 
        /// </summary>
        /// <param name="key">The key whose value is compared with <paramref name="comparisonValue" /> and 
        /// possibly replaced.</param> 
        /// <param name="newValue">The value that replaces the value of the element with <paramref name="key" /> if the comparison results in equality.</param> 
        /// <param name="comparisonValue">The value that is compared to the value of the element with
        /// <paramref name="key" />.</param>
        /// <returns>true if the value with <paramref name="key" /> was equal to <paramref name="comparisonValue" /> and replaced with <paramref name="newValue" />; otherwise, 
        /// false.</returns>
        /// <exception cref="T:System.ArgumentNullException"><paramref name="key" /> is a null 
        /// reference.</exception> 
        public bool TryUpdate(TKey key, TValue newValue, TValue comparisonValue)
        {
            if (key == null)
            {
                throw new ArgumentNullException("key");
            }
            int hashCode = this.m_comparer.GetHashCode(key);
            IEqualityComparer<TValue> @default = EqualityComparer<TValue>.Default;
            bool result;
            while (true)
            {
                ConcurrentDictionary<TKey, TValue>.Node[] buckets = this.m_buckets;
                int num;
                int num2;
                this.GetBucketAndLockNo(hashCode, out num, out num2, buckets.Length);
                lock (this.m_locks[num2])
                {
                    if (buckets != this.m_buckets)
                    {
                        continue;
                    }
                    ConcurrentDictionary<TKey, TValue>.Node node = null;
                    ConcurrentDictionary<TKey, TValue>.Node node2 = buckets[num];
                    while (node2 != null)
                    {
                        if (this.m_comparer.Equals(node2.m_key, key))
                        {
                            if (@default.Equals(node2.m_value, comparisonValue))
                            {
                                ConcurrentDictionary<TKey, TValue>.Node node3 = new ConcurrentDictionary<TKey, TValue>.Node(node2.m_key, newValue, hashCode, node2.m_next);
                                if (node == null)
                                {
                                    buckets[num] = node3;
                                }
                                else
                                {
                                    node.m_next = node3;
                                }
                                this.OnPropertyChanged("Count");
                                this.OnPropertyChanged("Item[]");
                                this.OnCollectionChanged();
                                result = true;
                                return result;
                            }
                            result = false;
                            return result;
                        }
                        else
                        {
                            node = node2;
                            node2 = node2.m_next;
                        }
                    }
                    result = false;
                }
                break;
            }
            return result;
        }
        /// <summary> 
        /// Removes all keys and values from the <see cref="T:LFNet.Common.Collections.ConcurrentDictionary`2" />.
        /// </summary> 
        public void Clear()
        {
            int toExclusive = 0;
            try
            {
                this.AcquireAllLocks(ref toExclusive);
                this.m_buckets = new ConcurrentDictionary<TKey, TValue>.Node[31];
                Array.Clear(this.m_countPerLock, 0, this.m_countPerLock.Length);
                this.OnPropertyChanged("Count");
                this.OnPropertyChanged("Item[]");
                this.OnCollectionChanged();
            }
            finally
            {
                this.ReleaseLocks(0, toExclusive);
            }
        }
        void ICollection<KeyValuePair<TKey, TValue>>.CopyTo(KeyValuePair<TKey, TValue>[] array, int index)
        {
            if (array == null)
            {
                throw new ArgumentNullException("array");
            }
            if (index < 0)
            {
                throw new ArgumentOutOfRangeException("index", this.GetResource("ConcurrentDictionary_IndexIsNegative"));
            }
            int toExclusive = 0;
            try
            {
                this.AcquireAllLocks(ref toExclusive);
                int num = 0;
                for (int i = 0; i < this.m_locks.Length; i++)
                {
                    num += this.m_countPerLock[i];
                }
                if (array.Length - num < index || num < 0)
                {
                    throw new ArgumentException(this.GetResource("ConcurrentDictionary_ArrayNotLargeEnough"));
                }
                this.CopyToPairs(array, index);
            }
            finally
            {
                this.ReleaseLocks(0, toExclusive);
            }
        }
        /// <summary>
        /// Copies the key and value pairs stored in the <see cref="T:LFNet.Common.Collections.ConcurrentDictionary`2" /> to a 
        /// new array. 
        /// </summary>
        /// <returns>A new array containing a snapshot of key and value pairs copied from the <see cref="T:LFNet.Common.Collections.ConcurrentDictionary`2" />.</returns>
        public KeyValuePair<TKey, TValue>[] ToArray()
        {
            int toExclusive = 0;
            checked
            {
                KeyValuePair<TKey, TValue>[] result;
                try
                {
                    this.AcquireAllLocks(ref toExclusive);
                    int num = 0;
                    for (int i = 0; i < this.m_locks.Length; i++)
                    {
                        num += this.m_countPerLock[i];
                    }
                    KeyValuePair<TKey, TValue>[] array = new KeyValuePair<TKey, TValue>[num];
                    this.CopyToPairs(array, 0);
                    result = array;
                }
                finally
                {
                    this.ReleaseLocks(0, toExclusive);
                }
                return result;
            }
        }
        /// <summary>
        /// Copy dictionary contents to an array - shared implementation between ToArray and CopyTo.
        ///
        /// Important: the caller must hold all locks in m_locks before calling CopyToPairs. 
        /// </summary>
        private void CopyToPairs(KeyValuePair<TKey, TValue>[] array, int index)
        {
            ConcurrentDictionary<TKey, TValue>.Node[] buckets = this.m_buckets;
            for (int i = 0; i < buckets.Length; i++)
            {
                for (ConcurrentDictionary<TKey, TValue>.Node node = buckets[i]; node != null; node = node.m_next)
                {
                    array[index] = new KeyValuePair<TKey, TValue>(node.m_key, node.m_value);
                    index++;
                }
            }
        }
        /// <summary>
        /// Copy dictionary contents to an array - shared implementation between ToArray and CopyTo.
        ///
        /// Important: the caller must hold all locks in m_locks before calling CopyToEntries. 
        /// </summary>
        private void CopyToEntries(DictionaryEntry[] array, int index)
        {
            ConcurrentDictionary<TKey, TValue>.Node[] buckets = this.m_buckets;
            for (int i = 0; i < buckets.Length; i++)
            {
                for (ConcurrentDictionary<TKey, TValue>.Node node = buckets[i]; node != null; node = node.m_next)
                {
                    array[index] = new DictionaryEntry(node.m_key, node.m_value);
                    index++;
                }
            }
        }
        /// <summary>
        /// Copy dictionary contents to an array - shared implementation between ToArray and CopyTo.
        ///
        /// Important: the caller must hold all locks in m_locks before calling CopyToObjects. 
        /// </summary>
        private void CopyToObjects(object[] array, int index)
        {
            ConcurrentDictionary<TKey, TValue>.Node[] buckets = this.m_buckets;
            for (int i = 0; i < buckets.Length; i++)
            {
                for (ConcurrentDictionary<TKey, TValue>.Node node = buckets[i]; node != null; node = node.m_next)
                {
                    array[index] = new KeyValuePair<TKey, TValue>(node.m_key, node.m_value);
                    index++;
                }
            }
        }
        /// <summary>Returns an enumerator that iterates through the <see cref="T:LFNet.Common.Collections.ConcurrentDictionary`2" />.</summary>
        /// <returns>An enumerator for the <see cref="T:LFNet.Common.Collections.ConcurrentDictionary`2" />.</returns>
        /// <remarks> 
        /// The enumerator returned from the dictionary is safe to use concurrently with
        /// reads and writes to the dictionary, however it does not represent a moment-in-time snapshot 
        /// of the dictionary.  The contents exposed through the enumerator may contain modifications 
        /// made to the dictionary after <see cref="M:LFNet.Common.Collections.ConcurrentDictionary`2.GetEnumerator" /> was called.
        /// </remarks> 
        public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator()
        {
            ConcurrentDictionary<TKey, TValue>.Node[] buckets = this.m_buckets;
            for (int i = 0; i < buckets.Length; i++)
            {
                ConcurrentDictionary<TKey, TValue>.Node node = buckets[i];
                Thread.MemoryBarrier();
                while (node != null)
                {
                    yield return new KeyValuePair<TKey, TValue>(node.m_key, node.m_value);
                    node = node.m_next;
                }
            }
            yield break;
        }
        /// <summary>
        /// Shared internal implementation for inserts and updates.
        /// If key exists, we always return false; and if updateIfExists == true we force update with value;
        /// If key doesn't exist, we always add value and return true; 
        /// </summary>
        private bool TryAddInternal(TKey key, TValue value, bool updateIfExists, bool acquireLock, out TValue resultingValue)
        {
            int hashCode = this.m_comparer.GetHashCode(key);
            checked
            {
                ConcurrentDictionary<TKey, TValue>.Node[] buckets;
                bool flag;
                while (true)
                {
                    buckets = this.m_buckets;
                    int num;
                    int num2;
                    this.GetBucketAndLockNo(hashCode, out num, out num2, buckets.Length);
                    flag = false;
                    bool flag2 = false;
                    try
                    {
                        if (acquireLock)
                        {
                            Monitor.Enter(this.m_locks[num2]);
                            flag2 = true;
                        }
                        if (buckets != this.m_buckets)
                        {
                            continue;
                        }
                        ConcurrentDictionary<TKey, TValue>.Node node = null;
                        for (ConcurrentDictionary<TKey, TValue>.Node node2 = buckets[num]; node2 != null; node2 = node2.m_next)
                        {
                            if (this.m_comparer.Equals(node2.m_key, key))
                            {
                                if (updateIfExists)
                                {
                                    ConcurrentDictionary<TKey, TValue>.Node node3 = new ConcurrentDictionary<TKey, TValue>.Node(node2.m_key, value, hashCode, node2.m_next);
                                    if (node == null)
                                    {
                                        buckets[num] = node3;
                                    }
                                    else
                                    {
                                        node.m_next = node3;
                                    }
                                    resultingValue = value;
                                }
                                else
                                {
                                    resultingValue = node2.m_value;
                                }
                                this.OnPropertyChanged("Item[]");
                                this.OnCollectionChanged();
                                return false;
                            }
                            node = node2;
                        }
                        buckets[num] = new ConcurrentDictionary<TKey, TValue>.Node(key, value, hashCode, buckets[num]);
                        this.m_countPerLock[num2]++;
                        if (this.m_countPerLock[num2] > buckets.Length / this.m_locks.Length)
                        {
                            flag = true;
                        }
                    }
                    finally
                    {
                        if (flag2)
                        {
                            Monitor.Exit(this.m_locks[num2]);
                        }
                    }
                    break;
                }
                if (flag)
                {
                    this.GrowTable(buckets);
                }
                this.OnPropertyChanged("Count");
                this.OnPropertyChanged("Item[]");
                this.OnCollectionChanged();
                resultingValue = value;
                return true;
            }
        }
        /// <summary>
        /// Adds a key/value pair to the <see cref="T:LFNet.Common.Collections.ConcurrentDictionary`2" /> 
        /// if the key does not already exist. 
        /// </summary>
        /// <param name="key">The key of the element to add.</param> 
        /// <param name="valueFactory">The function used to generate a value for the key</param>
        /// <exception cref="T:System.ArgumentNullException"><paramref name="key" /> is a null reference
        /// (Nothing in Visual Basic).</exception>
        /// <exception cref="T:System.ArgumentNullException"><paramref name="valueFactory" /> is a null reference 
        /// (Nothing in Visual Basic).</exception>
        /// <exception cref="T:System.OverflowException">The dictionary contains too many 
        /// elements.</exception> 
        /// <returns>The value for the key.  This will be either the existing value for the key if the
        /// key is already in the dictionary, or the new value for the key as returned by valueFactory 
        /// if the key was not in the dictionary.</returns>
        public TValue GetOrAdd(TKey key, Func<TKey, TValue> valueFactory)
        {
            if (key == null)
            {
                throw new ArgumentNullException("key");
            }
            if (valueFactory == null)
            {
                throw new ArgumentNullException("valueFactory");
            }
            TValue result;
            if (this.TryGetValue(key, out result))
            {
                return result;
            }
            this.TryAddInternal(key, valueFactory(key), false, true, out result);
            return result;
        }
        /// <summary> 
        /// Adds a key/value pair to the <see cref="T:LFNet.Common.Collections.ConcurrentDictionary`2" />
        /// if the key does not already exist. 
        /// </summary>
        /// <param name="key">The key of the element to add.</param>
        /// <param name="value">the value to be added, if the key does not already exist</param>
        /// <exception cref="T:System.ArgumentNullException"><paramref name="key" /> is a null reference 
        /// (Nothing in Visual Basic).</exception>
        /// <exception cref="T:System.OverflowException">The dictionary contains too many 
        /// elements.</exception> 
        /// <returns>The value for the key.  This will be either the existing value for the key if the
        /// key is already in the dictionary, or the new value if the key was not in the dictionary.</returns> 
        public TValue GetOrAdd(TKey key, TValue value)
        {
            if (key == null)
            {
                throw new ArgumentNullException("key");
            }
            TValue result;
            this.TryAddInternal(key, value, false, true, out result);
            return result;
        }
        /// <summary>
        /// Adds a key/value pair to the <see cref="T:LFNet.Common.Collections.ConcurrentDictionary`2" /> if the key does not already
        /// exist, or updates a key/value pair in the <see cref="T:LFNet.Common.Collections.ConcurrentDictionary`2" /> if the key
        /// already exists. 
        /// </summary>
        /// <param name="key">The key to be added or whose value should be updated</param> 
        /// <param name="addValueFactory">The function used to generate a value for an absent key</param> 
        /// <param name="updateValueFactory">The function used to generate a new value for an existing key
        /// based on the key's existing value</param> 
        /// <exception cref="T:System.ArgumentNullException"><paramref name="key" /> is a null reference
        /// (Nothing in Visual Basic).</exception>
        /// <exception cref="T:System.ArgumentNullException"><paramref name="addValueFactory" /> is a null reference
        /// (Nothing in Visual Basic).</exception> 
        /// <exception cref="T:System.ArgumentNullException"><paramref name="updateValueFactory" /> is a null reference
        /// (Nothing in Visual Basic).</exception> 
        /// <exception cref="T:System.OverflowException">The dictionary contains too many 
        /// elements.</exception>
        /// <returns>The new value for the key.  This will be either be the result of addValueFactory (if the key was 
        /// absent) or the result of updateValueFactory (if the key was present).</returns>
        public TValue AddOrUpdate(TKey key, Func<TKey, TValue> addValueFactory, Func<TKey, TValue, TValue> updateValueFactory)
        {
            if (key == null)
            {
                throw new ArgumentNullException("key");
            }
            if (addValueFactory == null)
            {
                throw new ArgumentNullException("addValueFactory");
            }
            if (updateValueFactory == null)
            {
                throw new ArgumentNullException("updateValueFactory");
            }
            TValue tValue2;
            while (true)
            {
                TValue tValue;
                if (this.TryGetValue(key, out tValue))
                {
                    tValue2 = updateValueFactory(key, tValue);
                    if (this.TryUpdate(key, tValue2, tValue))
                    {
                        break;
                    }
                }
                else
                {
                    tValue2 = addValueFactory(key);
                    TValue result;
                    if (this.TryAddInternal(key, tValue2, false, true, out result))
                    {
                        return result;
                    }
                }
            }
            return tValue2;
        }
        /// <summary>
        /// Adds a key/value pair to the <see cref="T:LFNet.Common.Collections.ConcurrentDictionary`2" /> if the key does not already 
        /// exist, or updates a key/value pair in the <see cref="T:LFNet.Common.Collections.ConcurrentDictionary`2" /> if the key 
        /// already exists.
        /// </summary> 
        /// <param name="key">The key to be added or whose value should be updated</param>
        /// <param name="addValue">The value to be added for an absent key</param>
        /// <param name="updateValueFactory">The function used to generate a new value for an existing key based on
        /// the key's existing value</param> 
        /// <exception cref="T:System.ArgumentNullException"><paramref name="key" /> is a null reference
        /// (Nothing in Visual Basic).</exception> 
        /// <exception cref="T:System.ArgumentNullException"><paramref name="updateValueFactory" /> is a null reference 
        /// (Nothing in Visual Basic).</exception>
        /// <exception cref="T:System.OverflowException">The dictionary contains too many 
        /// elements.</exception>
        /// <returns>The new value for the key.  This will be either be the result of addValueFactory (if the key was
        /// absent) or the result of updateValueFactory (if the key was present).</returns>
        public TValue AddOrUpdate(TKey key, TValue addValue, Func<TKey, TValue, TValue> updateValueFactory)
        {
            if (key == null)
            {
                throw new ArgumentNullException("key");
            }
            if (updateValueFactory == null)
            {
                throw new ArgumentNullException("updateValueFactory");
            }
            TValue tValue2;
            while (true)
            {
                TValue tValue;
                if (this.TryGetValue(key, out tValue))
                {
                    tValue2 = updateValueFactory(key, tValue);
                    if (this.TryUpdate(key, tValue2, tValue))
                    {
                        break;
                    }
                }
                else
                {
                    TValue result;
                    if (this.TryAddInternal(key, addValue, false, true, out result))
                    {
                        return result;
                    }
                }
            }
            return tValue2;
        }
        void IDictionary<TKey, TValue>.Add(TKey key, TValue value)
        {
            if (!this.TryAdd(key, value))
            {
                throw new ArgumentException(this.GetResource("ConcurrentDictionary_KeyAlreadyExisted"));
            }
        }
        bool IDictionary<TKey, TValue>.Remove(TKey key)
        {
            TValue tValue;
            return this.TryRemove(key, out tValue);
        }
        void ICollection<KeyValuePair<TKey, TValue>>.Add(KeyValuePair<TKey, TValue> keyValuePair)
        {
            ((IDictionary<TKey, TValue>)this).Add(keyValuePair.Key, keyValuePair.Value);
        }
        bool ICollection<KeyValuePair<TKey, TValue>>.Contains(KeyValuePair<TKey, TValue> keyValuePair)
        {
            TValue x;
            return this.TryGetValue(keyValuePair.Key, out x) && EqualityComparer<TValue>.Default.Equals(x, keyValuePair.Value);
        }
        bool ICollection<KeyValuePair<TKey, TValue>>.Remove(KeyValuePair<TKey, TValue> keyValuePair)
        {
            if (keyValuePair.Key == null)
            {
                throw new ArgumentNullException(this.GetResource("ConcurrentDictionary_ItemKeyIsNull"));
            }
            TValue tValue;
            return this.TryRemoveInternal(keyValuePair.Key, out tValue, true, keyValuePair.Value);
        }
        /// <summary>Returns an enumerator that iterates through the <see cref="T:LFNet.Common.Collections.ConcurrentDictionary`2" />.</summary> 
        /// <returns>An enumerator for the <see cref="T:LFNet.Common.Collections.ConcurrentDictionary`2" />.</returns>
        /// <remarks>
        /// The enumerator returned from the dictionary is safe to use concurrently with
        /// reads and writes to the dictionary, however it does not represent a moment-in-time snapshot 
        /// of the dictionary.  The contents exposed through the enumerator may contain modifications
        /// made to the dictionary after <see cref="M:LFNet.Common.Collections.ConcurrentDictionary`2.GetEnumerator" /> was called. 
        /// </remarks> 
        IEnumerator IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
        }
        /// <summary>
        /// Adds the specified key and value to the dictionary. 
        /// </summary>
        /// <param name="key">The object to use as the key.</param>
        /// <param name="value">The object to use as the value.</param>
        /// <exception cref="T:System.ArgumentNullException"><paramref name="key" /> is a null reference 
        /// (Nothing in Visual Basic).</exception>
        /// <exception cref="T:System.OverflowException">The dictionary contains too many 
        /// elements.</exception> 
        /// <exception cref="T:System.ArgumentException">
        /// <paramref name="key" /> is of a type that is not assignable to the key type <typeparamref name="TKey" /> of the <see cref="T:System.Collections.Generic.Dictionary{TKey,TValue}" />. -or-
        /// <paramref name="value" /> is of a type that is not assignable to <typeparamref name="TValue" />,
        /// the type of values in the <see cref="T:System.Collections.Generic.Dictionary{TKey,TValue}" />.
        /// -or- A value with the same key already exists in the <see cref="T:System.Collections.Generic.Dictionary{TKey,TValue}" />.
        /// </exception> 
        void IDictionary.Add(object key, object value)
        {
            if (key == null)
            {
                throw new ArgumentNullException("key");
            }
            if (!(key is TKey))
            {
                throw new ArgumentException(this.GetResource("ConcurrentDictionary_TypeOfKeyIncorrect"));
            }
            TValue value2;
            try
            {
                value2 = (TValue)((object)value);
            }
            catch (InvalidCastException)
            {
                throw new ArgumentException(this.GetResource("ConcurrentDictionary_TypeOfValueIncorrect"));
            }
            ((IDictionary<TKey, TValue>)this).Add((TKey)((object)key), value2);
        }
        /// <summary> 
        /// Gets whether the <see cref="T:System.Collections.Generic.IDictionary{TKey,TValue}" /> contains an
        /// element with the specified key. 
        /// </summary>
        /// <param name="key">The key to locate in the <see cref="T:System.Collections.Generic.IDictionary{TKey,TValue}" />.</param>
        /// <returns>true if the <see cref="T:System.Collections.Generic.IDictionary{TKey,TValue}" /> contains 
        /// an element with the specified key; otherwise, false.</returns>
        /// <exception cref="T:System.ArgumentNullException"> <paramref name="key" /> is a null reference 
        /// (Nothing in Visual Basic).</exception> 
        bool IDictionary.Contains(object key)
        {
            if (key == null)
            {
                throw new ArgumentNullException("key");
            }
            return key is TKey && this.ContainsKey((TKey)((object)key));
        }
        /// <summary>Provides an <see cref="T:System.Collections.Generics.IDictionaryEnumerator" /> for the 
        /// <see cref="T:System.Collections.Generic.IDictionary{TKey,TValue}" />.</summary> 
        /// <returns>An <see cref="T:System.Collections.Generics.IDictionaryEnumerator" /> for the <see cref="T:System.Collections.Generic.IDictionary{TKey,TValue}" />.</returns> 
        IDictionaryEnumerator IDictionary.GetEnumerator()
        {
            return new ConcurrentDictionary<TKey, TValue>.DictionaryEnumerator(this);
        }
        /// <summary> 
        /// Removes the element with the specified key from the <see cref="T:System.Collections.IDictionary" />. 
        /// </summary>
        /// <param name="key">The key of the element to remove.</param>
        /// <exception cref="T:System.ArgumentNullException"><paramref name="key" /> is a null reference
        /// (Nothing in Visual Basic).</exception> 
        void IDictionary.Remove(object key)
        {
            if (key == null)
            {
                throw new ArgumentNullException("key");
            }
            if (key is TKey)
            {
                TValue tValue;
                this.TryRemove((TKey)((object)key), out tValue);
            }
        }
        /// <summary>
        /// Copies the elements of the <see cref="T:System.Collections.ICollection" /> to an array, starting
        /// at the specified array index. 
        /// </summary>
        /// <param name="array">The one-dimensional array that is the destination of the elements copied from 
        /// the <see cref="T:System.Collections.ICollection" />. The array must have zero-based 
        /// indexing.</param>
        /// <param name="index">The zero-based index in <paramref name="array" /> at which copying 
        /// begins.</param>
        /// <exception cref="T:System.ArgumentNullException"><paramref name="array" /> is a null reference
        /// (Nothing in Visual Basic).</exception>
        /// <exception cref="T:System.ArgumentOutOfRangeException"><paramref name="index" /> is less than 
        /// 0.</exception>
        /// <exception cref="T:System.ArgumentException"><paramref name="index" /> is equal to or greater than 
        /// the length of the <paramref name="array" />. -or- The number of elements in the source <see cref="T:System.Collections.ICollection" />
        /// is greater than the available space from <paramref name="index" /> to the end of the destination 
        /// <paramref name="array" />.</exception>
        void ICollection.CopyTo(Array array, int index)
        {
            if (array == null)
            {
                throw new ArgumentNullException("array");
            }
            if (index < 0)
            {
                throw new ArgumentOutOfRangeException("index", this.GetResource("ConcurrentDictionary_IndexIsNegative"));
            }
            int toExclusive = 0;
            try
            {
                this.AcquireAllLocks(ref toExclusive);
                int num = 0;
                for (int i = 0; i < this.m_locks.Length; i++)
                {
                    num += this.m_countPerLock[i];
                }
                if (array.Length - num < index || num < 0)
                {
                    throw new ArgumentException(this.GetResource("ConcurrentDictionary_ArrayNotLargeEnough"));
                }
                KeyValuePair<TKey, TValue>[] array2 = array as KeyValuePair<TKey, TValue>[];
                if (array2 != null)
                {
                    this.CopyToPairs(array2, index);
                }
                else
                {
                    DictionaryEntry[] array3 = array as DictionaryEntry[];
                    if (array3 != null)
                    {
                        this.CopyToEntries(array3, index);
                    }
                    else
                    {
                        object[] array4 = array as object[];
                        if (array4 == null)
                        {
                            throw new ArgumentException(this.GetResource("ConcurrentDictionary_ArrayIncorrectType"), "array");
                        }
                        this.CopyToObjects(array4, index);
                    }
                }
            }
            finally
            {
                this.ReleaseLocks(0, toExclusive);
            }
        }
        /// <summary> 
        /// Replaces the internal table with a larger one. To prevent multiple threads from resizing the 
        /// table as a result of ----s, the table of buckets that was deemed too small is passed in as
        /// an argument to GrowTable(). GrowTable() obtains a lock, and then checks whether the bucket 
        /// table has been replaced in the meantime or not.
        /// </summary>
        /// <param name="buckets">Reference to the bucket table that was deemed too small.</param>
        private void GrowTable(ConcurrentDictionary<TKey, TValue>.Node[] buckets)
        {
            int toExclusive = 0;
            try
            {
                this.AcquireLocks(0, 1, ref toExclusive);
                if (buckets == this.m_buckets)
                {
                    ConcurrentDictionary<TKey, TValue>.Node[] array;
                    int[] array2;
                    checked
                    {
                        int num;
                        try
                        {
                            num = buckets.Length * 2 + 1;
                            while (num % 3 == 0 || num % 5 == 0 || num % 7 == 0)
                            {
                                num += 2;
                            }
                        }
                        catch (OverflowException)
                        {
                            return;
                        }
                        array = new ConcurrentDictionary<TKey, TValue>.Node[num];
                        array2 = new int[this.m_locks.Length];
                        this.AcquireLocks(1, this.m_locks.Length, ref toExclusive);
                    }
                    for (int i = 0; i < buckets.Length; i++)
                    {
                        checked
                        {
                            ConcurrentDictionary<TKey, TValue>.Node next;
                            for (ConcurrentDictionary<TKey, TValue>.Node node = buckets[i]; node != null; node = next)
                            {
                                next = node.m_next;
                                int num2;
                                int num3;
                                this.GetBucketAndLockNo(node.m_hashcode, out num2, out num3, array.Length);
                                array[num2] = new ConcurrentDictionary<TKey, TValue>.Node(node.m_key, node.m_value, node.m_hashcode, array[num2]);
                                array2[num3]++;
                            }
                        }
                    }
                    this.m_buckets = array;
                    this.m_countPerLock = array2;
                }
            }
            finally
            {
                this.ReleaseLocks(0, toExclusive);
            }
        }
        /// <summary>
        /// Computes the bucket and lock number for a particular key. 
        /// </summary> 
        private void GetBucketAndLockNo(int hashcode, out int bucketNo, out int lockNo, int bucketCount)
        {
            bucketNo = (hashcode & 2147483647) % bucketCount;
            lockNo = bucketNo % this.m_locks.Length;
        }
        /// <summary> 
        /// Acquires all locks for this hash table, and increments locksAcquired by the number
        /// of locks that were successfully acquired. The locks are acquired in an increasing
        /// order.
        /// </summary> 
        private void AcquireAllLocks(ref int locksAcquired)
        {
            this.AcquireLocks(0, this.m_locks.Length, ref locksAcquired);
        }
        /// <summary> 
        /// Acquires a contiguous range of locks for this hash table, and increments locksAcquired
        /// by the number of locks that were successfully acquired. The locks are acquired in an
        /// increasing order.
        /// </summary> 
        private void AcquireLocks(int fromInclusive, int toExclusive, ref int locksAcquired)
        {
            for (int i = fromInclusive; i < toExclusive; i++)
            {
                bool flag = false;
                try
                {
                    Monitor.Enter(this.m_locks[i]);
                    flag = true;
                }
                finally
                {
                    if (flag)
                    {
                        locksAcquired++;
                    }
                }
            }
        }
        /// <summary> 
        /// Releases a contiguous range of locks.
        /// </summary> 
        private void ReleaseLocks(int fromInclusive, int toExclusive)
        {
            for (int i = fromInclusive; i < toExclusive; i++)
            {
                Monitor.Exit(this.m_locks[i]);
            }
        }
        /// <summary>
        /// Gets a collection containing the keys in the dictionary. 
        /// </summary>
        private global::System.Collections.ObjectModel.ReadOnlyCollection<TKey> GetKeys()
        {
            int toExclusive = 0;
            global::System.Collections.ObjectModel.ReadOnlyCollection<TKey> result;
            try
            {
                this.AcquireAllLocks(ref toExclusive);
                List<TKey> list = new List<TKey>();
                for (int i = 0; i < this.m_buckets.Length; i++)
                {
                    for (ConcurrentDictionary<TKey, TValue>.Node node = this.m_buckets[i]; node != null; node = node.m_next)
                    {
                        list.Add(node.m_key);
                    }
                }
                result = new global::System.Collections.ObjectModel.ReadOnlyCollection<TKey>(list);
            }
            finally
            {
                this.ReleaseLocks(0, toExclusive);
            }
            return result;
        }
        /// <summary>
        /// Gets a collection containing the values in the dictionary.
        /// </summary>
        private global::System.Collections.ObjectModel.ReadOnlyCollection<TValue> GetValues()
        {
            int toExclusive = 0;
            global::System.Collections.ObjectModel.ReadOnlyCollection<TValue> result;
            try
            {
                this.AcquireAllLocks(ref toExclusive);
                List<TValue> list = new List<TValue>();
                for (int i = 0; i < this.m_buckets.Length; i++)
                {
                    for (ConcurrentDictionary<TKey, TValue>.Node node = this.m_buckets[i]; node != null; node = node.m_next)
                    {
                        list.Add(node.m_value);
                    }
                }
                result = new global::System.Collections.ObjectModel.ReadOnlyCollection<TValue>(list);
            }
            finally
            {
                this.ReleaseLocks(0, toExclusive);
            }
            return result;
        }
        /// <summary>
        /// A helper method for asserts. 
        /// </summary>
        [Conditional("DEBUG")]
        private void Assert(bool condition)
        {
            if (!condition)
            {
                throw new Exception("Assertion failed.");
            }
        }
        /// <summary>
        /// A helper function to obtain the string for a particular resource key.
        /// </summary>
        /// <param name="key"></param> 
        /// <returns></returns>
        private string GetResource(string key)
        {
            return key;
        }
        /// <summary> 
        /// Get the data array to be serialized
        /// </summary> 
        [OnSerializing]
        private void OnSerializing(StreamingContext context)
        {
            this.m_serializationArray = this.ToArray();
            this.m_serializationConcurrencyLevel = this.m_locks.Length;
            this.m_serializationCapacity = this.m_buckets.Length;
        }
        /// <summary> 
        /// Construct the dictionary from a previously seiralized one
        /// </summary> 
        [OnDeserialized]
        private void OnDeserialized(StreamingContext context)
        {
            KeyValuePair<TKey, TValue>[] serializationArray = this.m_serializationArray;
            this.m_buckets = new ConcurrentDictionary<TKey, TValue>.Node[this.m_serializationCapacity];
            this.m_countPerLock = new int[this.m_serializationConcurrencyLevel];
            this.m_locks = new object[this.m_serializationConcurrencyLevel];
            for (int i = 0; i < this.m_locks.Length; i++)
            {
                this.m_locks[i] = new object();
            }
            this.InitializeFromCollection(serializationArray);
            this.m_serializationArray = null;
        }
    }
}
