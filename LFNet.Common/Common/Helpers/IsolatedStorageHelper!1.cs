using System;
using System.IO;
using System.IO.IsolatedStorage;

namespace LFNet.Common.Helpers
{
    public class IsolatedStorageHelper<T>
    {
        private string _configFile;
        private IsolatedStorageFile _storage;

        public IsolatedStorageHelper(string fileName) : this(IsolatedStorageScope.Assembly | IsolatedStorageScope.User, fileName)
        {
        }

        public IsolatedStorageHelper(IsolatedStorageScope scope, string fileName)
        {
            this._storage = IsolatedStorageFile.GetStore(scope, (Type) null, (Type) null);
            this._configFile = fileName;
        }

        public T Retrieve()
        {
            T local = default(T);
            string[] fileNames = this.Storage.GetFileNames(this.ConfigFile);
            if ((fileNames != null) && (fileNames.Length != 0))
            {
                using (IsolatedStorageFileStream stream = new IsolatedStorageFileStream(this.ConfigFile, FileMode.Open, FileAccess.Read, this.Storage))
                {
                    if ((stream != null) && (stream.Length > 0L))
                    {
                        local = SelfSerializer<T>.Current.XmlDeserialize(stream);
                    }
                }
            }
            return local;
        }

        public void Store(T item)
        {
            using (IsolatedStorageFileStream stream = new IsolatedStorageFileStream(this.ConfigFile, FileMode.Create, FileAccess.Write, this.Storage))
            {
                SelfSerializer<T>.Current.XmlSerialize(item, stream);
            }
        }

        private string ConfigFile
        {
            get
            {
                return this._configFile;
            }
        }

        private IsolatedStorageFile Storage
        {
            get
            {
                return this._storage;
            }
        }
    }
}

