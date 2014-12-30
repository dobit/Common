using System.Collections.Concurrent;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Diagnostics;
using System.Threading;

namespace System.Collections.Concurrent
{
    /// <summary>
    /// Provides a thread-safe, concurrent collection for use with data binding.
    /// </summary>
    /// <typeparam name="T">Specifies the type of the elements in this collection.</typeparam>
    [DebuggerDisplay("Count={Count}"), DebuggerTypeProxy(typeof(IProducerConsumerCollection_DebugView<>))]
    public class ObservableConcurrentCollection<T> : ProducerConsumerCollectionBase<T>, INotifyCollectionChanged, INotifyPropertyChanged
    {
        private readonly SynchronizationContext _context;

        /// <summary>Event raised when the collection changes.</summary>
        public event NotifyCollectionChangedEventHandler CollectionChanged;

        /// <summary>Event raised when a property on the collection changes.</summary>
        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// Initializes an instance of the ObservableConcurrentCollection class with an underlying
        /// queue data structure.
        /// </summary>
        public ObservableConcurrentCollection() : this(new ConcurrentQueue<T>())
        {
        }

        /// <summary>
        /// Initializes an instance of the ObservableConcurrentCollection class with the specified
        /// collection as the underlying data structure.
        /// </summary>
        public ObservableConcurrentCollection(IProducerConsumerCollection<T> collection) : base(collection)
        {
            this._context = AsyncOperationManager.SynchronizationContext;
        }

        /// <summary>
        /// Notifies observers of CollectionChanged or PropertyChanged of an update to the dictionary.
        /// </summary>
        private void NotifyObserversOfChange()
        {
            SendOrPostCallback d = null;
            NotifyCollectionChangedEventHandler collectionHandler = this.CollectionChanged;
            PropertyChangedEventHandler propertyHandler = this.PropertyChanged;
            if ((collectionHandler != null) || (propertyHandler != null))
            {
                if (d == null)
                {
                    d = delegate (object s) {
                        if (collectionHandler != null)
                        {
                            collectionHandler((ObservableConcurrentCollection<T>) this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
                        }
                        if (propertyHandler != null)
                        {
                            propertyHandler((ObservableConcurrentCollection<T>) this, new PropertyChangedEventArgs("Count"));
                        }
                    };
                }
                this._context.Post(d, null);
            }
        }

        protected override bool TryAdd(T item)
        {
            bool flag = base.TryAdd(item);
            if (flag)
            {
                this.NotifyObserversOfChange();
            }
            return flag;
        }

        protected override bool TryTake(out T item)
        {
            bool flag = base.TryTake(out item);
            if (flag)
            {
                this.NotifyObserversOfChange();
            }
            return flag;
        }
    }
}

