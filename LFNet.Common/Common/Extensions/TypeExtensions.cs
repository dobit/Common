using System;
using System.ComponentModel;

namespace LFNet.Common.Extensions
{
    public static class TypeExtensions
    {
        public static bool IsNullable(this Type type)
        {
            if (type.IsValueType)
            {
                return false;
            }
            return (type.IsGenericType && (type.GetGenericTypeDefinition() == typeof(Nullable<>)));
        }

        public static bool IsNumeric(this Type type)
        {
            if (!type.IsArray)
            {
                switch (Type.GetTypeCode(type))
                {
                    case TypeCode.SByte:
                    case TypeCode.Byte:
                    case TypeCode.Int16:
                    case TypeCode.UInt16:
                    case TypeCode.Int32:
                    case TypeCode.UInt32:
                    case TypeCode.Int64:
                    case TypeCode.UInt64:
                    case TypeCode.Single:
                    case TypeCode.Double:
                    case TypeCode.Decimal:
                        return true;
                }
            }
            return false;
        }

        public static T ToType<T>(this object value)
        {
            if (value == null)
            {
                throw new ArgumentNullException("value");
            }
            Type type = typeof(T);
            Type c = value.GetType();
            if (type.IsAssignableFrom(c))
            {
                return (T) value;
            }
            if ((c.IsEnum || (value is string)) && type.IsEnum)
            {
                if (!EnumHelper.TryEnumIsDefined(type, value.ToString()))
                {
                    throw new ArgumentException(string.Format("The Enum value of '{0}' is not defined as a valid value for '{1}'.", value, type.FullName));
                }
                return (T) Enum.Parse(type, value.ToString(), false);
            }
            if (c.IsNumeric() && type.IsEnum)
            {
                return (T) Enum.ToObject(type, value);
            }
            TypeConverter converter = TypeDescriptor.GetConverter(type);
            if ((converter != null) && converter.CanConvertFrom(c))
            {
                return (T) converter.ConvertFrom(value);
            }
            if (value is IConvertible)
            {
                try
                {
                    return (T) Convert.ChangeType(value, type);
                }
                catch (Exception exception)
                {
                    throw new ArgumentException(string.Format("An incompatible value specified.  Target Type: {0} Value Type: {1}", type.FullName, value.GetType().FullName), "value", exception);
                }
            }
            throw new ArgumentException(string.Format("An incompatible value specified.  Target Type: {0} Value Type: {1}", type.FullName, value.GetType().FullName), "value");
        }
    }
}

