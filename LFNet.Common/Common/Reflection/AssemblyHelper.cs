using System;
using System.Collections.Generic;
using System.Reflection;

namespace LFNet.Common.Reflection
{
    public class AssemblyHelper
    {
        public static IEnumerable<Assembly> GetAssemblies()
        {
            return AppDomain.CurrentDomain.GetAssemblies();
        }

        public static string GetAssemblyProduct()
        {
            Assembly rootAssembly = GetRootAssembly();
            if (rootAssembly == null)
            {
                return string.Empty;
            }
            object[] customAttributes = rootAssembly.GetCustomAttributes(typeof(AssemblyProductAttribute), false);
            if (customAttributes.Length == 0)
            {
                return string.Empty;
            }
            return ((AssemblyProductAttribute) customAttributes[0]).Product;
        }

        public static string GetAssemblyTitle()
        {
            Assembly rootAssembly = GetRootAssembly();
            if (rootAssembly == null)
            {
                return string.Empty;
            }
            object[] customAttributes = rootAssembly.GetCustomAttributes(typeof(AssemblyTitleAttribute), false);
            if (customAttributes.Length == 0)
            {
                return string.Empty;
            }
            return ((AssemblyTitleAttribute) customAttributes[0]).Title;
        }

        public static Assembly GetRootAssembly()
        {
            Assembly entryAssembly = Assembly.GetEntryAssembly();
            if (entryAssembly == null)
            {
                entryAssembly = Assembly.GetCallingAssembly();
            }
            if (entryAssembly == null)
            {
                entryAssembly = Assembly.GetExecutingAssembly();
            }
            return entryAssembly;
        }
    }
}

