using System;
using System.Collections;
using System.ComponentModel;

namespace LFNet.Common.Helpers
{
    public class PropertyComparer : IComparer, IEqualityComparer
    {
        private SortExpression[] _sortExpressions;

        public PropertyComparer(string orderByClause)
        {
            this._sortExpressions = new SortExpression[0];
            this.BuildSortExpressions(orderByClause);
        }

        public PropertyComparer(SortExpression[] sortExpressions)
        {
            this._sortExpressions = new SortExpression[0];
            this._sortExpressions = sortExpressions;
        }

        public PropertyComparer(string propertyName, ListSortDirection sortDirection)
        {
            this._sortExpressions = new SortExpression[0];
            this._sortExpressions = new SortExpression[] { new SortExpression(propertyName, sortDirection) };
        }

        private void BuildSortExpressions(string orderByClause)
        {
            string[] strArray = orderByClause.Split(new char[] { ',' });
            this._sortExpressions = new SortExpression[strArray.Length];
            for (int i = 0; i < strArray.Length; i++)
            {
                string[] strArray2 = strArray[i].Trim().Split(new char[] { ' ' });
                this._sortExpressions[i].PropertyName = strArray2[0].Trim();
                if ((strArray2.Length == 2) && ((strArray2[1].Trim().ToUpperInvariant() == "DESC") || (strArray2[1].Trim().ToUpperInvariant() == "DESCENDING")))
                {
                    this._sortExpressions[i].SortDirection = ListSortDirection.Descending;
                }
                else
                {
                    this._sortExpressions[i].SortDirection = ListSortDirection.Ascending;
                }
            }
        }

        public int Compare(object a, object b)
        {
            for (int i = 0; i < this.SortExpressions.Length; i++)
            {
                if (this.SortExpressions[i].PropertyDescriptor == null)
                {
                    this.SortExpressions[i].PropertyDescriptor = TypeDescriptor.GetProperties(a).Find(this.SortExpressions[i].PropertyName, true);
                }
                object propertyValue = this.SortExpressions[i].GetPropertyValue(a);
                object obj3 = this.SortExpressions[i].GetPropertyValue(b);
                if (!(propertyValue is IComparable))
                {
                    throw new ArgumentException("Property type must implement IComparable.");
                }
                int result = ((IComparable) propertyValue).CompareTo(obj3);
                if (result != 0)
                {
                    if (this.SortExpressions[i].SortDirection == ListSortDirection.Ascending)
                    {
                        return result;
                    }
                    return this.ReverseResult(result);
                }
            }
            return 0;
        }

        public bool Equals(object x, object y)
        {
            return (this.Compare(x, y) == 0);
        }

        public int GetHashCode(object obj)
        {
            int num = 0x1cd5;
            for (int i = 0; i < this.SortExpressions.Length; i++)
            {
                if (this.SortExpressions[i].PropertyDescriptor == null)
                {
                    this.SortExpressions[i].PropertyDescriptor = TypeDescriptor.GetProperties(obj).Find(this.SortExpressions[i].PropertyName, true);
                }
                object propertyValue = this.SortExpressions[i].GetPropertyValue(obj);
                num = ((num << 5) + num) ^ propertyValue.GetHashCode();
            }
            return num;
        }

        private int ReverseResult(int result)
        {
            switch (result)
            {
                case -1:
                    return 1;

                case 0:
                    return 0;

                case 1:
                    return -1;
            }
            throw new Exception("Invalid compare result.");
        }

        public SortExpression[] SortExpressions
        {
            get
            {
                return this._sortExpressions;
            }
            set
            {
                this._sortExpressions = value;
            }
        }
    }
}

