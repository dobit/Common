using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Threading;

namespace System.Linq
{
    /// <summary>
    /// Provides LINQ support for Tasks by implementing the primary standard query operators.
    /// </summary>
    public static class LinqToTasks
    {
        public static Task<IGrouping<TKey, TElement>> GroupBy<TSource, TKey, TElement>(this Task<TSource> source, Func<TSource, TKey> keySelector, Func<TSource, TElement> elementSelector)
        {
            if (source == null)
            {
                throw new ArgumentNullException("source");
            }
            if (keySelector == null)
            {
                throw new ArgumentNullException("keySelector");
            }
            if (elementSelector == null)
            {
                throw new ArgumentNullException("elementSelector");
            }
            return source.ContinueWith<IGrouping<TKey, TElement>>(delegate (Task<TSource> t) {
                TSource arg = t.Result;
                TKey local2 = keySelector(arg);
                TElement local3 = elementSelector(arg);
                return new OneElementGrouping<TKey, TElement> { Key = local2, Element = local3 };
            }, TaskContinuationOptions.NotOnCanceled);
        }

        public static Task<TResult> GroupJoin<TOuter, TInner, TKey, TResult>(this Task<TOuter> outer, Task<TInner> inner, Func<TOuter, TKey> outerKeySelector, Func<TInner, TKey> innerKeySelector, Func<TOuter, Task<TInner>, TResult> resultSelector)
        {
            return outer.GroupJoin<TOuter, TInner, TKey, TResult>(inner, outerKeySelector, innerKeySelector, resultSelector, EqualityComparer<TKey>.Default);
        }

        public static Task<TResult> GroupJoin<TOuter, TInner, TKey, TResult>(this Task<TOuter> outer, Task<TInner> inner, Func<TOuter, TKey> outerKeySelector, Func<TInner, TKey> innerKeySelector, Func<TOuter, Task<TInner>, TResult> resultSelector, IEqualityComparer<TKey> comparer)
        {
            if (outer == null)
            {
                throw new ArgumentNullException("outer");
            }
            if (inner == null)
            {
                throw new ArgumentNullException("inner");
            }
            if (outerKeySelector == null)
            {
                throw new ArgumentNullException("outerKeySelector");
            }
            if (innerKeySelector == null)
            {
                throw new ArgumentNullException("innerKeySelector");
            }
            if (resultSelector == null)
            {
                throw new ArgumentNullException("resultSelector");
            }
            if (comparer == null)
            {
                throw new ArgumentNullException("comparer");
            }
            return outer.ContinueWith<Task<TResult>>(delegate {
                CancellationTokenSource cts = new CancellationTokenSource();
                return inner.ContinueWith<TResult>(delegate {
                    Task.WaitAll(new Task[] { outer, inner });
                    if (comparer.Equals(outerKeySelector(outer.Result), innerKeySelector(inner.Result)))
                    {
                        return resultSelector(outer.Result, inner);
                    }
                    cts.CancelAndThrow();
                    return default(TResult);
                }, cts.Token, TaskContinuationOptions.NotOnCanceled, TaskScheduler.Default);
            }, TaskContinuationOptions.NotOnCanceled).Unwrap<TResult>();
        }

        public static Task<TResult> Join<TOuter, TInner, TKey, TResult>(this Task<TOuter> outer, Task<TInner> inner, Func<TOuter, TKey> outerKeySelector, Func<TInner, TKey> innerKeySelector, Func<TOuter, TInner, TResult> resultSelector)
        {
            return outer.Join<TOuter, TInner, TKey, TResult>(inner, outerKeySelector, innerKeySelector, resultSelector, EqualityComparer<TKey>.Default);
        }

        public static Task<TResult> Join<TOuter, TInner, TKey, TResult>(this Task<TOuter> outer, Task<TInner> inner, Func<TOuter, TKey> outerKeySelector, Func<TInner, TKey> innerKeySelector, Func<TOuter, TInner, TResult> resultSelector, IEqualityComparer<TKey> comparer)
        {
            if (outer == null)
            {
                throw new ArgumentNullException("outer");
            }
            if (inner == null)
            {
                throw new ArgumentNullException("inner");
            }
            if (outerKeySelector == null)
            {
                throw new ArgumentNullException("outerKeySelector");
            }
            if (innerKeySelector == null)
            {
                throw new ArgumentNullException("innerKeySelector");
            }
            if (resultSelector == null)
            {
                throw new ArgumentNullException("resultSelector");
            }
            if (comparer == null)
            {
                throw new ArgumentNullException("comparer");
            }
            return outer.ContinueWith<Task<TResult>>(delegate {
                CancellationTokenSource cts = new CancellationTokenSource();
                return inner.ContinueWith<TResult>(delegate {
                    Task.WaitAll(new Task[] { outer, inner });
                    if (comparer.Equals(outerKeySelector(outer.Result), innerKeySelector(inner.Result)))
                    {
                        return resultSelector(outer.Result, inner.Result);
                    }
                    cts.CancelAndThrow();
                    return default(TResult);
                }, cts.Token, TaskContinuationOptions.NotOnCanceled, TaskScheduler.Default);
            }, TaskContinuationOptions.NotOnCanceled).Unwrap<TResult>();
        }

