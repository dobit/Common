using System;
using System.Collections.Generic;

namespace LFNet.Common.Helpers
{
	public class LambdaComparer<T> : IEqualityComparer<T>, IComparer<T>
	{
		private readonly Func<T, T, int> _compareValuesFunc;
		private readonly Func<T, int> _getHashCodeFunc;
		public LambdaComparer(Func<T, int> getComparisonValueFunc) : this((T a, T b) => getComparisonValueFunc(a).CompareTo(getComparisonValueFunc(b)), (T o) => getComparisonValueFunc(o).GetHashCode())
		{
		}
		public LambdaComparer(Func<T, long> getComparisonValueFunc) : this((T a, T b) => getComparisonValueFunc(a).CompareTo(getComparisonValueFunc(b)), (T o) => getComparisonValueFunc(o).GetHashCode())
		{
		}
		public LambdaComparer(Func<T, string> getComparisonValue) : this((T a, T b) => string.CompareOrdinal(getComparisonValue(a), getComparisonValue(b)), (T o) => getComparisonValue(o).GetHashCode())
		{
		}
		public LambdaComparer(Func<T, T, int> compareValuesFunc) : this(compareValuesFunc, (T o) => o.GetHashCode())
		{
		}
		public LambdaComparer(Func<T, T, int> compareValuesFunc, Func<T, int> getHashCodeFunc)
		{
			if (compareValuesFunc == null)
			{
				throw new ArgumentNullException("compareValuesFunc");
			}
			if (getHashCodeFunc == null)
			{
				throw new ArgumentNullException("getHashCodeFunc");
			}
			this._compareValuesFunc = compareValuesFunc;
			this._getHashCodeFunc = getHashCodeFunc;
		}
		public bool Equals(T x, T y)
		{
			return this.Compare(x, y) == 0;
		}
		public int GetHashCode(T obj)
		{
			return this._getHashCodeFunc(obj);
		}
		public int Compare(T x, T y)
		{
			return this._compareValuesFunc(x, y);
		}
	}
}
