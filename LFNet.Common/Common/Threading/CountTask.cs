using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace LFNet.Common.Threading
{
    /// <summary>
    /// 计数任务
    /// </summary>
    public class CountTask<TKey>
    {
        private Action<IEnumerable<TKey>, int> _action;

        public CountTask(Action<IEnumerable<TKey>, int> action)
        {
            _action = action;
        }


        private Dictionary<TKey, int> _dict = new Dictionary<TKey, int>();

        private void RunTask()
        {
            _isRuning = true;
            do
            {
                Dictionary<TKey, int> actionDict = _dict;
                lock (_dict)
                {

                    _dict = new Dictionary<TKey, int>();
                }
                foreach (IGrouping<int, TKey> g in actionDict.GroupBy(p => p.Value, p => p.Key))
                {
                    IGrouping<int, TKey> g1 = g;
                    _action(g1, g1.Key);  
                }
            } while (_dict.Count > 0);
            _isRuning = false;
        }

        public void Add(TKey instance, int count = 1)
        {
            lock (_dict)
            {
                if (_dict.ContainsKey(instance))
                {
                    _dict[instance] += count;
                }
                else
                    _dict.Add(instance, count);
            }
            if (!IsRuning)
            {
                ThreadPool.QueueUserWorkItem(o => RunTask());
            }
        }

        private bool _isRuning;

        /// <summary>
        /// 是否在执行
        /// </summary>
        public bool IsRuning
        {
            get { return _isRuning; }
            //set { _isRuning = value; }
        }
    }
}