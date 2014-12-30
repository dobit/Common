using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;

namespace LFNet.Common.Collections
{
	/// <summary>
	/// A collection that provides notifications when items get added, removed, or when the whole list is refreshed. 
	/// </summary>
	/// <typeparam name="T"></typeparam>
	[Serializable]
	public class ObservableCollection<T> : Collection<T>, INotifyPropertyChanged
	{
		[Serializable]
		private class SimpleMonitor : IDisposable
		{
			private int _busyCount;
			public bool Busy
			{
				get
				{
					return this._busyCount > 0;
				}
			}
			public void Dispose()
			{
				this._busyCount--;
			}
			public void Enter()
			{
				this._busyCount++;
			}
		}
		private const string COUNT_STRING = "Count";
		private const string INDEXER_NAME = "Item[]";
		private readonly ObservableCollection<T>.SimpleMonitor _monitor;
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
		/// Initializes a new instance of the <see cref="T:LFNet.Common.Collections.ObservableCollection`1" /> class.
		/// </summary>
		public ObservableCollection()
		{
			this._monitor = new ObservableCollection<T>.SimpleMonitor();
		}
		/// <summary>
		/// Initializes a new instance of the <see cref="T:LFNet.Common.Collections.ObservableCollection`1" /> class.
		/// </summary>
		/// <param name="collection">The collection.</param>
		public ObservableCollection(IEnumerable<T> collection)
		{
			this._monitor = new ObservableCollection<T>.SimpleMonitor();
			if (collection == null)
			{
				throw new ArgumentNullException("collection");
			}
			this.CopyFrom(collection);
		}
		/// <summary>
		/// Disallows reentrant attempts to change this collection.
		/// </summary>
		/// <returns>An IDisposable object that can be used to dispose of the object.</returns>
		protected IDisposable BlockReentrancy()
		{
			this._monitor.Enter();
			return this._monitor;
		}
		/// <summary>
		/// Checks for reentrant attempts to change this collection.
		/// </summary>
		protected void CheckReentrancy()
		{
			if (this._monitor.Busy && this.CollectionChanged != null && this.CollectionChanged.GetInvocationList().Length > 1)
			{
				throw new InvalidOperationException("ObservableCollectionReentrancyNotAllowed");
			}
		}
		/// <summary>
		/// Removes all elements from the <see cref="T:System.Collections.ObjectModel.Collection`1" />.
		/// </summary>
		protected override void ClearItems()
		{
			this.CheckReentrancy();
			base.ClearItems();
            this.OnPropertyChanged(COUNT_STRING);
            this.OnPropertyChanged(INDEXER_NAME);
			this.OnCollectionChanged();
		}
		private void CopyFrom(IEnumerable<T> collection)
		{
			IList<T> items = base.Items;
			if (collection != null && items != null)
			{
				using (IEnumerator<T> enumerator = collection.GetEnumerator())
				{
					while (enumerator.MoveNext())
					{
						items.Add(enumerator.Current);
					}
				}
			}
		}
		/// <summary>
		/// Inserts an element into the <see cref="T:System.Collections.ObjectModel.Collection`1" /> at the specified index.
		/// </summary>
		/// <param name="index">The zero-based index at which <paramref name="item" /> should be inserted.</param>
		/// <param name="item">The object to insert. The value can be null for reference types.</param>
		/// <exception cref="T:System.ArgumentOutOfRangeException">
		/// 	<paramref name="index" /> is less than zero.
		/// -or-
		/// <paramref name="index" /> is greater than <see cref="P:System.Collections.ObjectModel.Collection`1.Count" />.
		/// </exception>
		protected override void InsertItem(int index, T item)
		{
			this.CheckReentrancy();
			base.InsertItem(index, item);
            this.OnPropertyChanged(COUNT_STRING);
            this.OnPropertyChanged(INDEXER_NAME);
			this.OnCollectionChanged();
		}
		/// <summary>
		/// Moves the item at the specified index to a new location in the collection.
		/// </summary>
		/// <param name="oldIndex">The zero-based index specifying the location of the item to be moved.</param>
		/// <param name="newIndex">The zero-based index specifying the new location of the item.</param>
		public void Move(int oldIndex, int newIndex)
		{
			this.MoveItem(oldIndex, newIndex);
		}
		/// <summary>
		/// Moves the item at the specified index to a new location in the collection.
		/// </summary>
		/// <param name="oldIndex">The zero-based index specifying the location of the item to be moved.</param>
		/// <param name="newIndex">The zero-based index specifying the new location of the item.</param>
		protected virtual void MoveItem(int oldIndex, int newIndex)
		{
			this.CheckReentrancy();
			T item = base[oldIndex];
			base.RemoveItem(oldIndex);
			base.InsertItem(newIndex, item);
            this.OnPropertyChanged(INDEXER_NAME);
			this.OnCollectionChanged();
		}
		/// <summary>
		/// Raises the CollectionChanged event with the provided arguments.
		/// </summary>
		protected virtual void OnCollectionChanged()
		{
			if (this.CollectionChanged != null)
			{
				using (this.BlockReentrancy())
				{
					this.CollectionChanged(this, EventArgs.Empty);
				}
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
		private void OnPropertyChanged(string propertyName)
		{
			this.OnPropertyChanged(new PropertyChangedEventArgs(propertyName));
		}
		/// <summary>
		/// Removes the element at the specified index of the <see cref="T:System.Collections.ObjectModel.Collection`1" />.
		/// </summary>
		/// <param name="index">The zero-based index of the element to remove.</param>
		/// <exception cref="T:System.ArgumentOutOfRangeException">
		/// 	<paramref name="index" /> is less than zero.
		/// -or-
		/// <paramref name="index" /> is equal to or greater than <see cref="P:System.Collections.ObjectModel.Collection`1.Count" />.
		/// </exception>
		protected override void RemoveItem(int index)
		{
			this.CheckReentrancy();
			T arg_0D_0 = base[index];
			base.RemoveItem(index);
            this.OnPropertyChanged(COUNT_STRING);
            this.OnPropertyChanged(INDEXER_NAME);
			this.OnCollectionChanged();
		}
		/// <summary>
		/// Replaces the element at the specified index.
		/// </summary>
		/// <param name="index">The zero-based index of the element to replace.</param>
		/// <param name="item">The new value for the element at the specified index. The value can be null for reference types.</param>
		/// <exception cref="T:System.ArgumentOutOfRangeException">
		/// 	<paramref name="index" /> is less than zero.
		/// -or-
		/// <paramref name="index" /> is greater than <see cref="P:System.Collections.ObjectModel.Collection`1.Count" />.
		/// </exception>
		protected override void SetItem(int index, T item)
		{
			this.CheckReentrancy();
			T arg_0D_0 = base[index];
			base.SetItem(index, item);
            this.OnPropertyChanged(INDEXER_NAME);
			this.OnCollectionChanged();
		}
	}
}