        public static Task<TSource> OrderBy<TSource, TKey>(this Task<TSource> source, Func<TSource, TKey> keySelector)
        {
            if (source == null)
            {
                throw new ArgumentNullException("source");
            }
            return source;
        }

        public static Task<TSource> OrderByDescending<TSource, TKey>(this Task<TSource> source, Func<TSource, TKey> keySelector)
        {
            if (source == null)
            {
                throw new ArgumentNullException("source");
            }
            return source;
        }

        public static Task<TResult> Select<TSource, TResult>(this Task<TSource> source, Func<TSource, TResult> selector)
        {
            if (source == null)
            {
                throw new ArgumentNullException("source");
            }
            if (selector == null)
            {
                throw new ArgumentNullException("selector");
            }
            return source.ContinueWith<TResult>(t => selector(t.Result), TaskContinuationOptions.NotOnCanceled);
        }

        public static Task<TResult> SelectMany<TSource, TResult>(this Task<TSource> source, Func<TSource, Task<TResult>> selector)
        {
            if (source == null)
            {
                throw new ArgumentNullException("source");
            }
            if (selector == null)
            {
                throw new ArgumentNullException("selector");
            }
            return source.ContinueWith<Task<TResult>>(t => selector(t.Result), TaskContinuationOptions.NotOnCanceled).Unwrap<TResult>();
        }

        public static Task<TResult> SelectMany<TSource, TCollection, TResult>(this Task<TSource> source, Func<TSource, Task<TCollection>> collectionSelector, Func<TSource, TCollection, TResult> resultSelector)
        {
            if (source == null)
            {
                throw new ArgumentNullException("source");
            }
            if (collectionSelector == null)
            {
                throw new ArgumentNullException("collectionSelector");
            }
            if (resultSelector == null)
            {
                throw new ArgumentNullException("resultSelector");
            }
            return source.ContinueWith<Task<TResult>>(t => collectionSelector(t.Result).ContinueWith<TResult>(c => resultSelector(t.Result, c.Result), TaskContinuationOptions.NotOnCanceled), TaskContinuationOptions.NotOnCanceled).Unwrap<TResult>();
        }

        public static Task<TSource> ThenBy<TSource, TKey>(this Task<TSource> source, Func<TSource, TKey> keySelector)
        {
            if (source == null)
            {
                throw new ArgumentNullException("source");
            }
            return source;
        }

        public static Task<TSource> ThenByDescending<TSource, TKey>(this Task<TSource> source, Func<TSource, TKey> keySelector)
        {
            if (source == null)
            {
                throw new ArgumentNullException("source");
            }
            return source;
        }

        public static Task<TSource> Where<TSource>(this Task<TSource> source, Func<TSource, bool> predicate)
        {
            if (source == null)
            {
                throw new ArgumentNullException("source");
            }
            if (predicate == null)
            {
                throw new ArgumentNullException("predicate");
            }
            CancellationTokenSource cts = new CancellationTokenSource();
            return source.ContinueWith<TSource>(delegate (Task<TSource> t) {
                TSource arg = t.Result;
                if (!predicate(arg))
                {
                    cts.CancelAndThrow();
                }
                return arg;
            }, cts.Token, TaskContinuationOptions.NotOnCanceled, TaskScheduler.Default);
        }

        /// <summary>Represents a grouping of one element.</summary>
        /// <typeparam name="TKey">The type of the key for the element.</typeparam>
        /// <typeparam name="TElement">The type of the element.</typeparam>
        private class OneElementGrouping<TKey, TElement> : IGrouping<TKey, TElement>, IEnumerable<TElement>, IEnumerable
        {
            public IEnumerator<TElement> GetEnumerator()
            {
                yield return this.Element;
            }

            IEnumerator IEnumerable.GetEnumerator()
            {
                return this.GetEnumerator();
            }

            internal TElement Element { get; set; }

            public TKey Key { get; internal set; }

          
        }
    }
}

