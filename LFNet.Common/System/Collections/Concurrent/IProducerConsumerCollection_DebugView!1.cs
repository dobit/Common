using System.Collections.Concurrent;
using System.Diagnostics;

namespace System.Collections.Concurrent
{
    /// <summary>Debug view for the IProducerConsumerCollection.</summary>
    /// <typeparam name="T">Specifies the type of the data being aggregated.</typeparam>
    internal sealed class IProducerConsumerCollection_DebugView<T>
    {
        private IProducerConsumerCollection<T> _collection;

        public IProducerConsumerCollection_DebugView(IProducerConsumerCollection<T> collection)
        {
            this._collection = collection;
        }

        [DebuggerBrowsable(DebuggerBrowsableState.RootHidden)]
        public T[] Values
        {
            get
            {
                return this._collection.ToArray();
            }
        }
    }
}

