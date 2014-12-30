using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;

namespace LFNet.Common.Collections
{
	public class WrappedObservableCollection<TBase, TActual> : IObservableCollection<TBase>, ICollection<TBase>, IEnumerable<TBase>, IEnumerable, INotifyCollectionChanged, INotifyPropertyChanged where TBase : class where TActual : class, TBase
	{
        protected readonly global::System.Collections.ObjectModel.ObservableCollection<TActual> _innerCollection;
		/// <summary>
		/// Occurs when an item is added, removed, changed, moved, or the entire list is refreshed.
		/// </summary>
		[field: NonSerialized]
		public event NotifyCollectionChangedEventHandler CollectionChanged;
		/// <summary>
		/// Occurs when a property value changes.
		/// </summary>
		[field: NonSerialized]
		public event PropertyChangedEventHandler PropertyChanged;
		/// <summary>
		/// Gets the number of elements actually contained in the <see cref="T:System.Collections.Generic.List`1" />.
		/// </summary>
		/// <returns>
		/// The number of elements actually contained in the <see cref="T:System.Collections.Generic.List`1" />.
		/// </returns>
		public int Count
		{
			get
			{
				return this._innerCollection.Count;
			}
		}
		bool ICollection<TBase>.IsReadOnly
		{
			get
			{
				return ((ICollection<TBase>)this._innerCollection).IsReadOnly;
			}
		}
		public WrappedObservableCollection() : this(null)
		{
		}
		public WrappedObservableCollection(IEnumerable<TBase> data) : this((IEnumerable<TActual>) ((data == null) ? null : data.Cast<TActual>()))
		{
		}
		public WrappedObservableCollection(IEnumerable<TActual> data)
		{
            this._innerCollection = ((data == null) ? new global::System.Collections.ObjectModel.ObservableCollection<TActual>() : new global::System.Collections.ObjectModel.ObservableCollection<TActual>(data));
			this._innerCollection.CollectionChanged += new NotifyCollectionChangedEventHandler(this.InnerCollectionChanged);
			((INotifyPropertyChanged)this._innerCollection).PropertyChanged += new PropertyChangedEventHandler(this.InnerCollectionPropertyChanged);
		}
		/// <summary>
		/// Adds an object to the end of the <see cref="T:System.Collections.Generic.List`1" />.
		/// </summary>
		/// <param name="item">
		/// The object to be added to the end of the <see cref="T:System.Collections.Generic.List`1" />. The value can be <c>null</c> for reference types.
		/// </param>
		public void Add(TBase item)
		{
			this._innerCollection.Add((TActual)((object)item));
		}
		/// <summary>
		/// Removes all elements from the <see cref="T:System.Collections.Generic.List`1" />.
		/// </summary>
		public void Clear()
		{
			this._innerCollection.Clear();
		}
		/// <summary>
		/// Determines whether an element is in the <see cref="T:System.Collections.Generic.List`1" />.
		/// </summary>
		/// <returns><c>true</c> if <paramref name="item" /> is found in the <see cref="T:System.Collections.Generic.List`1" />; otherwise, <c>false</c>.
		/// </returns>
		/// <param name="item">
		/// The object to locate in the <see cref="T:System.Collections.Generic.List`1" />. The value can be <c>null</c> for reference types.
		/// </param>
		public bool Contains(TBase item)
		{
			return this._innerCollection.Contains(item);
		}
		/// <summary>
		/// Copies the entire <see cref="T:System.Collections.Generic.List`1" /> to a compatible one-dimensional array, starting at the specified index of the target array.
		/// </summary>
		/// <param name="array">
		/// The one-dimensional <see cref="T:System.Array" /> that is the destination of the elements copied from <see cref="T:System.Collections.Generic.List`1" />. The <see cref="T:System.Array" /> must have zero-based indexing.
		/// </param>
		/// <param name="arrayIndex">
		/// The zero-based index in <paramref name="array" /> at which copying begins.
		/// </param>
		/// <exception cref="T:System.ArgumentNullException">
		/// <paramref name="array" /> is <c>null</c>.
		/// </exception>
		/// <exception cref="T:System.ArgumentOutOfRangeException">
		/// <paramref name="arrayIndex" /> is less than 0.
		/// </exception>
		/// <exception cref="T:System.ArgumentException">
		/// <paramref name="arrayIndex" /> is equal to or greater than the length of <paramref name="array" />.
		///
		/// -or-
		///
		/// The number of elements in the source <see cref="T:System.Collections.Generic.List`1" /> is greater than the available space from <paramref name="arrayIndex" /> to the end of the destination <paramref name="array" />.
		/// </exception>
		public void CopyTo(TBase[] array, int arrayIndex)
		{
			this._innerCollection.CopyTo((TActual[])array, arrayIndex);
		}
		/// <summary>
		/// Removes the first occurrence of a specific object from the <see cref="T:System.Collections.Generic.List`1" />.
		/// </summary>
		/// <returns><c>true</c> if <paramref name="item" /> is successfully removed; otherwise, <c>false</c>.  This method also returns <c>false</c> if <paramref name="item" /> was not found in the <see cref="T:System.Collections.Generic.List`1" />.
		/// </returns>
		/// <param name="item">
		/// The object to remove from the <see cref="T:System.Collections.Generic.List`1" />. The value can be <c>null</c> for reference types.
		/// </param>
		public bool Remove(TBase item)
		{
			return this._innerCollection.Remove((TActual)((object)item));
		}
		/// <summary>
		/// Returns an enumerator that iterates through the <see cref="T:System.Collections.Generic.List`1" />.
		/// </summary>
		/// <returns>
		/// A <see cref="T:System.Collections.Generic.List`1.Enumerator" /> for the <see cref="T:System.Collections.Generic.List`1" />.
		/// </returns>
		public IEnumerator<TBase> GetEnumerator()
		{
			return ((IEnumerable<TBase>)this._innerCollection).GetEnumerator();
		}
		/// <summary>
		/// Returns an enumerator that iterates through a collection.
		/// </summary>
		/// <returns>
		/// An <see cref="T:System.Collections.IEnumerator" /> object that can be used to iterate through the collection.
		/// </returns>
		IEnumerator IEnumerable.GetEnumerator()
		{
			return ((IEnumerable)this._innerCollection).GetEnumerator();
		}
		private void InnerCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
		{
			if (this.CollectionChanged != null)
			{
				this.CollectionChanged(sender, e);
			}
		}
		private void InnerCollectionPropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			if (this.PropertyChanged != null)
			{
				this.PropertyChanged(sender, e);
			}
		}
	}
}
