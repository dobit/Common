using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Globalization;
using System.Runtime.Serialization;
using System.Text;

namespace LFNet.Common
{
#if UN
    [Serializable]
    public class NameObjectCollection<T> : Dictionary<string, T>
    {
        public NameObjectCollection()
        {

        }
        public NameObjectCollection(bool ignoreCase)
            : base(ignoreCase?StringComparer.OrdinalIgnoreCase:StringComparer.Ordinal)
        {

        }

        /// <summary>
        /// 如果不存在 返回一个空值
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public new T this[string name]
        {
            get
            {
                T value;
                TryGetValue(name, out value);
                return value;
            }
            set
            {
                base.Add(name, value);
            }


        }
    }
#endif

}