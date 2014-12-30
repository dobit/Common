using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;

namespace LFNet.Configuration
{
    public class NameConfigCollection<T> : BaseConfig<NameConfigCollection<T>> where T : NameConfigInfo
    {
        private List<T> _list = new List<T>();

        public List<T> List
        {
            get { return _list; }
            set { _list = value; }
        }

        #region Static Method
        /// <summary>
        /// 按名字索引的对象
        /// </summary>
        private static readonly Dictionary<string, T> IndexByName = new Dictionary<string, T>();

        static  NameConfigCollection()
        {
            BuildIndex();
        }

        /// <summary>
        /// 建立索引
        /// </summary>
       protected  static void BuildIndex()
        {
            IndexByName.Clear();
            foreach (T item in Instance.List)
            {
                IndexByName.Add(item.Name,item);
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
            return Instance.List.AsReadOnly();
        }

        #endregion

        /// <summary>
        /// 添加后Id会得到新的值
        /// </summary>
        /// <param name="item"></param>
        public static void Add(T item)
        {
            IndexByName.Add(item.Name,item);
            Instance.List.Add(item);
        }

        public static void Clear()
        {
            IndexByName.Clear();
            Instance.List.Clear();
        }

        public static bool Contains(T item)
        {
            return Instance.List.Contains(item);
        }

        public static int Count
        {
            get { return Instance.List.Count; }
        }

        public static bool Remove(T item)
        {
            bool ret = false;
            if (Contains(item))
            {
                ret= Instance.List.Remove(item);
                IndexByName.Remove(item.Name);
            }
            return ret;
        }

        public  static bool  Remove(string name)
        {
            return Remove(Get(name));
        }
        public  static  T Get(string  name)
        {
            return IndexByName[name];
        }

        public static IEnumerator<T> GetEnumerator()
        {
            return Instance.List.GetEnumerator();
        }
    }
}
