using System.Collections;
using System.Collections.Generic;

namespace LFNet.Common.Collections
{
    public class ReadOnlyCollection<T> : IReadOnlyCollection<T>, IEnumerable<T>, IEnumerable
    {
        protected readonly IList<T> _items;

        /// <summary>
        /// Initializes a new instance of the <see cref="T:LFNet.Common.Collections.NamedObjectCollection`1" /> class.
        /// </summary>
        /// <param name="items">The items from which the elements are copied.</param>
        public ReadOnlyCollection(IEnumerable<T> items)
        {
            this._items = new List<T>(items);
        }

        public IEnumerator<T> GetEnumerator()
        {
            return this._items.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return this._items.GetEnumerator();
        }

        public int Count
        {
            get
            {
                return this._items.Count;
            }
        }

        public T this[int index]
        {
            get
            {
                return this._items[index];
            }
        }
    }
}

