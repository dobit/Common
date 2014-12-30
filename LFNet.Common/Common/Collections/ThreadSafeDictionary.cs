using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.Serialization;
using System.Security.Permissions;
using System.Threading;
using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;

namespace LFNet.Common.Collections
{
	[Obsolete("Use ConcurrentDictionary instead."), XmlRoot("dictionary")]
	public class ThreadSafeDictionary<TKey, TValue> : IDictionary<TKey, TValue>, ICollection<KeyValuePair<TKey, TValue>>, IEnumerable<KeyValuePair<TKey, TValue>>, IDictionary, ICollection, IEnumerable, ISerializable, IDeserializationCallback, IXmlSerializable, INotifyPropertyChanged
	{
		private class DisposableAction : IDisposable
		{
			private readonly Action _exitAction;
			public DisposableAction(Action exitAction)
			{
				this._exitAction = exitAction;
			}
			void IDisposable.Dispose()
			{
				this._exitAction();
			}
		}
		private const string COUNT_STRING = "Count";
		private const string INDEXER_NAME = "Item[]";
		protected readonly Dictionary<TKey, TValue> innerDictionary;
		private readonly ReaderWriterLockSlim _lock;
		private object _syncRoot;
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
		/// <summary>Gets the <see cref="T:System.Collections.Generic.IEqualityComparer`1"></see> that is used to determine equality of keys for the dictionary. </summary>
		/// <returns>The <see cref="T:System.Collections.Generic.IEqualityComparer`1"></see> generic interface implementation that is used to determine equality of keys for the current <see cref="T:LFNet.Common.Collections.ThreadSafeDictionary`2"></see> and to provide hash values for the keys.</returns>
		public IEqualityComparer<TKey> Comparer
		{
			get
			{
				return this.innerDictionary.Comparer;
			}
		}
		/// <summary>Gets a collection containing the keys in the <see cref="T:LFNet.Common.Collections.ThreadSafeDictionary`2"></see>.</summary>
		/// <returns>A <see cref="T:System.Collections.Generic.Dictionary`2.KeyCollection"></see> containing the keys in the <see cref="T:LFNet.Common.Collections.ThreadSafeDictionary`2"></see>.</returns>
		public Dictionary<TKey, TValue>.KeyCollection Keys
		{
			get
			{
				Dictionary<TKey, TValue>.KeyCollection keys;
				using (this.EnterReadLock())
				{
					keys = this.innerDictionary.Keys;
				}
				return keys;
			}
		}
		/// <summary>Gets a collection containing the values in the <see cref="T:LFNet.Common.Collections.ThreadSafeDictionary`2"></see>.</summary>
		/// <returns>A <see cref="T:System.Collections.Generic.Dictionary`2.ValueCollection"></see> containing the values in the <see cref="T:LFNet.Common.Collections.ThreadSafeDictionary`2"></see>.</returns>
		public Dictionary<TKey, TValue>.ValueCollection Values
		{
			get
			{
				Dictionary<TKey, TValue>.ValueCollection values;
				using (this.EnterReadLock())
				{
					values = this.innerDictionary.Values;
				}
				return values;
			}
		}
		bool ICollection.IsSynchronized
		{
			get
			{
				return false;
			}
		}
		object ICollection.SyncRoot
		{
			get
			{
				if (this._syncRoot == null)
				{
					Interlocked.CompareExchange(ref this._syncRoot, new object(), null);
				}
				return this._syncRoot;
			}
		}
		bool IDictionary.IsFixedSize
		{
			get
			{
				return false;
			}
		}
		bool IDictionary.IsReadOnly
		{
			get
			{
				return false;
			}
		}
		object IDictionary.this[object key]
		{
			get
			{
				object result;
				using (this.EnterReadLock())
				{
					result = ((IDictionary)this.innerDictionary)[key];
				}
				return result;
			}
			set
			{
				using (this.EnterWriteLock())
				{
					((IDictionary)this.innerDictionary)[key] = value;
				}
			}
		}
		ICollection IDictionary.Keys
		{
			get
			{
				ICollection keys;
				using (this.EnterReadLock())
				{
					keys = ((IDictionary)this.innerDictionary).Keys;
				}
				return keys;
			}
		}
		ICollection IDictionary.Values
		{
			get
			{
				ICollection values;
				using (this.EnterReadLock())
				{
					values = ((IDictionary)this.innerDictionary).Values;
				}
				return values;
			}
		}
		/// <summary>Gets the number of key/value pairs contained in the <see cref="T:LFNet.Common.Collections.ThreadSafeDictionary`2"></see>.</summary>
		/// <returns>The number of key/value pairs contained in the <see cref="T:LFNet.Common.Collections.ThreadSafeDictionary`2"></see>.</returns>
		public int Count
		{
			get
			{
				int count;
				using (this.EnterReadLock())
				{
					count = this.innerDictionary.Count;
				}
				return count;
			}
		}
		/// <summary>Gets or sets the value associated with the specified key.</summary>
		/// <returns>The value associated with the specified key. If the specified key is not found, a get operation throws a <see cref="T:System.Collections.Generic.KeyNotFoundException"></see>, and a set operation creates a new element with the specified key.</returns>
		/// <param name="key">The key of the value to get or set.</param>
		/// <exception cref="T:System.ArgumentNullException">key is null.</exception>
		/// <exception cref="T:System.Collections.Generic.KeyNotFoundException">The property is retrieved and key does not exist in the collection.</exception>
		public TValue this[TKey key]
		{
			get
			{
				TValue result;
				using (this.EnterReadLock())
				{
					result = this.innerDictionary[key];
				}
				return result;
			}
			set
			{
				using (this.EnterWriteLock())
				{
					this.innerDictionary[key] = value;
				}
				this.OnPropertyChanged("Item[]");
				this.OnCollectionChanged();
			}
		}
		bool ICollection<KeyValuePair<TKey, TValue>>.IsReadOnly
		{
			get
			{
				return false;
			}
		}
		ICollection<TKey> IDictionary<TKey, TValue>.Keys
		{
			get
			{
				ICollection<TKey> keys;
				using (this.EnterReadLock())
				{
					keys = ((IDictionary<TKey, TValue>)this.innerDictionary).Keys;
				}
				return keys;
			}
		}
		ICollection<TValue> IDictionary<TKey, TValue>.Values
		{
			get
			{
				ICollection<TValue> values;
				using (this.EnterReadLock())
				{
					values = ((IDictionary<TKey, TValue>)this.innerDictionary).Values;
				}
				return values;
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
		/// Initializes a new instance of the <see cref="T:LFNet.Common.Collections.ThreadSafeDictionary`2" /> class.
		/// </summary>
		public ThreadSafeDictionary()
		{
			this.innerDictionary = new Dictionary<TKey, TValue>();
			this._lock = new ReaderWriterLockSlim();
		}
		/// <summary>Initializes a new instance of the <see cref="T:LFNet.Common.Collections.ThreadSafeDictionary`2"></see> class that contains elements copied from the specified <see cref="T:System.Collections.Generic.IDictionary`2"></see> and uses the default equality comparer for the key type.</summary>
		/// <param name="dictionary">The <see cref="T:System.Collections.Generic.IDictionary`2"></see> whose elements are copied to the new <see cref="T:LFNet.Common.Collections.ThreadSafeDictionary`2"></see>.</param>
		/// <exception cref="T:System.ArgumentException">dictionary contains one or more duplicate keys.</exception>
		/// <exception cref="T:System.ArgumentNullException">dictionary is null.</exception>
		public ThreadSafeDictionary(IDictionary<TKey, TValue> dictionary)
		{
			this.innerDictionary = new Dictionary<TKey, TValue>(dictionary);
			this._lock = new ReaderWriterLockSlim();
		}
		/// <summary>Initializes a new instance of the <see cref="T:LFNet.Common.Collections.ThreadSafeDictionary`2"></see> class that is empty, has the default initial capacity, and uses the specified <see cref="T:System.Collections.Generic.IEqualityComparer`1"></see>.</summary>
		/// <param name="comparer">The <see cref="T:System.Collections.Generic.IEqualityComparer`1"></see> implementation to use when comparing keys, or null to use the default <see cref="T:System.Collections.Generic.EqualityComparer`1"></see> for the type of the key.</param>
		public ThreadSafeDictionary(IEqualityComparer<TKey> comparer)
		{
			this.innerDictionary = new Dictionary<TKey, TValue>(comparer);
			this._lock = new ReaderWriterLockSlim();
		}
		/// <summary>Initializes a new instance of the <see cref="T:LFNet.Common.Collections.ThreadSafeDictionary`2"></see> class that is empty, has the specified initial capacity, and uses the default equality comparer for the key type.</summary>
		/// <param name="capacity">The initial number of elements that the <see cref="T:LFNet.Common.Collections.ThreadSafeDictionary`2"></see> can contain.</param>
		/// <exception cref="T:System.ArgumentOutOfRangeException">capacity is less than 0.</exception>
		public ThreadSafeDictionary(int capacity)
		{
			this.innerDictionary = new Dictionary<TKey, TValue>(capacity);
			this._lock = new ReaderWriterLockSlim();
		}
		/// <summary>Initializes a new instance of the <see cref="T:LFNet.Common.Collections.ThreadSafeDictionary`2"></see> class that contains elements copied from the specified <see cref="T:System.Collections.Generic.IDictionary`2"></see> and uses the specified <see cref="T:System.Collections.Generic.IEqualityComparer`1"></see>.</summary>
		/// <param name="dictionary">The <see cref="T:System.Collections.Generic.IDictionary`2"></see> whose elements are copied to the new <see cref="T:LFNet.Common.Collections.ThreadSafeDictionary`2"></see>.</param>
		/// <param name="comparer">The <see cref="T:System.Collections.Generic.IEqualityComparer`1"></see> implementation to use when comparing keys, or null to use the default <see cref="T:System.Collections.Generic.EqualityComparer`1"></see> for the type of the key.</param>
		/// <exception cref="T:System.ArgumentException">dictionary contains one or more duplicate keys.</exception>
		/// <exception cref="T:System.ArgumentNullException">dictionary is null.</exception>
		public ThreadSafeDictionary(IDictionary<TKey, TValue> dictionary, IEqualityComparer<TKey> comparer)
		{
			this.innerDictionary = new Dictionary<TKey, TValue>(dictionary, comparer);
			this._lock = new ReaderWriterLockSlim();
		}
		/// <summary>Initializes a new instance of the <see cref="T:LFNet.Common.Collections.ThreadSafeDictionary`2"></see> class that is empty, has the specified initial capacity, and uses the specified <see cref="T:System.Collections.Generic.IEqualityComparer`1"></see>.</summary>
		/// <param name="capacity">The initial number of elements that the <see cref="T:LFNet.Common.Collections.ThreadSafeDictionary`2"></see> can contain.</param>
		/// <param name="comparer">The <see cref="T:System.Collections.Generic.IEqualityComparer`1"></see> implementation to use when comparing keys, or null to use the default <see cref="T:System.Collections.Generic.EqualityComparer`1"></see> for the type of the key.</param>
		/// <exception cref="T:System.ArgumentOutOfRangeException">capacity is less than 0.</exception>
		public ThreadSafeDictionary(int capacity, IEqualityComparer<TKey> comparer)
		{
			this.innerDictionary = new Dictionary<TKey, TValue>(capacity, comparer);
			this._lock = new ReaderWriterLockSlim();
		}
		/// <summary>Implements the <see cref="T:System.Runtime.Serialization.ISerializable"></see> interface and raises the deserialization event when the deserialization is complete.</summary>
		/// <param name="sender">The source of the deserialization event.</param>
		/// <exception cref="T:System.Runtime.Serialization.SerializationException">The <see cref="T:System.Runtime.Serialization.SerializationInfo"></see> object associated with the current <see cref="T:LFNet.Common.Collections.ThreadSafeDictionary`2"></see> instance is invalid.</exception>
		public virtual void OnDeserialization(object sender)
		{
			this.innerDictionary.OnDeserialization(sender);
		}
		void ICollection.CopyTo(Array array, int index)
		{
			using (this.EnterReadLock())
			{
				((ICollection)this.innerDictionary).CopyTo(array, index);
			}
		}
		void IDictionary.Add(object key, object value)
		{
			using (this.EnterWriteLock())
			{
				((IDictionary)this.innerDictionary).Add(key, value);
			}
		}
		bool IDictionary.Contains(object key)
		{
			bool result;
			using (this.EnterReadLock())
			{
				result = ((IDictionary)this.innerDictionary).Contains(key);
			}
			return result;
		}
		IDictionaryEnumerator IDictionary.GetEnumerator()
		{
			return ((IDictionary)this.innerDictionary).GetEnumerator();
		}
		void IDictionary.Remove(object key)
		{
			using (this.EnterWriteLock())
			{
				((IDictionary)this.innerDictionary).Remove(key);
			}
		}
		/// <summary>Adds the specified key and value to the dictionary.</summary>
		/// <param name="value">The value of the element to add. The value can be null for reference types.</param>
		/// <param name="key">The key of the element to add.</param>
		/// <exception cref="T:System.ArgumentException">An element with the same key already exists in the <see cref="T:LFNet.Common.Collections.ThreadSafeDictionary`2"></see>.</exception>
		/// <exception cref="T:System.ArgumentNullException">key is null.</exception>
		public void Add(TKey key, TValue value)
		{
			using (this.EnterWriteLock())
			{
				this.innerDictionary.Add(key, value);
			}
			this.OnPropertyChanged("Count");
			this.OnPropertyChanged("Item[]");
			this.OnCollectionChanged();
		}
		/// <summary>Removes all keys and values from the <see cref="T:LFNet.Common.Collections.ThreadSafeDictionary`2"></see>.</summary>
		public void Clear()
		{
			using (this.EnterWriteLock())
			{
				this.innerDictionary.Clear();
			}
			this.OnPropertyChanged("Count");
			this.OnPropertyChanged("Item[]");
			this.OnCollectionChanged();
		}
		/// <summary>Determines whether the <see cref="T:LFNet.Common.Collections.ThreadSafeDictionary`2"></see> contains the specified key.</summary>
		/// <returns>true if the <see cref="T:LFNet.Common.Collections.ThreadSafeDictionary`2"></see> contains an element with the specified key; otherwise, false.</returns>
		/// <param name="key">The key to locate in the <see cref="T:LFNet.Common.Collections.ThreadSafeDictionary`2"></see>.</param>
		/// <exception cref="T:System.ArgumentNullException">key is null.</exception>
		public bool ContainsKey(TKey key)
		{
			bool result;
			using (this.EnterReadLock())
			{
				result = this.innerDictionary.ContainsKey(key);
			}
			return result;
		}
		/// <summary>Removes the value with the specified key from the <see cref="T:LFNet.Common.Collections.ThreadSafeDictionary`2"></see>.</summary>
		/// <returns>true if the element is successfully found and removed; otherwise, false.  This method returns false if key is not found in the <see cref="T:LFNet.Common.Collections.ThreadSafeDictionary`2"></see>.</returns>
		/// <param name="key">The key of the element to remove.</param>
		/// <exception cref="T:System.ArgumentNullException">key is null.</exception>
		public bool Remove(TKey key)
		{
			bool flag;
			using (this.EnterWriteLock())
			{
				flag = this.innerDictionary.Remove(key);
			}
			if (flag)
			{
				this.OnPropertyChanged("Count");
				this.OnPropertyChanged("Item[]");
				this.OnCollectionChanged();
			}
			return flag;
		}
		void ICollection<KeyValuePair<TKey, TValue>>.Add(KeyValuePair<TKey, TValue> keyValuePair)
		{
			this.Add(keyValuePair.Key, keyValuePair.Value);
		}
		bool ICollection<KeyValuePair<TKey, TValue>>.Contains(KeyValuePair<TKey, TValue> keyValuePair)
		{
			bool result;
			using (this.EnterReadLock())
			{
				result = ((ICollection<KeyValuePair<TKey, TValue>>)this.innerDictionary).Contains(keyValuePair);
			}
			return result;
		}
		void ICollection<KeyValuePair<TKey, TValue>>.CopyTo(KeyValuePair<TKey, TValue>[] array, int index)
		{
			using (this.EnterReadLock())
			{
				((ICollection<KeyValuePair<TKey, TValue>>)this.innerDictionary).CopyTo(array, index);
			}
		}
		bool ICollection<KeyValuePair<TKey, TValue>>.Remove(KeyValuePair<TKey, TValue> keyValuePair)
		{
			return this.Remove(keyValuePair.Key);
		}
		IEnumerator<KeyValuePair<TKey, TValue>> IEnumerable<KeyValuePair<TKey, TValue>>.GetEnumerator()
		{
			return ((IEnumerable<KeyValuePair<TKey, TValue>>)this.innerDictionary).GetEnumerator();
		}
		IEnumerator IEnumerable.GetEnumerator()
		{
			return ((IEnumerable)this.innerDictionary).GetEnumerator();
		}
		/// <summary>
		/// Gets the value associated with the specified key.
		/// </summary>
		/// <param name="key">The key whose value to get.</param>
		/// <param name="value">When this method returns, the value associated with the specified key, if the key is found; otherwise, the default value for the type of the <paramref name="value" /> parameter. This parameter is passed uninitialized.</param>
		/// <returns>
		/// true if the object that implements <see cref="T:System.Collections.Generic.IDictionary`2" /> contains an element with the specified key; otherwise, false.
		/// </returns>
		/// <exception cref="T:System.ArgumentNullException"><paramref name="key" /> is null.</exception>
		public bool TryGetValue(TKey key, out TValue value)
		{
			bool result;
			using (this.EnterReadLock())
			{
				result = this.innerDictionary.TryGetValue(key, out value);
			}
			return result;
		}
		/// <summary>Implements the <see cref="T:System.Runtime.Serialization.ISerializable"></see> interface and returns the data needed to serialize the <see cref="T:LFNet.Common.Collections.ThreadSafeDictionary`2"></see> instance.</summary>
		/// <param name="context">A <see cref="T:System.Runtime.Serialization.StreamingContext"></see> structure that contains the source and destination of the serialized stream associated with the <see cref="T:LFNet.Common.Collections.ThreadSafeDictionary`2"></see> instance.</param>
		/// <param name="info">A <see cref="T:System.Runtime.Serialization.SerializationInfo"></see> object that contains the information required to serialize the <see cref="T:LFNet.Common.Collections.ThreadSafeDictionary`2"></see> instance.</param>
		/// <exception cref="T:System.ArgumentNullException">info is null.</exception>
		[SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.SerializationFormatter)]
		public virtual void GetObjectData(SerializationInfo info, StreamingContext context)
		{
			this.innerDictionary.GetObjectData(info, context);
		}
		/// <summary>Determines whether the <see cref="T:LFNet.Common.Collections.ThreadSafeDictionary`2"></see> contains a specific value.</summary>
		/// <returns>true if the <see cref="T:LFNet.Common.Collections.ThreadSafeDictionary`2"></see> contains an element with the specified value; otherwise, false.</returns>
		/// <param name="value">The value to locate in the <see cref="T:LFNet.Common.Collections.ThreadSafeDictionary`2"></see>. The value can be null for reference types.</param>
		public bool ContainsValue(TValue value)
		{
			bool result;
			using (this.EnterReadLock())
			{
				result = this.innerDictionary.ContainsValue(value);
			}
			return result;
		}
		/// <summary>Returns an enumerator that iterates through the <see cref="T:LFNet.Common.Collections.ThreadSafeDictionary`2"></see>.</summary>
		/// <returns>A <see cref="T:System.Collections.Generic.Dictionary`2.Enumerator"></see> structure for the <see cref="T:LFNet.Common.Collections.ThreadSafeDictionary`2"></see>.</returns>
		public Dictionary<TKey, TValue>.Enumerator GetEnumerator()
		{
			return this.innerDictionary.GetEnumerator();
		}
		public XmlSchema GetSchema()
		{
			return null;
		}
		public void ReadXml(XmlReader reader)
		{
			XmlSerializer xmlSerializer = new XmlSerializer(typeof(TKey));
			XmlSerializer xmlSerializer2 = new XmlSerializer(typeof(TValue));
			bool isEmptyElement = reader.IsEmptyElement;
			reader.Read();
			if (isEmptyElement)
			{
				return;
			}
			using (this.EnterWriteLock())
			{
				while (reader.NodeType != XmlNodeType.EndElement)
				{
					reader.ReadStartElement("item");
					reader.ReadStartElement("key");
					TKey key = (TKey)((object)xmlSerializer.Deserialize(reader));
					reader.ReadEndElement();
					reader.ReadStartElement("value");
					TValue value = (TValue)((object)xmlSerializer2.Deserialize(reader));
					reader.ReadEndElement();
					this.innerDictionary.Add(key, value);
					reader.ReadEndElement();
					reader.MoveToContent();
				}
			}
			reader.ReadEndElement();
		}
		public void WriteXml(XmlWriter writer)
		{
			XmlSerializer xmlSerializer = new XmlSerializer(typeof(TKey));
			XmlSerializer xmlSerializer2 = new XmlSerializer(typeof(TValue));
			using (this.EnterReadLock())
			{
				foreach (TKey current in this.innerDictionary.Keys)
				{
					writer.WriteStartElement("item");
					writer.WriteStartElement("key");
					xmlSerializer.Serialize(writer, current);
					writer.WriteEndElement();
					writer.WriteStartElement("value");
					TValue tValue = this.innerDictionary[current];
					xmlSerializer2.Serialize(writer, tValue);
					writer.WriteEndElement();
					writer.WriteEndElement();
				}
			}
		}
		/// <summary>
		/// Enters the read lock.
		/// </summary>
		/// <returns></returns>
		public IDisposable EnterReadLock()
		{
			this._lock.TryEnterReadLock(500);
			return new ThreadSafeDictionary<TKey, TValue>.DisposableAction(new Action(this._lock.ExitReadLock));
		}
		/// <summary>
		/// Enters the upgradeable read lock.
		/// </summary>
		/// <returns></returns>
		public IDisposable EnterUpgradeableReadLock()
		{
			this._lock.TryEnterUpgradeableReadLock(500);
			return new ThreadSafeDictionary<TKey, TValue>.DisposableAction(new Action(this._lock.ExitUpgradeableReadLock));
		}
		/// <summary>
		/// Enters the write lock.
		/// </summary>
		/// <returns></returns>
		public IDisposable EnterWriteLock()
		{
			this._lock.TryEnterWriteLock(500);
			return new ThreadSafeDictionary<TKey, TValue>.DisposableAction(new Action(this._lock.ExitWriteLock));
		}
	}
}
