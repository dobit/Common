using System.Collections;
using System.ComponentModel;
using System.Runtime.InteropServices;

namespace LFNet.Common.Helpers
{
    [StructLayout(LayoutKind.Sequential)]
    public struct SortExpression
    {
        public global::System.ComponentModel.PropertyDescriptor PropertyDescriptor;
        public string PropertyName;
        public ListSortDirection SortDirection;
        private Hashtable _propertyValueCache;
        public SortExpression(string propertyName)
        {
            this.PropertyName = propertyName;
            this.SortDirection = ListSortDirection.Ascending;
            this.PropertyDescriptor = null;
            this._propertyValueCache = new Hashtable();
        }

        public SortExpression(string propertyName, ListSortDirection sortDirection)
        {
            this.PropertyName = propertyName;
            this.SortDirection = sortDirection;
            this.PropertyDescriptor = null;
            this._propertyValueCache = new Hashtable();
        }

        public object GetPropertyValue(object obj)
        {
            if (this.PropertyDescriptor == null)
            {
                return null;
            }
            if (this._propertyValueCache == null)
            {
                this._propertyValueCache = new Hashtable(100);
            }
            if (this._propertyValueCache.Contains(obj))
            {
                return this._propertyValueCache[obj];
            }
            object obj2 = this.PropertyDescriptor.GetValue(obj);
            this._propertyValueCache.Add(obj, obj2);
            return obj2;
        }
    }
}

