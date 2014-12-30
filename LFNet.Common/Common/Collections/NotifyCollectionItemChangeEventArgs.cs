using System.ComponentModel;

namespace LFNet.Common.Collections
{
    public class NotifyCollectionItemChangeEventArgs : PropertyChangedEventArgs
    {
        public NotifyCollectionItemChangeEventArgs(int index, string propertyName) : base(propertyName)
        {
            this.Index = index;
        }

        public int Index { get; set; }
    }
}

