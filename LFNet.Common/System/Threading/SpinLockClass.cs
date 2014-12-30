using System;
using System.Threading;

namespace System.Threading
{
    /// <summary>Provides a simple, reference type wrapper for SpinLock.</summary>
    public class SpinLockClass
    {
        private SpinLock _spinLock;

        /// <summary>Initializes an instance of the SpinLockClass class.</summary>
        public SpinLockClass()
        {
            this._spinLock = new SpinLock();
        }

        /// <summary>Initializes an instance of the SpinLockClass class.</summary>
        /// <param name="enableThreadOwnerTracking">
        /// Controls whether the SpinLockClass should track
        /// thread-ownership fo the lock.
        /// </param>
        public SpinLockClass(bool enableThreadOwnerTracking)
        {
            this._spinLock = new SpinLock(enableThreadOwnerTracking);
        }

        /// <summary>Enters the lock.</summary>
        /// <param name="lockTaken">
        /// Upon exit of the Enter method, specifies whether the lock was acquired. 
        /// The variable passed by reference must be initialized to false.
        /// </param>
        public void Enter(ref bool lockTaken)
        {
            this._spinLock.Enter(ref lockTaken);
        }

        /// <summary>Runs the specified delegate under the lock.</summary>
        /// <param name="runUnderLock">The delegate to be executed while holding the lock.</param>
        public void Execute(Action runUnderLock)
        {
            bool lockTaken = false;
            try
            {
                this.Enter(ref lockTaken);
                runUnderLock();
            }
            finally
            {
                if (lockTaken)
                {
                    this.Exit();
                }
            }
        }

        /// <summary>Exits the SpinLock.</summary>
        public void Exit()
        {
            this._spinLock.Exit();
        }

        /// <summary>Exits the SpinLock.</summary>
        /// <param name="useMemoryBarrier">
        /// A Boolean value that indicates whether a memory fence should be issued in
        /// order to immediately publish the exit operation to other threads.
        /// </param>
        public void Exit(bool useMemoryBarrier)
        {
            this._spinLock.Exit(useMemoryBarrier);
        }
    }
}

