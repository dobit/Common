using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace System.Threading
{
    /// <summary>Debugger type proxy for AsyncCache.</summary>
    /// <typeparam name="TKey">Specifies the type of the cache's keys.</typeparam>
    /// <typeparam name="TValue">Specifies the type of the cache's values.</typeparam>
    internal class AsyncCache_DebugView<TKey, TValue>
    {
        private readonly AsyncCache<TKey, TValue> _asyncCache;

        internal AsyncCache_DebugView(AsyncCache<TKey, TValue> asyncCache)
        {
            this._asyncCache = asyncCache;
        }

        [DebuggerBrowsable(DebuggerBrowsableState.RootHidden)]
        internal KeyValuePair<TKey, Task<TValue>>[] Values
        {
            get
            {
                return this._asyncCache.ToArray<KeyValuePair<TKey, Task<TValue>>>();
            }
        }
    }
}

