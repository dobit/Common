using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Runtime.Serialization;
using System.Security.Permissions;
using System.Threading;

namespace LFNet.Common.Threading
{
	/// <summary>
	/// Provides support for lazy initialization. 
	/// </summary>
	/// <typeparam name="T">Specifies the type of element being laziliy initialized.</typeparam> 
	/// <remarks> 
	/// <para>
	/// By default, all public and protected members of <see cref="T:LFNet.Common.Threading.Lazy`1" /> are thread-safe and may be used 
	/// concurrently from multiple threads.  These thread-safety guarantees may be removed optionally and per instance
	/// using parameters to the type's constructors.
	/// </para>
	/// </remarks> 
	[ComVisible(false)]
	[HostProtection(SecurityAction.LinkDemand, Synchronization = true, ExternalThreading = true)]
	[Serializable]
	public class Lazy<T>
	{
		/// <summary>
		/// wrapper class to box the initialized value, this is mainly created to avoid boxing/unboxing the value each time the value is called in case T is 
		/// a value type 
		/// </summary>
		[Serializable]
		private class Boxed
		{
			internal T m_value;
			internal Boxed(T value)
			{
				this.m_value = value;
			}
		}
		/// <summary>
		/// Wrapper class to wrap the excpetion thrown by the value factory
		/// </summary> 
		private class LazyInternalExceptionHolder
		{
			internal Exception m_exception;
			internal LazyInternalExceptionHolder(Exception ex)
			{
				this.m_exception = ex;
			}
		}
		private static Func<T> PUBLICATION_ONLY_OR_ALREADY_INITIALIZED = () => default(T);
		private volatile object m_boxed;
		[NonSerialized]
		private Func<T> m_valueFactory;
		[NonSerialized]
		private readonly object m_threadSafeObj;
		/// <summary>Gets the value of the Lazy&lt;T&gt; for debugging display purposes.</summary>
		internal T ValueForDebugDisplay
		{
			get
			{
				if (!this.IsValueCreated)
				{
					return default(T);
				}
				return ((Lazy<T>.Boxed)this.m_boxed).m_value;
			}
		}
		/// <summary>
		/// Gets a value indicating whether this instance may be used concurrently from multiple threads. 
		/// </summary>
		internal LazyThreadSafetyMode Mode
		{
			get
			{
				if (this.m_threadSafeObj == null)
				{
					return LazyThreadSafetyMode.None;
				}
				if (this.m_threadSafeObj == Lazy<T>.PUBLICATION_ONLY_OR_ALREADY_INITIALIZED)
				{
					return LazyThreadSafetyMode.PublicationOnly;
				}
				return LazyThreadSafetyMode.ExecutionAndPublication;
			}
		}
		/// <summary> 
		/// Gets whether the value creation is faulted or not
		/// </summary> 
		internal bool IsValueFaulted
		{
			get
			{
				return this.m_boxed is Lazy<T>.LazyInternalExceptionHolder;
			}
		}
		/// <summary>Gets a value indicating whether the <see cref="T:System.Threading.Lazy{T}" /> has been initialized. 
		/// </summary> 
		/// <value>true if the <see cref="T:System.Threading.Lazy{T}" /> instance has been initialized;
		/// otherwise, false.</value> 
		/// <remarks>
		/// The initialization of a <see cref="T:System.Threading.Lazy{T}" /> instance may result in either
		/// a value being produced or an exception being thrown.  If an exception goes unhandled during initialization,
		/// the <see cref="T:System.Threading.Lazy{T}" /> instance is still considered initialized, and that exception 
		/// will be thrown on subsequent accesses to <see cref="P:CodeSmith.Core.Threading.Lazy`1.Value" />.  In such cases, <see cref="P:CodeSmith.Core.Threading.Lazy`1.IsValueCreated" />
		/// will return true. 
		/// </remarks> 
		public bool IsValueCreated
		{
			get
			{
				return this.m_boxed != null && this.m_boxed is Lazy<T>.Boxed;
			}
		}
		/// <summary>Gets the lazily initialized value of the current <see cref="T:System.Threading.Lazy{T}" />.</summary>
		/// <value>The lazily initialized value of the current <see cref="T:System.Threading.Lazy{T}" />.</value> 
		/// <exception cref="T:System.MissingMemberException">
		/// The <see cref="T:System.Threading.Lazy{T}" /> was initialized to use the default constructor 
		/// of the type being lazily initialized, and that type does not have a public, parameterless constructor. 
		/// </exception>
		/// <exception cref="T:System.MemberAccessException"> 
		/// The <see cref="T:System.Threading.Lazy{T}" /> was initialized to use the default constructor
		/// of the type being lazily initialized, and permissions to access the constructor were missing.
		/// </exception>
		/// <exception cref="T:System.InvalidOperationException"> 
		/// The <see cref="T:System.Threading.Lazy{T}" /> was constructed with the <see cref="T:System.Threading.LazyThreadSafetyMode.ExecutionAndPublication" /> or
		/// <see cref="T:System.Threading.LazyThreadSafetyMode.None" />  and the initialization function attempted to access <see cref="P:CodeSmith.Core.Threading.Lazy`1.Value" /> on this instance. 
		/// </exception> 
		/// <remarks>
		/// If <see cref="P:CodeSmith.Core.Threading.Lazy`1.IsValueCreated" /> is false, accessing <see cref="P:CodeSmith.Core.Threading.Lazy`1.Value" /> will force initialization. 
		/// Please <see cref="T:System.Threading.LazyThreadSafetyMode" /> for more information on how <see cref="T:System.Threading.Lazy{T}" /> will behave if an exception is thrown
		/// from initialization delegate.
		/// </remarks>
		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		public T Value
		{
			get
			{
				if (this.m_boxed == null)
				{
					return this.LazyInitValue();
				}
				Lazy<T>.Boxed boxed = this.m_boxed as Lazy<T>.Boxed;
				if (boxed != null)
				{
					return boxed.m_value;
				}
				Lazy<T>.LazyInternalExceptionHolder lazyInternalExceptionHolder = this.m_boxed as Lazy<T>.LazyInternalExceptionHolder;
				throw lazyInternalExceptionHolder.m_exception;
			}
		}
		/// <summary>
		/// Initializes a new instance of the <see cref="T:System.Threading.Lazy{T}" /> class that
		/// uses <typeparamref name="T" />'s default constructor for lazy initialization.
		/// </summary> 
		/// <remarks>
		/// An instance created with this constructor may be used concurrently from multiple threads. 
		/// </remarks> 
		public Lazy() : this(LazyThreadSafetyMode.ExecutionAndPublication)
		{
		}
		/// <summary> 
		/// Initializes a new instance of the <see cref="T:System.Threading.Lazy{T}" /> class that uses a
		/// specified initialization function. 
		/// </summary> 
		/// <param name="valueFactory">
		/// The <see cref="T:System.Func{T}" /> invoked to produce the lazily-initialized value when it is 
		/// needed.
		/// </param>
		/// <exception cref="T:System.ArgumentNullException"><paramref name="valueFactory" /> is a null
		/// reference (Nothing in Visual Basic).</exception> 
		/// <remarks>
		/// An instance created with this constructor may be used concurrently from multiple threads. 
		/// </remarks> 
		public Lazy(Func<T> valueFactory) : this(valueFactory, LazyThreadSafetyMode.ExecutionAndPublication)
		{
		}
		/// <summary> 
		/// Initializes a new instance of the <see cref="T:System.Threading.Lazy{T}" />
		/// class that uses <typeparamref name="T" />'s default constructor and a specified thread-safety mode. 
		/// </summary> 
		/// <param name="isThreadSafe">true if this instance should be usable by multiple threads concurrently; false if the instance will only be used by one thread at a time.
		/// </param> 
		public Lazy(bool isThreadSafe) : this(isThreadSafe ? LazyThreadSafetyMode.ExecutionAndPublication : LazyThreadSafetyMode.None)
		{
		}
		/// <summary> 
		/// Initializes a new instance of the <see cref="T:System.Threading.Lazy{T}" /> 
		/// class that uses <typeparamref name="T" />'s default constructor and a specified thread-safety mode.
		/// </summary> 
		/// <param name="mode">The lazy thread-safety mode mode</param>
		/// <exception cref="T:System.ArgumentOutOfRangeException"><paramref name="mode" /> mode contains an invalid valuee</exception>
		public Lazy(LazyThreadSafetyMode mode)
		{
			this.m_threadSafeObj = Lazy<T>.GetObjectFromMode(mode);
		}
		/// <summary> 
		/// Initializes a new instance of the <see cref="T:System.Threading.Lazy{T}" /> class
		/// that uses a specified initialization function and a specified thread-safety mode.
		/// </summary>
		/// <param name="valueFactory"> 
		/// The <see cref="T:System.Func{T}" /> invoked to produce the lazily-initialized value when it is needed.
		/// </param> 
		/// <param name="isThreadSafe">true if this instance should be usable by multiple threads concurrently; false if the instance will only be used by one thread at a time. 
		/// </param>
		/// <exception cref="T:System.ArgumentNullException"><paramref name="valueFactory" /> is 
		/// a null reference (Nothing in Visual Basic).</exception>
		public Lazy(Func<T> valueFactory, bool isThreadSafe) : this(valueFactory, isThreadSafe ? LazyThreadSafetyMode.ExecutionAndPublication : LazyThreadSafetyMode.None)
		{
		}
		/// <summary> 
		/// Initializes a new instance of the <see cref="T:System.Threading.Lazy{T}" /> class
		/// that uses a specified initialization function and a specified thread-safety mode. 
		/// </summary>
		/// <param name="valueFactory">
		/// The <see cref="T:System.Func{T}" /> invoked to produce the lazily-initialized value when it is needed.
		/// </param> 
		/// <param name="mode">The lazy thread-safety mode.</param>
		/// <exception cref="T:System.ArgumentNullException"><paramref name="valueFactory" /> is 
		/// a null reference (Nothing in Visual Basic).</exception> 
		/// <exception cref="T:System.ArgumentOutOfRangeException"><paramref name="mode" /> mode contains an invalid value.</exception>
		public Lazy(Func<T> valueFactory, LazyThreadSafetyMode mode)
		{
			if (valueFactory == null)
			{
				throw new ArgumentNullException("valueFactory");
			}
			this.m_threadSafeObj = Lazy<T>.GetObjectFromMode(mode);
			this.m_valueFactory = valueFactory;
		}
		/// <summary> 
		/// Static helper function that returns an object based on the given mode. it also throws an exception if the mode is invalid
		/// </summary>
		private static object GetObjectFromMode(LazyThreadSafetyMode mode)
		{
			if (mode == LazyThreadSafetyMode.ExecutionAndPublication)
			{
				return new object();
			}
			if (mode == LazyThreadSafetyMode.PublicationOnly)
			{
				return Lazy<T>.PUBLICATION_ONLY_OR_ALREADY_INITIALIZED;
			}
			if (mode != LazyThreadSafetyMode.None)
			{
				throw new ArgumentOutOfRangeException("mode", "The mode argument specifies an invalid value.");
			}
			return null;
		}
		/// <summary>Forces initialization during serialization.</summary> 
		/// <param name="context">The StreamingContext for the serialization operation.</param> 
		[OnSerializing]
		private void OnSerializing(StreamingContext context)
		{
			T arg_06_0 = this.Value;
		}
		/// <summary>Creates and returns a string representation of this instance.</summary> 
		/// <returns>The result of calling <see cref="M:System.Object.ToString" /> on the <see cref="P:LFNet.Common.Threading.Lazy`1.Value" />.</returns>
		/// <exception cref="T:System.NullReferenceException"> 
		/// The <see cref="P:LFNet.Common.Threading.Lazy`1.Value" /> is null.
		/// </exception>
		public override string ToString()
		{
			if (!this.IsValueCreated)
			{
				return "Value is not created.";
			}
			T value = this.Value;
			return value.ToString();
		}
		/// <summary>
		/// local helper method to initialize the value 
		/// </summary>
		/// <returns>The inititialized T value</returns>
		private T LazyInitValue()
		{
			Lazy<T>.Boxed boxed = null;
			LazyThreadSafetyMode mode = this.Mode;
			if (mode == LazyThreadSafetyMode.None)
			{
				boxed = this.CreateValue();
				this.m_boxed = boxed;
			}
			else
			{
				if (mode == LazyThreadSafetyMode.PublicationOnly)
				{
					boxed = this.CreateValue();
					if (Interlocked.CompareExchange(ref this.m_boxed, boxed, null) != null)
					{
						boxed = (Lazy<T>.Boxed)this.m_boxed;
					}
				}
				else
				{
					lock (this.m_threadSafeObj)
					{
						if (this.m_boxed == null)
						{
							boxed = this.CreateValue();
							this.m_boxed = boxed;
						}
						else
						{
							boxed = (this.m_boxed as Lazy<T>.Boxed);
							if (boxed == null)
							{
								Lazy<T>.LazyInternalExceptionHolder lazyInternalExceptionHolder = this.m_boxed as Lazy<T>.LazyInternalExceptionHolder;
								throw lazyInternalExceptionHolder.m_exception;
							}
						}
					}
				}
			}
			return boxed.m_value;
		}
		/// <summary>Creates an instance of T using m_valueFactory in case its not null or use reflection to create a new T()</summary> 
		/// <returns>An instance of Boxed.</returns>
		private Lazy<T>.Boxed CreateValue()
		{
			Lazy<T>.Boxed result = null;
			LazyThreadSafetyMode mode = this.Mode;
			if (this.m_valueFactory != null)
			{
				try
				{
					if (mode != LazyThreadSafetyMode.PublicationOnly && this.m_valueFactory == Lazy<T>.PUBLICATION_ONLY_OR_ALREADY_INITIALIZED)
					{
						throw new InvalidOperationException("ValueFactory attempted to access the Value property of this instance.");
					}
					Func<T> valueFactory = this.m_valueFactory;
					if (mode != LazyThreadSafetyMode.PublicationOnly)
					{
						this.m_valueFactory = Lazy<T>.PUBLICATION_ONLY_OR_ALREADY_INITIALIZED;
					}
					result = new Lazy<T>.Boxed(valueFactory());
					return result;
				}
				catch (Exception ex)
				{
					if (mode != LazyThreadSafetyMode.PublicationOnly)
					{
						this.m_boxed = new Lazy<T>.LazyInternalExceptionHolder(ex);
					}
					throw;
				}
			}
			try
			{
				result = new Lazy<T>.Boxed((T)((object)Activator.CreateInstance(typeof(T))));
			}
			catch (MissingMethodException)
			{
				Exception ex2 = new MissingMemberException("The lazily-initialized type does not have a public, parameterless constructor.");
				if (mode != LazyThreadSafetyMode.PublicationOnly)
				{
					this.m_boxed = new Lazy<T>.LazyInternalExceptionHolder(ex2);
				}
				throw ex2;
			}
			return result;
		}
	}
}
