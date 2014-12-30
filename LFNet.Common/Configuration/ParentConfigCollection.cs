using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;
using System.Xml.Serialization;
namespace LFNet.Configuration
{
    /// <summary>
    /// 存在子父级关系的配置对象集合
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class ParentConfigCollection<T> : BaseConfig<ParentConfigCollection<T>> where T : ParentConfigInfo
    {
        private List<T> m_list = new List<T>();

        public List<T> List
        {
            get { return m_list; }
            set { m_list = value; }
        }


        #region Static Method
        /// <summary>
        /// 按Id索引的对象
        /// </summary>
        private static readonly Dictionary<int, T> IndexById = new Dictionary<int, T>();
        /// <summary>
        /// 按名字索引的对象
        /// </summary>
        private static readonly Dictionary<string, List<T>> IndexByName = new Dictionary<string, List<T>>();

        private static readonly Dictionary<int, List<T>> IndexByParentId = new Dictionary<int, List<T>>();

        static ParentConfigCollection()
        {
            BuildIndex();
        }

        /// <summary>
        /// 建立索引
        /// </summary>
        protected static void BuildIndex()
        {
            IndexById.Clear();
            IndexByName.Clear();
            IndexByParentId.Clear();
            foreach (T item in Instance.List)
            {
                IndexById.Add(item.Id, item);
                if (!IndexByName.ContainsKey(item.Name))
                {
                    IndexByName.Add(item.Name, new List<T>() { item });
                }
                else
                    IndexByName[item.Name].Add(item);

                if (!IndexByParentId.ContainsKey(item.ParentId))
                {
                    IndexByParentId.Add(item.ParentId, new List<T>() { item });
                }
                else
                    IndexByParentId[item.ParentId].Add(item);
            }

        }


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
            int l = Count;
            if (l == 0) item.Id = 1;
            else
            {
                item.Id = Instance.List[Count - 1].Id + 1;
            }
            //IndexById.Add(item.Id, item);
            //if (!IndexByName.ContainsKey(item.Name))
            //{
            //    IndexByName.Add(item.Name, new List<T>() { item });
            //}
            //else
            //    IndexByName[item.Name].Add(item);
            //if (!IndexByParentId.ContainsKey(item.ParentId))
            //{
            //    IndexByParentId.Add(item.ParentId, new List<T>() { item });
            //}
            //else
            //    IndexByParentId[item.ParentId].Add(item);
            Instance.List.Add(item);
            Save();
            BuildIndex();
        }

        public static void Clear()
        {
            IndexById.Clear();
            IndexByName.Clear();
            IndexByParentId.Clear();
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

        public  static bool Remove(T item)
        {
            bool ret = false;
            if (Contains(item))
            {
                ret = Instance.List.Remove(item);
                Save();
                BuildIndex();
                //IndexById.Remove(item.Id);
                //IndexByName[item.Name].Remove(item);
                //if (IndexByName[item.Name].Count == 0)
                //{
                //    IndexByName.Remove(item.Name);
                //}
                //IndexByParentId[item.ParentId].Remove(item);
                //if (IndexByParentId[item.ParentId].Count == 0)
                //{
                //    IndexByParentId.Remove(item.ParentId);
                //}
            }
            return ret;
        }

        public static bool Remove(int id)
        {
            return Remove(Get(id));
        }
        /// <summary>
        /// 返回按ID查找到的对象，未找到返回null
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public static T Get(int id)
        {
            if (IndexById.ContainsKey(id))
                return IndexById[id];
            return null;
        }

        public static ReadOnlyCollection<T> Get(string name)
        {
            if (IndexByName.ContainsKey(name))
                return IndexByName[name].AsReadOnly();
            return null;
        }

        public static IEnumerator<T> GetEnumerator()
        {
            return Instance.List.GetEnumerator();
        }

        /// <summary>
        /// 通过父级Id检索
        /// </summary>
        /// <param name="parentId"></param>
        /// <returns></returns>
        public static ReadOnlyCollection<T> GetByParentId(int parentId)
        {
            if (IndexByParentId.ContainsKey(parentId))
                return IndexByParentId[parentId].AsReadOnly();
            else
                return null;
        }

        /// <summary>
        /// 更新
        /// </summary>
        /// <param name="item"></param>
        public static void Update(T item)
        {
            T ordinaryItem = Get(item.Id);
            if(!ordinaryItem.Equals(item))
            {
               int pos=  Instance.List.IndexOf(ordinaryItem);
               Instance.List[pos] = item;
            }
            Save();
            BuildIndex();
        }
    }
}
