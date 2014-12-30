using System;
using System.Globalization;

namespace LFNet.Common.Helpers
{
    public class TypeHelper
    {
        public static T ChangeType<T>(object v)
        {
            Type enumType = typeof(T);
            Type type = v.GetType();
            if (enumType.Equals(type))
            {
                return (T) v;
            }
            if (enumType.IsEnum && type.Equals(typeof(string)))
            {
                return (T) Enum.Parse(enumType, v.ToString());
            }
            if (enumType.Equals(typeof(bool)))
            {
                return (T) ToBoolean(v);
            }
            return (T) Convert.ChangeType(v, typeof(T), CultureInfo.InvariantCulture);
        }

        private static object ToBoolean(object value)
        {
            bool flag;
            int num;
            if (bool.TryParse(value.ToString(), out flag))
            {
                return flag;
            }
            if (int.TryParse(value.ToString(), out num))
            {
                return Convert.ToBoolean(num);
            }
            return Convert.ToBoolean(value);
        }
    }
}

