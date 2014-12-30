using System.Collections.Generic;
using System.ComponentModel;

namespace LFNet.Common.Helpers
{
    public class PropertyComparer<T> : PropertyComparer, IComparer<T>, IEqualityComparer<T>
    {
        public PropertyComparer(string orderByClause) : base(orderByClause)
        {
        }

        public PropertyComparer(SortExpression[] sortExpressions) : base(sortExpressions)
        {
        }

        public PropertyComparer(string propertyName, ListSortDirection sortDirection) : base(propertyName, sortDirection)
        {
        }

        public int Compare(T x, T y)
        {
            return base.Compare(x, y);
        }

        public bool Equals(T x, T y)
        {
            return base.Equals(x, y);
        }

        public int GetHashCode(T obj)
        {
            return base.GetHashCode(obj);
        }
    }
}

