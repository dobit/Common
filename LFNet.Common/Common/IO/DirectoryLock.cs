using System;
using System.IO;

namespace LFNet.Common.IO
{
    public sealed class DirectoryLock : LockBase<DirectoryLock>
    {
        /// <summary>
        /// The name of the lock file.
        /// </summary>
        private const string LockFileName = "dir.lock";

        public DirectoryLock(string directory) : base(directory)
        {
            LockBase<DirectoryLock>.DefaultTimeOutInSeconds = 5.0;
            this.Directory = Path.GetDirectoryName(Path.GetFullPath(directory).ToLowerInvariant());
            if ((this.Directory != null) && !global::System.IO.Directory.Exists(this.Directory))
            {
                throw new ArgumentException(string.Format("Directory '{0}' does not exist", this.Directory), "directory");
            }
        }

        /// <summary>
        /// Acquires a lock in a specific amount of time.
        /// </summary>
        /// <param name="timeout">The time to wait for when trying to acquire a lock.</param>
        public override void AcquireLock(TimeSpan timeout)
        {
            string path = Path.Combine(this.Directory, "dir.lock");
            this.CreateLock(path, timeout);
        }

        public static int ForceReleaseLock(string directory, bool includeSubdirectories = false)
        {
            int num = 0;
            SearchOption searchOption = includeSubdirectories ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly;
            foreach (string str in global::System.IO.Directory.GetFiles(directory, "dir.lock", searchOption))
            {
                File.Delete(str);
                num++;
            }
            return num;
        }

        /// <summary>
        /// Releases the lock.
        /// </summary>
        /// <param name="forceRemove">If set to true, the lock will be removed forcefully.</param>
        public override void ReleaseLock(bool forceRemove)
        {
            string path = Path.Combine(this.Directory, "dir.lock");
            if (forceRemove)
            {
                ForceReleaseLock(this.Directory, false);
            }
            else
            {
                this.RemoveLock(path);
            }
        }

        /// <summary>
        /// The directory that is being locked.
        /// </summary>
        /// <value>The directory.</value>
        public string Directory { get; private set; }
    }
}

