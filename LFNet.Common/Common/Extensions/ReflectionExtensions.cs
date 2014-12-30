using System.ComponentModel;
using System.Reflection;

namespace LFNet.Common.Extensions
{
    public static class ReflectionExtensions
    {
        public static string Description(this PropertyInfo property)
        {
            object[] customAttributes = property.GetCustomAttributes(typeof(DescriptionAttribute), true);
            if ((customAttributes.Length > 0) && (customAttributes[0] is DescriptionAttribute))
            {
                return ((DescriptionAttribute) customAttributes[0]).Description;
            }
            return string.Empty;
        }

        public static bool IsBrowsable(this PropertyInfo property)
        {
            object[] customAttributes = property.GetCustomAttributes(typeof(BrowsableAttribute), true);
            if (customAttributes.Length > 0)
            {
                return ((BrowsableAttribute) customAttributes[0]).Browsable;
            }
            return true;
        }
    }
}

