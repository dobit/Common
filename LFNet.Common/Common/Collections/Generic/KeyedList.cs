using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace LFNet.Common.Collections.Generic
{
    /// <summary>
    /// 快速列表
    /// </summary>
    /// <typeparam name="T">对象类型</typeparam>
    /// <typeparam name="TKey">主键类型</typeparam>
    public class KeyedList<T, TKey>:IEnumerable<T>
    {
        private readonly Func<List<T>> _listFunc;
        private readonly Func<T, TKey> _keySelector;

        private List<T> _list;
        private Dictionary<TKey, T> _dict;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="listFunc">返回全部列表的委托</param>
        /// <param name="keySelector">主键委托</param>
        public KeyedList(Func<List<T>> listFunc, Func<T, TKey> keySelector)
        {
            _listFunc = listFunc;
            _keySelector = keySelector;
        }

        private void Init()
        {
            _list = _listFunc();
            _dict = _list.ToDictionary(_keySelector);
        }

        /// <summary>
        /// 返回列表
        /// </summary>
        public List<T> List
        {
            get
            {
                if (_list == null)
                    Init();
                return _list;
            }

        }

        /// <summary>
        /// 通过主键获取一项
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public T this[TKey key]
        {
            get { return Get(key); }
        }

        /// <summary>
        /// 通过主键获取一项
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public T Get(TKey key)
        {
            if (_list == null)
                Init();
            if(!_dict.ContainsKey(key)) return default(T);
            return _dict[key];
        }

        /// <summary>
        /// 对象重置
        /// </summary>
        public virtual void Reset()
        {
            _list = null;
        }

        #region Implementation of IEnumerable

        /// <summary>
        /// Returns an enumerator that iterates through the collection.
        /// </summary>
        /// <returns>
        /// A <see cref="T:System.Collections.Generic.IEnumerator`1"/> that can be used to iterate through the collection.
        /// </returns>
        /// <filterpriority>1</filterpriority>
        public IEnumerator<T> GetEnumerator()
        {
            return List.GetEnumerator();
        }

        /// <summary>
        /// Returns an enumerator that iterates through a collection.
        /// </summary>
        /// <returns>
        /// An <see cref="T:System.Collections.IEnumerator"/> object that can be used to iterate through the collection.
        /// </returns>
        /// <filterpriority>2</filterpriority>
        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        #endregion

        
    }
}