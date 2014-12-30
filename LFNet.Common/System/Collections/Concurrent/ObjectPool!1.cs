using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;

namespace System.Collections.Concurrent
{
    /// <summary>Provides a thread-safe object pool.</summary>
    /// <typeparam name="T">Specifies the type of the elements stored in the pool.</typeparam>
    [DebuggerTypeProxy(typeof(IProducerConsumerCollection_DebugView<>)), DebuggerDisplay("Count={Count}")]
    public sealed class ObjectPool<T> : ProducerConsumerCollectionBase<T>
    {
        private readonly Func<T> _generator;

        /// <summary>Initializes an instance of the ObjectPool class.</summary>
        /// <param name="generator">The function used to create items when no items exist in the pool.</param>
        public ObjectPool(Func<T> generator) : this(generator, new ConcurrentQueue<T>())
        {
        }

        /// <summary>Initializes an instance of the ObjectPool class.</summary>
        /// <param name="generator">The function used to create items when no items exist in the pool.</param>
        /// <param name="collection">The collection used to store the elements of the pool.</param>
        public ObjectPool(Func<T> generator, IProducerConsumerCollection<T> collection) : base(collection)
        {
            if (generator == null)
            {
                throw new ArgumentNullException("generator");
            }
            this._generator = generator;
        }

        /// <summary>Gets an item from the pool.</summary>
        /// <returns>The removed or created item.</returns>
        /// <remarks>If the pool is empty, a new item will be created and returned.</remarks>
        public T GetObject()
        {
            T local;
            if (!base.TryTake(out local))
            {
                return this._generator();
            }
            return local;
        }

        /// <summary>Adds the provided item into the pool.</summary>
        /// <param name="item">The item to be added.</param>
        public void PutObject(T item)
        {
            base.TryAdd(item);
        }

        /// <summary>Clears the object pool, returning all of the data that was in the pool.</summary>
        /// <returns>An array containing all of the elements in the pool.</returns>
        public T[] ToArrayAndClear()
        {
            T local;
            List<T> list = new List<T>();
            while (base.TryTake(out local))
            {
                list.Add(local);
            }
            return list.ToArray();
        }

        protected override bool TryAdd(T item)
        {
            this.PutObject(item);
            return true;
        }

        protected override bool TryTake(out T item)
        {
            item = this.GetObject();
            return true;
        }
    }
}

