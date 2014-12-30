using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;

namespace System.Threading
{
    /// <summary>Provides a reduction variable for aggregating data across multiple threads involved in a computation.</summary>
    /// <typeparam name="T">Specifies the type of the data being aggregated.</typeparam>
    [DebuggerTypeProxy(typeof(ReductionVariable_DebugView<>)), DebuggerDisplay("Count={_values.Count}")]
    public sealed class ReductionVariable<T>
    {
        /// <summary>The factory used to initialize a value on a thread.</summary>
        private readonly Func<T> _seedFactory;
        /// <summary>Thread-local storage for each thread's value.</summary>
        private readonly ThreadLocal<StrongBox<T>> _threadLocal;
        /// <summary>The list of all thread-local values for later enumeration.</summary>
        private readonly ConcurrentQueue<StrongBox<T>> _values;

        /// <summary>Initializes the instances.</summary>
        public ReductionVariable()
        {
            this._values = new ConcurrentQueue<StrongBox<T>>();
            this._threadLocal = new ThreadLocal<StrongBox<T>>(new Func<StrongBox<T>>(this.CreateValue));
        }

        /// <summary>Initializes the instances.</summary>
        /// <param name="seedFactory">
        /// The function invoked to provide the initial value for a thread.  
        /// If null, the default value of T will be used as the seed.
        /// </param>
        public ReductionVariable(Func<T> seedFactory) : this()
        {
            this._seedFactory = seedFactory;
        }

        /// <summary>Creates a value for the current thread and stores it in the central list of values.</summary>
        /// <returns>The boxed value.</returns>
        private StrongBox<T> CreateValue()
        {
            StrongBox<T> item = new StrongBox<T>((this._seedFactory != null) ? this._seedFactory() : default(T));
            this._values.Enqueue(item);
            return item;
        }

        /// <summary>Applies an accumulator function over the values in this variable.</summary>
        /// <param name="function">An accumulator function to be invoked on each value.</param>
        /// <returns>The accumulated value.</returns>
        public T Reduce(Func<T, T, T> function)
        {
            return this.Values.Aggregate<T>(function);
        }

        /// <summary>
        /// Applies an accumulator function over the values in this variable.
        /// The specified seed is used as the initial accumulator value.
        /// </summary>
        /// <param name="seed"></param>
        /// <param name="function">An accumulator function to be invoked on each value.</param>
        /// <returns>The accumulated value.</returns>
        public TAccumulate Reduce<TAccumulate>(TAccumulate seed, Func<TAccumulate, T, TAccumulate> function)
        {
            return this.Values.Aggregate<T, TAccumulate>(seed, function);
        }

        /// <summary>Gets or sets the value for the current thread.</summary>
        public T Value
        {
            get
            {
                return this._threadLocal.Value.Value;
            }
            set
            {
                this._threadLocal.Value.Value = value;
            }
        }

        /// <summary>Gets the values for all of the threads that have used this instance.</summary>
        public IEnumerable<T> Values
        {
            get
            {
                return (from s in this._values select s.Value);
            }
        }
    }
}

