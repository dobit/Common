using System;
using System.IO;
using System.Threading;
using LFNet.Common.Component;
using LFNet.Common.Extensions;

namespace LFNet.Common.IO
{
	public abstract class LockBase<T> : DisposableBase where T : LockBase<T>
	{
		/// <summary>
		/// The default time to wait when trying to acquire a lock.
		/// </summary>
		protected static double DefaultTimeOutInSeconds
		{
			get;
			set;
		}
		/// <summary>
		/// Ensures that the derived classes always have a string parameter to pass in a path.
		/// </summary>
		/// <param name="path"></param>
		protected LockBase(string path)
		{
		}
		/// <summary>
		/// Acquires a lock while waiting with the default timeout value.
		/// </summary>
		public virtual void AcquireLock()
		{
			this.AcquireLock(TimeSpan.FromSeconds(LockBase<T>.DefaultTimeOutInSeconds));
		}
		/// <summary>
		/// Acquires a lock in a specific amount of time.
		/// </summary>
		/// <param name="timeout">The time to wait for when trying to acquire a lock.</param>
		public abstract void AcquireLock(TimeSpan timeout);
		/// <summary>
		/// Acquires a lock while waiting with the default timeout value.
		/// </summary>
		/// <param name="path">The path to acquire a lock on.</param>
		/// <returns>A lock instance.</returns>
		public static T Acquire(string path)
		{
			return LockBase<T>.Acquire(path, TimeSpan.FromSeconds(LockBase<T>.DefaultTimeOutInSeconds));
		}
		/// <summary>
		/// Acquires a lock in a specific amount of time.
		/// </summary>
		/// <param name="path">The path to acquire a lock on.</param>
		/// <param name="timeout">The time to wait for when trying to acquire a lock.</param>
		/// <returns>A lock instance.</returns>
		public static T Acquire(string path, TimeSpan timeout)
		{
			T t = Activator.CreateInstance(typeof(T), new object[]
			{
				path
			}) as T;
			if (t == null)
			{
				throw new Exception("Unable to locking instance.");
			}
			t.AcquireLock(timeout);
			return t;
		}
		/// <summary>
		/// Creates a lock file.
		/// </summary>
		/// <param name="path">The place to create the lock file.</param>
		/// <param name="timeout">The amount of time to wait before a TimeoutException is thrown.</param>
		protected virtual void CreateLock(string path, TimeSpan timeout)
		{
			DateTime t = DateTime.UtcNow.Add(timeout);
			while (true)
			{
				if (!File.Exists(path))
				{
					try
					{
						using (FileStream fileStream = new FileStream(path, FileMode.CreateNew, FileAccess.Write, FileShare.None))
						{
							fileStream.Close();
						}
					}
					catch (IOException)
					{
						continue;
					}
					return;
				}
				if (t < DateTime.UtcNow)
				{
					break;
				}
				Thread.Sleep(500);
			}
			string message = string.Format("[Thread: {0}] The lock '{1}' timed out.", Thread.CurrentThread.ManagedThreadId, path);
			throw new TimeoutException(message);
		}
		/// <summary>
		/// Releases the lock.
		/// </summary>
		public virtual void ReleaseLock()
		{
			this.ReleaseLock(false);
		}
		/// <summary>
		/// Releases the lock.
		/// </summary>
		/// <param name="forceRemove">If set to true, the lock will be removed forcefully.</param>
		public abstract void ReleaseLock(bool forceRemove);
		/// <summary>
		/// Releases the lock.
		/// </summary>
		/// <param name="path">The path to the lock file.</param>
		protected virtual void RemoveLock(string path)
		{
			new Action(delegate
			{
				try
				{
					if (File.Exists(path))
					{
						File.Delete(path);
					}
				}
				catch (IOException)
				{
					throw;
				}
			}).Retry(5, 100);
		}
		/// <summary>
		/// Releases the lock.
		/// </summary>
		protected override void DisposeUnmanagedResources()
		{
			this.ReleaseLock();
		}
	}
}
