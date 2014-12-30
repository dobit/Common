using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;

namespace System.Collections.Concurrent
{
    /// <summary>Provides a thread-safe priority queue data structure.</summary>
    /// <typeparam name="TKey">Specifies the type of keys used to prioritize values.</typeparam>
    /// <typeparam name="TValue">Specifies the type of elements in the queue.</typeparam>
    [DebuggerDisplay("Count={Count}")]
    public class ConcurrentPriorityQueue<TKey, TValue> : IProducerConsumerCollection<KeyValuePair<TKey, TValue>>, IEnumerable<KeyValuePair<TKey, TValue>>, ICollection, IEnumerable where TKey: IComparable<TKey>
    {
        private readonly MinBinaryHeap _minHeap;
        private readonly object _syncLock;

        /// <summary>Initializes a new instance of the ConcurrentPriorityQueue class.</summary>
        public ConcurrentPriorityQueue()
        {
            this._syncLock = new object();
            this._minHeap = new MinBinaryHeap();
        }

        /// <summary>Initializes a new instance of the ConcurrentPriorityQueue class that contains elements copied from the specified collection.</summary>
        /// <param name="collection">The collection whose elements are copied to the new ConcurrentPriorityQueue.</param>
        public ConcurrentPriorityQueue(IEnumerable<KeyValuePair<TKey, TValue>> collection)
        {
            this._syncLock = new object();
            this._minHeap = new MinBinaryHeap();
            if (collection == null)
            {
                throw new ArgumentNullException("collection");
            }
            foreach (KeyValuePair<TKey, TValue> pair in collection)
            {
                this._minHeap.Insert(pair);
            }
        }

        /// <summary>Empties the queue.</summary>
        public void Clear()
        {
            lock (this._syncLock)
            {
                this._minHeap.Clear();
            }
        }

        /// <summary>Copies the elements of the collection to an array, starting at a particular array index.</summary>
        /// <param name="array">
        /// The one-dimensional array that is the destination of the elements copied from the queue.
        /// </param>
        /// <param name="index">
        /// The zero-based index in array at which copying begins.
        /// </param>
        /// <remarks>The elements will not be copied to the array in any guaranteed order.</remarks>
        public void CopyTo(KeyValuePair<TKey, TValue>[] array, int index)
        {
            lock (this._syncLock)
            {
                this._minHeap.Items.CopyTo(array, index);
            }
        }

        /// <summary>Adds the key/value pair to the priority queue.</summary>
        /// <param name="item">The key/value pair to be added to the queue.</param>
        public void Enqueue(KeyValuePair<TKey, TValue> item)
        {
            lock (this._syncLock)
            {
                this._minHeap.Insert(item);
            }
        }

        /// <summary>Adds the key/value pair to the priority queue.</summary>
        /// <param name="priority">The priority of the item to be added.</param>
        /// <param name="value">The item to be added.</param>
        public void Enqueue(TKey priority, TValue value)
        {
            this.Enqueue(new KeyValuePair<TKey, TValue>(priority, value));
        }

        /// <summary>Returns an enumerator that iterates through the collection.</summary>
        /// <returns>An enumerator for the contents of the queue.</returns>
        /// <remarks>
        /// The enumeration represents a moment-in-time snapshot of the contents of the queue. It does not
        /// reflect any updates to the collection after GetEnumerator was called. The enumerator is safe to
        /// use concurrently with reads from and writes to the queue.
        /// </remarks>
        public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator()
        {
            return ((IEnumerable<KeyValuePair<TKey, TValue>>) this.ToArray()).GetEnumerator();
        }

        bool IProducerConsumerCollection<KeyValuePair<TKey, TValue>>.TryAdd(KeyValuePair<TKey, TValue> item)
        {
            this.Enqueue(item);
            return true;
        }

        bool IProducerConsumerCollection<KeyValuePair<TKey, TValue>>.TryTake(out KeyValuePair<TKey, TValue> item)
        {
            return this.TryDequeue(out item);
        }

        /// <summary>Copies the elements of the collection to an array, starting at a particular array index.</summary>
        /// <param name="array">
        /// The one-dimensional array that is the destination of the elements copied from the queue.
        /// </param>
        /// <param name="index">
        /// The zero-based index in array at which copying begins.
        /// </param>
        void ICollection.CopyTo(Array array, int index)
        {
            lock (this._syncLock)
            {
                ((ICollection)this._minHeap.Items).CopyTo(array, index);
            }
        }

        /// <summary>Returns an enumerator that iterates through a collection.</summary>
        /// <returns>An IEnumerator that can be used to iterate through the collection.</returns>
        IEnumerator IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
        }

        /// <summary>Copies the elements stored in the queue to a new array.</summary>
        /// <returns>A new array containing a snapshot of elements copied from the queue.</returns>
        public KeyValuePair<TKey, TValue>[] ToArray()
        {
            lock (this._syncLock)
            {
                MinBinaryHeap heap = new MinBinaryHeap(this._minHeap);
                KeyValuePair<TKey, TValue>[] pairArray = new KeyValuePair<TKey, TValue>[this._minHeap.Count];
                for (int i = 0; i < pairArray.Length; i++)
                {
                    pairArray[i] = heap.Remove();
                }
                return pairArray;
            }
        }

