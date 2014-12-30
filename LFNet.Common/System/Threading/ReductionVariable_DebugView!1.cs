using System.Diagnostics;
using System.Linq;

namespace System.Threading
{
    /// <summary>Debug view for the reductino variable</summary>
    /// <typeparam name="T">Specifies the type of the data being aggregated.</typeparam>
    internal sealed class ReductionVariable_DebugView<T>
    {
        private ReductionVariable<T> _variable;

        public ReductionVariable_DebugView(ReductionVariable<T> variable)
        {
            this._variable = variable;
        }

        [DebuggerBrowsable(DebuggerBrowsableState.RootHidden)]
        public T[] Values
        {
            get
            {
                return this._variable.Values.ToArray<T>();
            }
        }
    }
}

