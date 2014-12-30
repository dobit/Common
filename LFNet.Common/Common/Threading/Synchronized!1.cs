namespace LFNet.Common.Threading
{
    /// <summary>
    /// Synchronized access wrapper class
    /// </summary>
    /// <typeparam name="T">The type that has its access synchronized.</typeparam>
    public class Synchronized<T>
    {
        private T _value;
        private readonly object _valueLock;

        /// <summary>
        /// Initializes a new instance of the <see cref="T:LFNet.Common.Threading.Synchronized`1" /> class.
        /// </summary>
        public Synchronized() : this(default(T), new object())
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="T:LFNet.Common.Threading.Synchronized`1" /> class.
        /// </summary>
        /// <param name="value">The initial value.</param>
        public Synchronized(T value) : this(value, new object())
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="T:LFNet.Common.Threading.Synchronized`1" /> class.
        /// </summary>
        /// <param name="value">The initial value.</param>
        /// <param name="Lock">The shared lock.</param>
        public Synchronized(T value, object Lock)
        {
            this._valueLock = Lock ?? new object();
            this.Value = value;
        }

        /// <summary>
        /// Performs an implicit conversion from <see cref="T:LFNet.Common.Threading.Synchronized`1" /> to {T}.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <returns>The result of the conversion.</returns>
        public static implicit operator T(Synchronized<T> value)
        {
            return value.Value;
        }

        /// <summary>
        /// Gets or sets the value.
        /// </summary>
        /// <value>The value.</value>
        public T Value
        {
            get
            {
                lock (this._valueLock)
                {
                    return this._value;
                }
            }
            set
            {
                lock (this._valueLock)
                {
                    this._value = value;
                }
            }
        }
    }
}

