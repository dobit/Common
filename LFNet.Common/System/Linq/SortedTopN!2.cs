using System;
using System.Collections;
using System.Collections.Generic;

namespace System.Linq
{
    internal class SortedTopN<TKey, TValue> : IEnumerable<KeyValuePair<TKey, TValue>>, IEnumerable
    {
        private IComparer<TKey> _comparer;
        private int _n;
        private List<TKey> _topNKeys;
        private List<TValue> _topNValues;

        public SortedTopN(int count, IComparer<TKey> comparer)
        {
            if (count < 1)
            {
                throw new ArgumentOutOfRangeException("count");
            }
            if (comparer == null)
            {
                throw new ArgumentNullException("comparer");
            }
            this._n = count;
            this._topNKeys = new List<TKey>(count);
            this._topNValues = new List<TValue>(count);
            this._comparer = comparer;
        }

        public bool Add(KeyValuePair<TKey, TValue> item)
        {
            return this.Add(item.Key, item.Value);
        }

        public bool Add(TKey key, TValue value)
        {
            int index = this._topNKeys.BinarySearch(key, this._comparer);
            if (index < 0)
            {
                index = ~index;
            }
            if ((this._topNKeys.Count >= this._n) && (index == 0))
            {
                return false;
            }
            if (this._topNKeys.Count == this._n)
            {
                this._topNKeys.RemoveAt(0);
                this._topNValues.RemoveAt(0);
                index--;
            }
            if (index < this._n)
            {
                this._topNKeys.Insert(index, key);
                this._topNValues.Insert(index, value);
            }
            else
            {
                this._topNKeys.Add(key);
                this._topNValues.Add(value);
            }
            return true;
        }

        public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator()
        {
            int iteratorVariable0 = this._topNKeys.Count - 1;
            while (true)
            {
                if (iteratorVariable0 < 0)
                {
                    yield break;
                }
                yield return new KeyValuePair<TKey, TValue>(this._topNKeys[iteratorVariable0], this._topNValues[iteratorVariable0]);
                iteratorVariable0--;
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
        }

        public IEnumerable<TValue> Values
        {
            get
            {
                int iteratorVariable0 = this._topNKeys.Count - 1;
                while (true)
                {
                    if (iteratorVariable0 < 0)
                    {
                        yield break;
                    }
                    yield return this._topNValues[iteratorVariable0];
                    iteratorVariable0--;
                }
            }
        }

       
    }
}

