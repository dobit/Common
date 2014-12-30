﻿using System;
using System.Collections.Concurrent;
using System.Collections.Generic;

namespace System.Collections.Concurrent
{
    /// <summary>Extension methods for IProducerConsumerCollection.</summary>
    public static class ProducerConsumerCollectionExtensions
    {
        /// <summary>Adds the contents of an enumerable to the collection.</summary>
        /// <typeparam name="T">Specifies the type of the elements in the collection.</typeparam>
        /// <param name="target">The target collection to be augmented.</param>
        /// <param name="source">The source enumerable containing the data to be added.</param>
        public static void AddFromEnumerable<T>(this IProducerConsumerCollection<T> target, IEnumerable<T> source)
        {
            foreach (T local in source)
            {
                target.TryAdd(local);
            }
        }

        /// <summary>Adds the contents of an observable to the collection.</summary>
        /// <typeparam name="T">Specifies the type of the elements in the collection.</typeparam>
        /// <param name="target">The target collection to be augmented.</param>
        /// <param name="source">The source observable containing the data to be added.</param>
        /// <returns>An IDisposable that may be used to cancel the transfer.</returns>
        public static IDisposable AddFromObservable<T>(this IProducerConsumerCollection<T> target, IObservable<T> source)
        {
            if (target == null)
            {
                throw new ArgumentNullException("target");
            }
            if (source == null)
            {
                throw new ArgumentNullException("source");
            }
            Action<T> onNext = delegate (T item) {
                target.TryAdd(item);
            };
            Action<Exception> onError = delegate (Exception error) {
            };
            Action onCompleted = delegate {
            };
            return source.Subscribe(new DelegateBasedObserver<T>(onNext, onError, onCompleted));
        }

        /// <summary>Clears the collection by repeatedly taking elements until it's empty.</summary>
        /// <typeparam name="T">Specifies the type of the elements in the collection.</typeparam>
        /// <param name="collection">The collection to be cleared.</param>
        public static void Clear<T>(this IProducerConsumerCollection<T> collection)
        {
            T local;
            while (collection.TryTake(out local))
            {
            }
        }

        /// <summary>Creates an enumerable which will consume and return elements from the collection.</summary>
        /// <typeparam name="T">Specifies the type of the elements in the collection.</typeparam>
        /// <param name="collection">The collection to be consumed.</param>
        /// <returns>An enumerable that consumes elements from the collection and returns them.</returns>
        public static IEnumerable<T> GetConsumingEnumerable<T>(this IProducerConsumerCollection<T> collection)
        {
            while (true)
            {
                T iteratorVariable0;
                if (!collection.TryTake(out iteratorVariable0))
                {
                    yield break;
                }
                yield return iteratorVariable0;
            }
        }

        /// <summary>Creates a take-only facade for the collection.</summary>
        /// <typeparam name="T">Specifies the type of the elements in the collection.</typeparam>
        /// <param name="collection">The collection to be wrapped.</param>
        /// <returns>
        /// An IProducerConsumerCollection that wraps the target collection and supports only take
        /// functionality, not add.
        /// </returns>
        public static IProducerConsumerCollection<T> ToConsumerOnlyCollection<T>(this IProducerConsumerCollection<T> collection)
        {
            return new ProduceOrConsumeOnlyCollection<T>(collection, false);
        }

        /// <summary>Creates an add-only facade for the collection.</summary>
        /// <typeparam name="T">Specifies the type of the elements in the collection.</typeparam>
        /// <param name="collection">The collection to be wrapped.</param>
        /// <returns>
        /// An IProducerConsumerCollection that wraps the target collection and supports only add
        /// functionality, not take.
        /// </returns>
        public static IProducerConsumerCollection<T> ToProducerOnlyCollection<T>(this IProducerConsumerCollection<T> collection)
        {
            return new ProduceOrConsumeOnlyCollection<T>(collection, true);
        }

       

        private sealed class ProduceOrConsumeOnlyCollection<T> : ProducerConsumerCollectionBase<T>
        {
            private readonly bool _produceOnly;

            public ProduceOrConsumeOnlyCollection(IProducerConsumerCollection<T> contained, bool produceOnly) : base(contained)
            {
                this._produceOnly = produceOnly;
            }

            protected override bool TryAdd(T item)
            {
                if (!this._produceOnly)
                {
                    throw new NotSupportedException();
                }
                return base.TryAdd(item);
            }

            protected override bool TryTake(out T item)
            {
                if (this._produceOnly)
                {
                    throw new NotSupportedException();
                }
                return base.TryTake(out item);
            }
        }
    }
}

