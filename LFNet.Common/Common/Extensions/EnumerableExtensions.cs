using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace LFNet.Common.Extensions
{
    public static class EnumerableExtensions
    {
        private static readonly global::System.Random _random = new global::System.Random();

        public static void AddRange<T>(this ICollection<T> list, IEnumerable<T> range)
        {
            foreach (T local in range)
            {
                list.Add(local);
            }
        }

        public static IEnumerable<T> AsNullIfEmpty<T>(this IEnumerable<T> items)
        {
            if ((items != null) && items.Any<T>())
            {
                return items;
            }
            return null;
        }

        public static bool Contains<T>(this IEnumerable<T> enumerable, Func<T, bool> function)
        {
            T objA = enumerable.FirstOrDefault<T>(function);
            T objB = default(T);
            return !object.Equals(objA, objB);
        }

        public static IEnumerable<TSource> DistinctBy<TSource, TKey>(this IEnumerable<TSource> source, Func<TSource, TKey> keySelector)
        {
            return source.DistinctBy<TSource, TKey>(keySelector, EqualityComparer<TKey>.Default);
        }

        public static IEnumerable<TSource> DistinctBy<TSource, TKey>(this IEnumerable<TSource> source, Func<TSource, TKey> keySelector, IEqualityComparer<TKey> comparer)
        {
            if (source == null)
            {
                throw new ArgumentNullException("source");
            }
            if (keySelector == null)
            {
                throw new ArgumentNullException("keySelector");
            }
            if (comparer == null)
            {
                throw new ArgumentNullException("comparer");
            }
            return DistinctByImpl<TSource, TKey>(source, keySelector, comparer);
        }

        private static IEnumerable<TSource> DistinctByImpl<TSource, TKey>(IEnumerable<TSource> source, Func<TSource, TKey> keySelector, IEqualityComparer<TKey> comparer)
        {
            HashSet<TKey> iteratorVariable0 = new HashSet<TKey>(comparer);
            foreach (TSource iteratorVariable1 in source)
            {
                if (iteratorVariable0.Add(keySelector(iteratorVariable1)))
                {
                    yield return iteratorVariable1;
                }
            }
        }

        public static void ForEach<T>(this IEnumerable<T> collection, Action<T> action)
        {
            foreach (T local in collection)
            {
                action(local);
            }
        }

        public static int IndexOf<TSource>(this IEnumerable<TSource> source, Func<TSource, bool> selector)
        {
            int num = 0;
            foreach (TSource local in source)
            {
                if (selector(local))
                {
                    return num;
                }
                num++;
            }
            return -1;
        }

        public static int IndexOf<TSource>(this IEnumerable<TSource> source, TSource item)
        {
            return source.IndexOf<TSource>(item, null);
        }

        public static int IndexOf<TSource>(this IEnumerable<TSource> source, TSource item, IEqualityComparer<TSource> itemComparer)
        {
            if (source == null)
            {
                throw new ArgumentNullException("source");
            }
            IList<TSource> list = source as IList<TSource>;
            if (list != null)
            {
                return list.IndexOf(item);
            }
            IList list2 = source as IList;
            if (list2 != null)
            {
                return list2.IndexOf(item);
            }
            if (itemComparer == null)
            {
                itemComparer = EqualityComparer<TSource>.Default;
            }
            int num = 0;
            foreach (TSource local in source)
            {
                if (itemComparer.Equals(item, local))
                {
                    return num;
                }
                num++;
            }
            return -1;
        }

        public static bool IsNullOrEmpty<T>(this IEnumerable<T> items)
        {
            if (items != null)
            {
                return !items.Any<T>();
            }
            return true;
        }

        public static T Random<T>(this IEnumerable<T> items)
        {
            if (items == null)
            {
                throw new ArgumentNullException("items");
            }
            int maxValue = items.Count<T>();
            if (maxValue == 0)
            {
                return default(T);
            }
            return items.ElementAt<T>(_random.Next(maxValue));
        }

        public static IList<T> Shuffle<T>(this IList<T> list)
        {
            int count = list.Count;
            while (count > 1)
            {
                count--;
                int num2 = _random.Next(count + 1);
                T local = list[num2];
                list[num2] = list[count];
                list[count] = local;
            }
            return list;
        }

        public static IEnumerable<T> TakeRandom<T>(this IEnumerable<T> items, int count)
        {
            if (items == null)
            {
                throw new ArgumentNullException("items");
            }
            return (from t in items
                orderby _random.Next()
                select t).Take<T>(count);
        }

       
    }
}

