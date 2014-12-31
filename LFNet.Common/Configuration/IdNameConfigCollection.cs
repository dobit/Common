using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace LFNet.Configuration
{
    /// <summary>
    /// Name为可重复多集合
    /// 如果要把Name作为主键请使用NameConfigCollection
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class IdNameConfigCollection<T>  where T : IdNameConfigInfo
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
        /// <summary>
        /// 按名字索引的对象
        /// </summary>
        private static readonly Dictionary<string, List<T>> IndexByName = new Dictionary<string, List<T>>();

        static  IdNameConfigCollection()
        {
            BuildIndex();
        }

        /// <summary>
        /// 建立索引
        /// </summary>
       protected  static void BuildIndex()
        {
            IndexById.Clear();
            IndexByName.Clear();
            foreach (T item in ConfigFileManager.GetConfig<IdNameConfigCollection<T>>().List)
            {
                IndexById.Add(item.Id, item);
                if (!IndexByName.ContainsKey(item.Name))
                {
                    IndexByName.Add(item.Name, new List<T>() { item });
                }
                else
                    IndexByName[item.Name].Add(item);
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
        /// 返回所有
        /// </summary>
        public static ReadOnlyCollection<T> ListAll()
        {
            return ConfigFileManager.GetConfig<IdNameConfigCollection<T>>().List.AsReadOnly();
        }

        #endregion

        /// <summary>
        /// 添加后Id会得到新的值
        /// </summary>
        /// <param name="item"></param>
        public static void Add(T item)
        {
            int l = Count;
            if (l == 0) item.Id = 1;
            else
            {
                item.Id = ConfigFileManager.GetConfig<IdNameConfigCollection<T>>().List[Count - 1].Id + 1;
            }
            //IndexById.Add(item.Id,item);
            //if (!IndexByName.ContainsKey(item.Name))
            //{
            //    IndexByName.Add(item.Name, new List<T>() { item });
            //}
            //else
            //    IndexByName[item.Name].Add(item);
            ConfigFileManager.GetConfig<IdNameConfigCollection<T>>().List.Add(item);
             ConfigFileManager.SaveConfig<IdNameConfigCollection<T>>();
            BuildIndex();
        }

        public static void Clear()
        {
            IndexById.Clear();
            IndexByName.Clear();
            ConfigFileManager.GetConfig<IdNameConfigCollection<T>>().List.Clear();
        }

        public static bool Contains(T item)
        {
            return ConfigFileManager.GetConfig<IdNameConfigCollection<T>>().List.Contains(item);
        }

        public static int Count
        {
            get { return ConfigFileManager.GetConfig<IdNameConfigCollection<T>>().List.Count; }
        }

        public static bool Remove(T item)
        {
            bool ret = false;
            if (Contains(item))
            {
                ret = ConfigFileManager.GetConfig<IdNameConfigCollection<T>>().List.Remove(item);
                //IndexById.Remove(item.Id);
                //if (!IndexByName.ContainsKey(item.Name))
                //{
                //    IndexByName.Add(item.Name, new List<T> { item });
                //}
                //else
                //    IndexByName[item.Name].Add(item);
                ConfigFileManager.SaveConfig<IdNameConfigCollection<T>>(); 
                BuildIndex();
            }
            return ret;
        }

        public  static bool Remove(int id)
        {
            return Remove(Get(id));
        }

        public  static  T Get(int id)
        {
            if (IndexById.ContainsKey(id))
                return IndexById[id];
            return null;
        }

        public  static ReadOnlyCollection<T> Get(string name)
        {
            if (IndexByName.ContainsKey(name))
                return IndexByName[name].AsReadOnly();
            return null;
        }

        public static IEnumerator<T> GetEnumerator()
        {
            return ConfigFileManager.GetConfig<IdNameConfigCollection<T>>().List.GetEnumerator();
        }

        /// <summary>
        /// 更新
        /// </summary>
        /// <param name="item"></param>
        public static void Update(T item)
        {
            T ordinaryItem = Get(item.Id);
            if (!ordinaryItem.Equals(item))
            {
                int pos = ConfigFileManager.GetConfig<IdNameConfigCollection<T>>().List.IndexOf(ordinaryItem);
                ConfigFileManager.GetConfig<IdNameConfigCollection<T>>().List[pos] = item;
            }
             ConfigFileManager.SaveConfig<IdNameConfigCollection<T>>(); 
            BuildIndex();
        }
    }
}
