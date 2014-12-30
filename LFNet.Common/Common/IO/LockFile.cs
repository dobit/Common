using System;

namespace LFNet.Common.IO
{
    public sealed class LockFile : LockBase<LockFile>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="T:LFNet.Common.IO.LockFile" /> class.
        /// </summary>
        /// <param name="fileName">The file.</param>
        public LockFile(string fileName) : base(fileName)
        {
            this.FileName = fileName;
            LockBase<LockFile>.DefaultTimeOutInSeconds = 30.0;
        }

        /// <summary>
        /// Acquires a lock in a specific amount of time.
        /// </summary>
        /// <param name="timeout">The time to wait for when trying to acquire a lock.</param>
        public override void AcquireLock(TimeSpan timeout)
        {
            this.CreateLock(this.FileName, timeout);
        }

        /// <summary>
        /// Releases the lock.
        /// </summary>
        /// <param name="forceRemove">If set to true, the lock will be removed forcefully.</param>
        public override void ReleaseLock(bool forceRemove)
        {
            this.RemoveLock(this.FileName);
        }

        /// <summary>
        /// The file that is being locked.
        /// </summary>
        public string FileName { get; private set; }
    }
}

