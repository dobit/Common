using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;

namespace LFNet.Common.Collections
{
    public interface INamedObjectCollection<T> : IObservableList<T>, IList<T>, ICollection<T>, IEnumerable<T>, IEnumerable, INotifyCollectionChanged, INotifyPropertyChanged where T: INamedObject
    {
        /// <summary>
        /// Determines whether an element is in the collection with the specified name.
        /// </summary>
        /// <param name="name">The name of the item to locate in the collection.</param>
        /// <returns>
        /// <c>true</c> if item is found in the collection; otherwise, <c>false</c>.
        /// </returns>
        bool Contains(string name);
        /// <summary>
        /// Determines the index of a specific item in the list.
        /// </summary>
        /// <param name="name">The name of the item to locate in the list.</param>
        /// <returns>The index of item if found in the list; otherwise, -1.</returns>
        int IndexOf(string name);

        /// <summary>
        /// Gets the item with the specified name.
        /// </summary>
        /// <returns>
        /// The item with the specified name.
        /// </returns>
        T this[string name] { get; }
    }
}

