using System.Collections;
using System.Collections.Generic;

namespace LFNet.Common.Collections
{
    public interface IReadOnlyCollection<out T> : IEnumerable<T>, IEnumerable
    {
        /// <summary>
        /// How many items are in the collection.
        /// </summary>
        int Count { get; }

        /// <summary>
        /// Gets the item with the specified index.
        /// </summary>
        /// <returns>
        /// The item with the specified index.
        /// </returns>
        T this[int index] { get; }
    }
}

