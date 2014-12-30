namespace LFNet.Common.Threading
{
    /// <summary>A debugger view of the Lazy&lt;T&gt; to surface additional debugging properties and 
    /// to ensure that the Lazy&lt;T&gt; does not become initialized if it was not already.</summary>
    internal sealed class System_LazyDebugView<T>
    {
        private readonly Lazy<T> m_lazy;

        /// <summary>Constructs a new debugger view object for the provided Lazy object.</summary> 
        /// <param name="lazy">A Lazy object to browse in the debugger.</param>
        public System_LazyDebugView(Lazy<T> lazy)
        {
            this.m_lazy = lazy;
        }

        /// <summary>Returns whether the Lazy object is initialized or not.</summary>
        public bool IsValueCreated
        {
            get
            {
                return this.m_lazy.IsValueCreated;
            }
        }

        /// <summary>Returns the execution mode of the Lazy object</summary>
        public bool IsValueFaulted
        {
            get
            {
                return this.m_lazy.IsValueFaulted;
            }
        }

        /// <summary>Returns the execution mode of the Lazy object</summary>
        public LazyThreadSafetyMode Mode
        {
            get
            {
                return this.m_lazy.Mode;
            }
        }

        /// <summary>Returns the value of the Lazy object.</summary>
        public T Value
        {
            get
            {
                return this.m_lazy.ValueForDebugDisplay;
            }
        }
    }
}

