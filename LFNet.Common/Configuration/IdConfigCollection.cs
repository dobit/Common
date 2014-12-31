using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;

namespace LFNet.Configuration
{
    /// <summary>
    /// 按照ID索引的分类
    /// 未排序
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class IdConfigCollection<T> where T : IdConfigInfo
    {
        private List<T> _list = new List<T>();

        public List<T> List
        {
            get { return _list; }
            set { _list = value; }
        }

        #region Static Method
        /// <summary>
        /// 按Id索引的对象
        /// </summary>
        private static readonly Dictionary<int,T> IndexById=new Dictionary<int, T>();
       

        static  IdConfigCollection()
        {
            BuildIndex();
        }

        /// <summary>
        /// 建立索引
        /// </summary>
       protected  static void BuildIndex()
        {
            IndexById.Clear();

            foreach (T item in ConfigFileManager.GetConfig<IdConfigCollection<T>>().List)
            {
                IndexById.Add(item.Id, item);
            }

        }

      
        ///// <summary>
        ///// 获取最大的Id值
        ///// </summary>
        ///// <returns></returns>
        //private static int GetMaxFunctionId()
        //{
        //    int ret = 1;
        //    foreach (T item in Instance.List)
        //    {

        //        if (item.Id > ret)
        //            ret = item.Id;
        //    }
        //    return ret;
        //}

        /// <summary>
        /// 反会所有
        /// </summary>
        public static ReadOnlyCollection<T> ListAll()
        {
            return ConfigFileManager.GetConfig<IdConfigCollection<T>>().List.AsReadOnly();
        }

        #endregion

        /// <summary>
        /// 添加后Id会得到新的值
        /// </summary>
        /// <param name="item"></param>
        public static void Add(T item)
        {
            var config = ConfigFileManager.GetConfig<IdConfigCollection<T>>();
            int l = Count;
            if (l == 0) item.Id = 1;
            else
            {
                item.Id = config.List[Count - 1].Id + 1;
            }
            IndexById.Add(item.Id,item);
            config.List.Add(item);
        }

        public static void Clear()
        {
            IndexById.Clear();
            ConfigFileManager.GetConfig<IdConfigCollection<T>>().List.Clear();
        }

        public static bool Contains(T item)
        {
            return ConfigFileManager.GetConfig<IdConfigCollection<T>>().List.Contains(item);
        }

        public static int Count
        {
            get { return ConfigFileManager.GetConfig<IdConfigCollection<T>>().List.Count; }
        }

        public static bool Remove(T item)
        {
            bool ret = false;
            if (Contains(item))
            {
                ret = ConfigFileManager.GetConfig<IdConfigCollection<T>>().List.Remove(item);
                IndexById.Remove(item.Id);
            }
            return ret;
        }

        public  static bool  Remove(int id)
        {
            return Remove(Get(id));
        }

        public  static  T Get(int id)
        {
            return IndexById[id];
        }

        public static IEnumerator<T> GetEnumerator()
        {
            return ConfigFileManager.GetConfig<IdConfigCollection<T>>().List.GetEnumerator();
        }
    }

    
}
