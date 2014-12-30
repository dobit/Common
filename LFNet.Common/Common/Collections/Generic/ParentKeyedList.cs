using System;
using System.Collections.Generic;
using System.Linq;

namespace LFNet.Common.Collections.Generic
{
   

    /// <summary>
    /// 父级列表
    /// </summary>
    /// <typeparam name="T">类型</typeparam>
    /// <typeparam name="TKey">主键类型</typeparam>
    public class ParentKeyedList<T, TKey>:KeyedList<T,TKey>
    {
        private readonly Func<T, TKey> _parentKeySelector;
        private readonly Func<T, object> _orderKeySelector;
        private Dictionary<TKey, List<T>> _parentDict;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="listFunc">返回全部列表的委托</param>
        /// <param name="keySelector">主键委托</param>
        /// <param name="parentKeySelector">父级键 </param>
        /// <param name="orderKeySelector">排序选择 </param>
        public ParentKeyedList(Func<List<T>> listFunc, Func<T, TKey> keySelector, Func<T, TKey> parentKeySelector, Func<T, object> orderKeySelector)
            : base(listFunc, keySelector)
        {
            _parentKeySelector = parentKeySelector;
            _orderKeySelector = orderKeySelector;
        }
        private void Init()
        {

            _parentDict = List.GroupBy(_parentKeySelector).ToDictionary(g => g.Key, g => g.OrderBy(_orderKeySelector).ToList());
        }
        /// <summary>
        /// 按父键建立的字典表
        /// </summary>
        public Dictionary<TKey, List<T>> ParentKeyedDictionary { get { return _parentDict; } }

        /// <summary>
        /// 通过父级键值获取列表
        /// </summary>
        /// <param name="parentKey"></param>
        /// <returns></returns>
        public List<T> GetByParentKey(TKey parentKey)
        {
            if (_parentDict == null)
                Init();
            if (!_parentDict.ContainsKey(parentKey))return new List<T>();
            return _parentDict[parentKey];
        }
        /// <summary>
        /// 对象重置
        /// </summary>
        public override void Reset()
        {
            base.Reset();
            _parentDict = null;
        }
    }
}