using System;
using System.Diagnostics;

namespace LFNet.Common.Component
{
    /// <summary>
    /// A class representing a singleton pattern.
    /// </summary>
    /// <typeparam name="T">The type of the singleton</typeparam>
    public abstract class SingletonBase<T> where T: class
    {
        private static readonly Lazy<T> _instance;

        static SingletonBase()
        {
            SingletonBase<T>._instance = new Lazy<T>(() => (T) Activator.CreateInstance(typeof(T), true));
        }

        /// <summary>
        /// Initializes a new instance of the class.
        /// </summary>
        protected SingletonBase()
        {
        }

        /// <summary>
        /// Gets the current instance of the singleton.
        /// </summary>
        [DebuggerBrowsable(DebuggerBrowsableState.Never), DebuggerNonUserCode]
        public static T Current
        {
            get
            {
                return SingletonBase<T>._instance.Value;
            }
        }
    }
}

