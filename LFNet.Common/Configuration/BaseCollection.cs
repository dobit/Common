using System;
using System.Collections.Generic;

namespace LFNet.Configuration
{
    /// <summary>
    /// 基类集合
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public abstract class BaseCollection<T> : List<T>
    {
        private object locker=new object();
        protected BaseCollection()
        {
            Load();
        }

        /// <summary>
        /// 加载
        /// </summary>
        public void Load()
        {
            try
            {
                IEnumerable<T> collection = (IEnumerable<T>)Utils.Load(typeof(List<T>), GetConfigFilename());
                base.AddRange(collection);
            }
            catch (Exception ex)
            {
                if (ex is System.IO.FileNotFoundException || ex is System.IO.DirectoryNotFoundException)
                    return;
                throw;
            }

        }

        /// <summary>
        /// 配置文件默认保存路径
        /// </summary>
        private static string configPath = ConfigFileManager.ConfigPath;
        /// <summary>
        /// 配置文件地址
        /// </summary>
        protected virtual string GetConfigFilename()
        {
            return configPath+"\\collection\\" + typeof(T).Name+"s."+ConfigFileManager.configFileExtName;
        }


        public new void Add(T item)
        {
            base.Add(item);
            Save();
        }
        /// <summary>
        /// 添加
        /// </summary>
        /// <param name="collection"></param>
        public new void AddRange(IEnumerable<T> collection)
        {
            base.AddRange(collection);

            Save();
        }

        public new void Clear()
        {
            base.Clear();
            Save();
        }

        public new bool Remove(T item)
        {
            if (base.Remove(item))
            {
                Save();
                return true;
            }
            return false;
        }

        public new void RemoveAt(int index)
        {
            base.RemoveAt(index);
            Save();
        }

        public new int RemoveAll(Predicate<T> match)
        {
            int cnt = base.RemoveAll(match);
            if (cnt > 0)
            {
                Save();
            }
            return cnt;
        }

        public new void RemoveRange(int index, int count)
        {
            base.RemoveRange(index, count);
            Save();
        }

        public bool Save()
        {
            bool ret;
            lock (locker)
            {
                ret = Utils.Save(this, GetConfigFilename());
            }
            return ret;
        }

        
    }
}