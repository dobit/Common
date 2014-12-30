using System;
using System.Collections.Generic;
using System.Linq;

namespace LFNet.Common.Collections.Generic
{
   

    /// <summary>
    /// �����б�
    /// </summary>
    /// <typeparam name="T">����</typeparam>
    /// <typeparam name="TKey">��������</typeparam>
    public class ParentKeyedList<T, TKey>:KeyedList<T,TKey>
    {
        private readonly Func<T, TKey> _parentKeySelector;
        private readonly Func<T, object> _orderKeySelector;
        private Dictionary<TKey, List<T>> _parentDict;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="listFunc">����ȫ���б��ί��</param>
        /// <param name="keySelector">����ί��</param>
        /// <param name="parentKeySelector">������ </param>
        /// <param name="orderKeySelector">����ѡ�� </param>
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
        /// �������������ֵ��
        /// </summary>
        public Dictionary<TKey, List<T>> ParentKeyedDictionary { get { return _parentDict; } }

        /// <summary>
        /// ͨ��������ֵ��ȡ�б�
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
        /// ��������
        /// </summary>
        public override void Reset()
        {
            base.Reset();
            _parentDict = null;
        }
    }
}