        /// <summary>Attempts to remove and return the next prioritized item in the queue.</summary>
        /// <param name="result">
        /// When this method returns, if the operation was successful, result contains the object removed. If
        /// no object was available to be removed, the value is unspecified.
        /// </param>
        /// <returns>
        /// true if an element was removed and returned from the queue succesfully; otherwise, false.
        /// </returns>
        public bool TryDequeue(out KeyValuePair<TKey, TValue> result)
        {
            result = new KeyValuePair<TKey, TValue>();
            lock (this._syncLock)
            {
                if (this._minHeap.Count > 0)
                {
                    result = this._minHeap.Remove();
                    return true;
                }
            }
            return false;
        }

        /// <summary>Attempts to return the next prioritized item in the queue.</summary>
        /// <param name="result">
        /// When this method returns, if the operation was successful, result contains the object.
        /// The queue was not modified by the operation.
        /// </param>
        /// <returns>
        /// true if an element was returned from the queue succesfully; otherwise, false.
        /// </returns>
        public bool TryPeek(out KeyValuePair<TKey, TValue> result)
        {
            result = new KeyValuePair<TKey, TValue>();
            lock (this._syncLock)
            {
                if (this._minHeap.Count > 0)
                {
                    result = this._minHeap.Peek();
                    return true;
                }
            }
            return false;
        }

        /// <summary>Gets the number of elements contained in the queue.</summary>
        public int Count
        {
            get
            {
                lock (this._syncLock)
                {
                    return this._minHeap.Count;
                }
            }
        }

        /// <summary>Gets whether the queue is empty.</summary>
        public bool IsEmpty
        {
            get
            {
                return (this.Count == 0);
            }
        }

        /// <summary>
        /// Gets a value indicating whether access to the ICollection is synchronized with the SyncRoot.
        /// </summary>
        bool ICollection.IsSynchronized
        {
            get
            {
                return true;
            }
        }

        /// <summary>
        /// Gets an object that can be used to synchronize access to the collection.
        /// </summary>
        object ICollection.SyncRoot
        {
            get
            {
                return this._syncLock;
            }
        }

        /// <summary>Implements a binary heap that prioritizes smaller values.</summary>
        private sealed class MinBinaryHeap
        {
            private readonly List<KeyValuePair<TKey, TValue>> _items;

            /// <summary>Initializes an empty heap.</summary>
            public MinBinaryHeap()
            {
                this._items = new List<KeyValuePair<TKey, TValue>>();
            }

            public MinBinaryHeap(ConcurrentPriorityQueue<TKey, TValue>.MinBinaryHeap heapToCopy)
            {
                this._items = new List<KeyValuePair<TKey, TValue>>(heapToCopy.Items);
            }

            /// <summary>Empties the heap.</summary>
            public void Clear()
            {
                this._items.Clear();
            }

            /// <summary>Adds an item to the heap.</summary>
            public void Insert(KeyValuePair<TKey, TValue> entry)
            {
                this._items.Add(entry);
                int num = this._items.Count - 1;
                if (num != 0)
                {
                    while (num > 0)
                    {
                        int num2 = (num - 1) / 2;
                        KeyValuePair<TKey, TValue> pair = this._items[num2];
                        if (entry.Key.CompareTo(pair.Key) >= 0)
                        {
                            break;
                        }
                        this._items[num] = pair;
                        num = num2;
                    }
                }
                else
                {
                    return;
                }
                this._items[num] = entry;
            }

            /// <summary>Adds an item to the heap.</summary>
            public void Insert(TKey key, TValue value)
            {
                this.Insert(new KeyValuePair<TKey, TValue>(key, value));
            }

            /// <summary>Returns the entry at the top of the heap.</summary>
            public KeyValuePair<TKey, TValue> Peek()
            {
                if (this._items.Count == 0)
                {
                    throw new InvalidOperationException("The heap is empty.");
                }
                return this._items[0];
            }

            /// <summary>Removes the entry at the top of the heap.</summary>
            public KeyValuePair<TKey, TValue> Remove()
            {
                int num3;
                if (this._items.Count == 0)
                {
                    throw new InvalidOperationException("The heap is empty.");
                }
                KeyValuePair<TKey, TValue> pair = this._items[0];
                if (this._items.Count <= 2)
                {
                    this._items.RemoveAt(0);
                    return pair;
                }
                this._items[0] = this._items[this._items.Count - 1];
                this._items.RemoveAt(this._items.Count - 1);
                int num = 0;
                int num2 = 0;
            Label_0084:
                num3 = (2 * num) + 1;
                int num4 = num3 + 1;
                if (num3 < this._items.Count)
                {
                    KeyValuePair<TKey, TValue> pair2 = this._items[num];
                    KeyValuePair<TKey, TValue> pair3 = this._items[num3];
                    if (pair3.Key.CompareTo(pair2.Key) < 0)
                    {
                        num2 = num3;
                    }
                    if (num4 < this._items.Count)
                    {
                        KeyValuePair<TKey, TValue> pair4 = this._items[num2];
                        KeyValuePair<TKey, TValue> pair5 = this._items[num4];
                        if (pair5.Key.CompareTo(pair4.Key) < 0)
                        {
                            num2 = num4;
                        }
                    }
                    if (num != num2)
                    {
                        KeyValuePair<TKey, TValue> pair6 = this._items[num];
                        this._items[num] = this._items[num2];
                        this._items[num2] = pair6;
                        num = num2;
                        goto Label_0084;
                    }
                }
                return pair;
            }

            /// <summary>Gets the number of objects stored in the heap.</summary>
            public int Count
            {
                get
                {
                    return this._items.Count;
                }
            }

            internal List<KeyValuePair<TKey, TValue>> Items
            {
                get
                {
                    return this._items;
                }
            }
        }
    }
}

