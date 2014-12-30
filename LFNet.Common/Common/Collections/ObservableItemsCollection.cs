using System;
using System.ComponentModel;

namespace LFNet.Common.Collections
{
    public class ObservableItemsCollection<T> : global::System.Collections.ObjectModel.ObservableCollection<T> where T: class, INotifyPropertyChanged
    {
        public event EventHandler<NotifyCollectionItemChangeEventArgs> ItemChanged;

        protected override void ClearItems()
        {
            foreach (T local in base.Items)
            {
                local.PropertyChanged -= new PropertyChangedEventHandler(this.ItemPropertyChanged);
            }
            base.ClearItems();
        }

        protected override void InsertItem(int index, T item)
        {
            base.InsertItem(index, item);
            item.PropertyChanged += new PropertyChangedEventHandler(this.ItemPropertyChanged);
        }

        private void ItemPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            T item = sender as T;
            if (base.IndexOf(item) >= 0)
            {
                this.OnItemChanged(base.IndexOf(item), e.PropertyName);
            }
        }

        private void OnItemChanged(int index, string propertyName)
        {
            if (this.ItemChanged != null)
            {
                this.ItemChanged(this, new NotifyCollectionItemChangeEventArgs(index, propertyName));
            }
        }

        protected override void RemoveItem(int index)
        {
            base.Items[index].PropertyChanged -= new PropertyChangedEventHandler(this.ItemPropertyChanged);
            base.RemoveItem(index);
        }

        protected override void SetItem(int index, T item)
        {
            base.Items[index].PropertyChanged -= new PropertyChangedEventHandler(this.ItemPropertyChanged);
            base.SetItem(index, item);
            base.Items[index].PropertyChanged += new PropertyChangedEventHandler(this.ItemPropertyChanged);
        }
    }
}

