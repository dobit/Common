using System;
using System.Globalization;
using System.IO;
using System.Runtime;
using LFNet.Common.Extensions;

namespace LFNet.Common.Helpers
{
    public class HashCodeCombiner
    {
        private long _combinedHash = 0x1505L;

        public void Add(DateTime dt)
        {
            this.Add(dt.GetHashCode());
        }

        public void Add(int n)
        {
            this._combinedHash = ((this._combinedHash << 5) + this._combinedHash) ^ n;
        }

        public void Add(long n)
        {
            this.Add(n.GetHashCode());
        }

        public void Add(object o)
        {
            if (o != null)
            {
                this.Add(o.GetHashCode());
            }
        }

        public void Add(string s)
        {
            if (!string.IsNullOrEmpty(s))
            {
                this.Add(s.GetStableHashCode());
            }
        }

        public void AddCaseInsensitiveString(string s)
        {
            if (!string.IsNullOrEmpty(s))
            {
                this.Add(s.ToLowerInvariant().GetStableHashCode());
            }
        }

        public void AddDirectory(string directoryName)
        {
            DirectoryInfo info = new DirectoryInfo(directoryName);
            if (info.Exists)
            {
                this.AddCaseInsensitiveString(directoryName);
                foreach (DirectoryInfo info2 in info.GetDirectories())
                {
                    this.AddDirectory(info2.FullName);
                }
                foreach (FileInfo info3 in info.GetFiles())
                {
                    this.AddFile(info3.FullName);
                }
                this.Add(info.CreationTimeUtc);
                this.Add(info.LastWriteTimeUtc);
            }
        }

        public void AddFile(string fileName)
        {
            this.AddCaseInsensitiveString(fileName);
            if (File.Exists(fileName))
            {
                FileInfo info = new FileInfo(fileName);
                this.Add(info.CreationTimeUtc);
                this.Add(info.LastWriteTimeUtc);
                this.Add(info.Length);
            }
            else if (Directory.Exists(fileName))
            {
                this.AddDirectory(fileName);
            }
        }

        public long CombinedHash
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this._combinedHash;
            }
        }

        public int CombinedHash32
        {
            get
            {
                return this._combinedHash.GetHashCode();
            }
        }

        public string CombinedHashString
        {
            get
            {
                return this._combinedHash.ToString("x", CultureInfo.InvariantCulture);
            }
        }
    }
}

