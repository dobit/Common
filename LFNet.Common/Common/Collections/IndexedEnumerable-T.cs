using System;
using System.Collections;
using System.Collections.Generic;

namespace LFNet.Common.Collections
{
    /// <summary>
    /// IndexedEnumerable makes enumerating over collections much easier by implementing properties like: IsEven, IsOdd, IsLast.
    /// </summary>
    /// <typeparam name="T">Type to iterate over</typeparam>
    public class IndexedEnumerable<T> : IEnumerable<IndexedEnumerable<T>.EntryItem>, IEnumerable
    {
        private readonly IEnumerable<T> _enumerable;

        /// <summary>
        /// Default constructor.
        /// </summary>
        private IndexedEnumerable()
        {
        }

        /// <summary>
        /// Constructor that takes an IEnumerable&lt;T&gt;
        /// </summary>
        /// <param name="enumerable">The collection to enumerate.</param>
        public IndexedEnumerable(IEnumerable<T> enumerable)
        {
            if (enumerable == null)
            {
                throw new ArgumentNullException("enumerable");
            }
            this._enumerable = enumerable;
        }

        /// <summary>
        /// Returns an enumeration of Entry objects.
        /// </summary>
        public IEnumerator<EntryItem> GetEnumerator()
        {
            using (IEnumerator<T> iteratorVariable0 = this._enumerable.GetEnumerator())
            {
                if (iteratorVariable0.MoveNext())
                {
                    int iteratorVariable1 = 0;
                    bool isFirst = true;
                    bool isLast = false;
                    while (!isLast)
                    {
                        T current = iteratorVariable0.Current;
                        isLast = !iteratorVariable0.MoveNext();
                        yield return new EntryItem(isFirst, isLast, current, iteratorVariable1++);
                        isFirst = false;
                    }
                }
            }
        }

        /// <summary>
        /// Non-generic form of GetEnumerator.
        /// </summary>
        IEnumerator IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
        }


        /// <summary>
        /// Represents each entry returned within a collection,
        /// containing the _value and whether it is the first and/or
        /// the last entry in the collection's. enumeration
        /// </summary>
        public class EntryItem
        {
            internal EntryItem()
            {
            }

            internal EntryItem(bool isFirst, bool isLast, T value, int index)
            {
                this.IsFirst = isFirst;
                this.IsLast = isLast;
                this.Value = value;
                this.Index = index;
                this.IsEven = (index % 2) == 0;
                this.IsOdd = !this.IsEven;
            }

            public static implicit operator T(IndexedEnumerable<T>.EntryItem item)
            {
                return item.Value;
            }

            /// <summary>
            /// The index of the current item in the collection.
            /// </summary>
            public int Index { get; internal set; }

            /// <summary>
            /// Returns true if the current item has an even index
            /// </summary>
            public bool IsEven { get; internal set; }

            /// <summary>
            /// Returns true if it is the first item in the collection.
            /// </summary>
            public bool IsFirst { get; internal set; }

            /// <summary>
            /// Returns true if it is the last item in the collection.
            /// </summary>
            public bool IsLast { get; internal set; }

            /// <summary>
            /// Returns true if the current item has an odd index
            /// </summary>
            public bool IsOdd { get; internal set; }

            /// <summary>
            /// The Entry Value.
            /// </summary>
            public T Value { get; internal set; }
        }
    }
}

