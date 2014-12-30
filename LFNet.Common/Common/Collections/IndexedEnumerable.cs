using System.Collections.Generic;

namespace LFNet.Common.Collections
{
    /// <summary>
    /// IndexedEnumerable makes enumerating over collections much easier by implementing properties like: IsEven, IsOdd, IsLast.
    /// </summary>
    public static class IndexedEnumerable
    {
        /// <summary>
        /// Returns an IndexedEnumerable from any collection implementing IEnumerable&lt;T&gt;
        /// </summary>
        /// <typeparam name="T">Type of enumerable</typeparam>
        /// <param name="source">Source enumerable</param>
        /// <returns>A new IndexedEnumerable&lt;T&gt;.</returns>
        public static IndexedEnumerable<T> AsIndexedEnumerable<T>(this IEnumerable<T> source)
        {
            return new IndexedEnumerable<T>(source);
        }

        /// <summary>
        /// Returns an IndexedEnumerable from any collection implementing IEnumerable&lt;T&gt;
        /// </summary>
        /// <typeparam name="T">Type of enumerable</typeparam>
        /// <param name="source">Source enumerable</param>
        /// <returns>A new IndexedEnumerable&lt;T&gt;.</returns>
        public static IndexedEnumerable<T> Create<T>(IEnumerable<T> source)
        {
            return new IndexedEnumerable<T>(source);
        }
    }
}

