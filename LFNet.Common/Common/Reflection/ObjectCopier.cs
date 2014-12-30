using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Data.Linq;
using System.IO;
using System.Reflection;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Threading;

namespace LFNet.Common.Reflection
{
    /// <summary>
    /// Copy data from a source into a target object by copying public property values.
    /// </summary>
    /// <remarks></remarks>
    public static class ObjectCopier
    {
        private static readonly Type BinaryType = typeof(Binary);
        private static readonly Type ByteArrayType = typeof(byte[]);
        private static readonly Type NullableType = typeof(Nullable<>);
        private static readonly Type StringType = typeof(string);

        /// <summary>
        /// Uses BinaryFormatter.Serialize to Clone an object.
        /// </summary>
        /// <param name="obj">The source object.</param>
        /// <returns>A cloned copy of the object.</returns>
        public static object BinaryClone(object obj)
        {
            using (MemoryStream stream = new MemoryStream())
            {
                BinaryFormatter formatter = new BinaryFormatter(null, new StreamingContext(StreamingContextStates.Clone));
                formatter.Serialize(stream, obj);
                stream.Seek(0L, SeekOrigin.Begin);
                return formatter.Deserialize(stream);
            }
        }

        /// <summary>
        /// Attempts to coerce a value of one type into
        /// a value of a different type.
        /// </summary>
        /// <param name="desiredType">
        /// Type to which the value should be coerced.
        /// </param>
        /// <param name="valueType">
        /// Original type of the value.
        /// </param>
        /// <param name="value">
        /// The value to coerce.
        /// </param>
        /// <remarks>
        /// <para>
        /// If the desired type is a primitive type or Decimal, 
        /// empty string and null values will result in a 0 
        /// or equivalent.
        /// </para>
        /// <para>
        /// If the desired type is a <see cref="T:System.Nullable" /> type, empty string
        /// and null values will result in a null result.
        /// </para>
        /// <para>
        /// If the desired type is an <c>enum</c> the value's ToString()
        /// result is parsed to convert into the <c>enum</c> value.
        /// </para>
        /// </remarks>
        public static object CoerceValue(Type desiredType, Type valueType, object value)
        {
            if (desiredType.Equals(valueType))
            {
                return value;
            }
            if (desiredType.IsGenericType && (desiredType.GetGenericTypeDefinition() == NullableType))
            {
                if (value == null)
                {
                    return null;
                }
                if (valueType.Equals(StringType) && (Convert.ToString(value) == string.Empty))
                {
                    return null;
                }
            }
            desiredType = GetUnderlyingType(desiredType);
            if ((desiredType.IsPrimitive || desiredType.Equals(typeof(decimal))) && (valueType.Equals(StringType) && string.IsNullOrEmpty((string) value)))
            {
                return 0;
            }
            if (value == null)
            {
                return null;
            }
            if (desiredType.Equals(typeof(Guid)))
            {
                return new Guid(value.ToString());
            }
            if (desiredType.IsEnum && valueType.Equals(StringType))
            {
                return Enum.Parse(desiredType, value.ToString(), true);
            }
            if (((desiredType.IsArray && desiredType.Equals(ByteArrayType)) || desiredType.Equals(BinaryType)) && valueType.Equals(StringType))
            {
                byte[] buffer = Convert.FromBase64String((string) value);
                if (desiredType.IsArray && desiredType.Equals(ByteArrayType))
                {
                    return buffer;
                }
                return new Binary(buffer);
            }
            if (((valueType.IsArray && valueType.Equals(ByteArrayType)) || valueType.Equals(BinaryType)) && desiredType.Equals(StringType))
            {
                byte[] inArray = (value is Binary) ? ((Binary) value).ToArray() : ((byte[]) value);
                return Convert.ToBase64String(inArray);
            }
            try
            {
                if (desiredType.Equals(StringType))
                {
                    return value.ToString();
                }
                return Convert.ChangeType(value, desiredType, Thread.CurrentThread.CurrentCulture);
            }
            catch
            {
                TypeConverter converter = TypeDescriptor.GetConverter(desiredType);
                if ((converter == null) || !converter.CanConvertFrom(valueType))
                {
                    throw;
                }
                return converter.ConvertFrom(value);
            }
        }

        /// <summary>
        /// Copies values from the <see cref="T:System.Collections.Specialized.NameValueCollection" /> into the properties of the target.
        /// </summary>
        /// <param name="source">The <see cref="T:System.Collections.Specialized.NameValueCollection" /> source.</param>
        /// <param name="target">The target object.</param>
        public static void Copy(NameValueCollection source, object target)
        {
            ObjectCopierSettings settings = new ObjectCopierSettings();
            Copy(source, target, settings);
        }

        /// <summary>
        /// Copies values from the source into the properties of the target.
        /// </summary>
        /// <param name="source">An object containing the source values.</param>
        /// <param name="target">An object with properties to be set from the source.</param>
        /// <remarks>
        /// The property names and types of the source object must match the property names and types
        /// on the target object. Source properties may not be indexed. 
        /// Target properties may not be readonly or indexed.
        /// </remarks>
        public static void Copy(object source, object target)
        {
            ObjectCopierSettings settings = new ObjectCopierSettings();
            Copy(source, target, settings);
        }

        /// <summary>
        /// Copies values from the source into the target <see cref="T:System.Collections.IDictionary" />.
        /// </summary>
        /// <param name="source">The source object.</param>
        /// <param name="target">The target <see cref="T:System.Collections.IDictionary" />.</param>
        public static void Copy(object source, IDictionary<string, object> target)
        {
            ObjectCopierSettings settings = new ObjectCopierSettings();
            Copy(source, target, settings);
        }

        /// <summary>
        /// Copies values from the <see cref="T:System.Collections.IDictionary" /> into the properties of the target.
        /// </summary>
        /// <param name="source">The <see cref="T:System.Collections.IDictionary" /> source.</param>
        /// <param name="target">The target object.</param>
        public static void Copy(IDictionary<string, object> source, object target)
        {
            ObjectCopierSettings settings = new ObjectCopierSettings();
            Copy(source, target, settings);
        }

        /// <summary>
        /// Copies values from the <see cref="T:System.Collections.Specialized.NameValueCollection" /> into the properties of the target.
        /// </summary>
        /// <param name="source">The <see cref="T:System.Collections.Specialized.NameValueCollection" /> source.</param>
        /// <param name="target">The target object.</param>
        /// <param name="ignoreList">A list of property names to ignore. 
        /// These properties will not be set on the target object.</param>
        public static void Copy(NameValueCollection source, object target, params string[] ignoreList)
        {
            ObjectCopierSettings settings = new ObjectCopierSettings {
                IgnoreList = new List<string>(ignoreList)
            };
            Copy(source, target, settings);
        }

        /// <summary>
        /// Copies values from the <see cref="T:System.Collections.IDictionary" /> into the properties of the target.
        /// </summary>
        /// <param name="source">The <see cref="T:System.Collections.IDictionary" /> source.</param>
        /// <param name="target">The target object.</param>
        /// <param name="ignoreList">A list of property names to ignore. 
        /// These properties will not be set on the target object.</param>
        public static void Copy(IDictionary<string, object> source, object target, params string[] ignoreList)
        {
            ObjectCopierSettings settings = new ObjectCopierSettings {
                IgnoreList = new List<string>(ignoreList)
            };
            Copy(source, target, settings);
        }

        /// <summary>
        /// Copies values from the <see cref="T:System.Collections.Specialized.NameValueCollection" /> into the properties of the target.
        /// </summary>
        /// <param name="source">The <see cref="T:System.Collections.Specialized.NameValueCollection" /> source.</param>
        /// <param name="target">The target object.</param>
        /// <param name="settings">The settings to use when copying properties.</param>
        public static void Copy(NameValueCollection source, object target, ObjectCopierSettings settings)
        {
            Dictionary<string, object> dictionary = new Dictionary<string, object>();
            for (int i = 0; i < source.Count; i++)
            {
                if (!string.IsNullOrEmpty(source.Keys[i]))
                {
                    dictionary.Add(source.Keys[i], source[i]);
                }
            }
            Copy((IDictionary<string, object>) dictionary, target, settings);
        }

        /// <summary>
        /// Copies values from the source into the properties of the target.
        /// </summary>
        /// <param name="source">An object containing the source values.</param>
        /// <param name="target">An object with properties to be set from the source.</param>
        /// <param name="ignoreList">A list of property names to ignore. 
        /// These properties will not be set on the target object.</param>
        /// <remarks>
        /// The property names and types of the source object must match the property names and types
        /// on the target object. Source properties may not be indexed. 
        /// Target properties may not be readonly or indexed.
        /// </remarks>
        public static void Copy(object source, object target, params string[] ignoreList)
        {
            ObjectCopierSettings settings = new ObjectCopierSettings {
                IgnoreList = new List<string>(ignoreList)
            };
            Copy(source, target, settings);
        }

        /// <summary>
        /// Copies values from the source into the target <see cref="T:System.Collections.IDictionary" />.
        /// </summary>
        /// <param name="source">The source object.</param>
        /// <param name="target">The target <see cref="T:System.Collections.IDictionary" />.</param>
        /// <param name="ignoreList">A list of property names to ignore. 
        /// These properties will not be added to the targeted <see cref="T:System.Collections.IDictionary" />.</param>
        public static void Copy(object source, IDictionary<string, object> target, params string[] ignoreList)
        {
            ObjectCopierSettings settings = new ObjectCopierSettings {
                IgnoreList = new List<string>(ignoreList)
            };
            Copy(source, target, settings);
        }

        /// <summary>
        /// Copies values from the source into the properties of the target.
        /// </summary>
        /// <param name="source">An object containing the source values.</param>
        /// <param name="target">An object with properties to be set from the source.</param>
        /// <param name="settings">The settings to use when copying properties.</param>
        /// <remarks>
        /// <para>
        /// The property names and types of the source object must match the property names and types
        /// on the target object. Source properties may not be indexed.
        /// Target properties may not be readonly or indexed.
        /// </para><para>
        /// Properties to copy are determined based on the source object. Any properties
        /// on the source object marked with the <see cref="T:System.ComponentModel.BrowsableAttribute" /> equal
        /// to false are ignored.
        /// </para>
        /// </remarks>
        public static void Copy(object source, object target, ObjectCopierSettings settings)
        {
            string[] cachedPropertyNames;
            if (source == null)
            {
                throw new ArgumentNullException("source", "Source object can not be Null.");
            }
            if (target == null)
            {
                throw new ArgumentNullException("target", "Target object can not be Null.");
            }
            if (settings == null)
            {
                settings = new ObjectCopierSettings();
            }
            if (settings.UseDynamicCache)
            {
                cachedPropertyNames = MethodCaller.GetCachedPropertyNames(source.GetType());
            }
            else
            {
                cachedPropertyNames = MethodCaller.GetPropertyNames(source.GetType());
            }
            foreach (string str in cachedPropertyNames)
            {
                if (!settings.IgnoreList.Contains(str))
                {
                    try
                    {
                        object obj2 = GetPropertyValue(source, str, settings.UseDynamicCache);
                        SetPropertyValue(target, str, obj2, settings.UseDynamicCache);
                    }
                    catch (Exception exception)
                    {
                        if (!settings.SuppressExceptions)
                        {
                            throw new InvalidOperationException(string.Format("Property '{0}' copy failed.", str), exception);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Copies values from the <see cref="T:System.Collections.IDictionary" /> into the properties of the target.
        /// </summary>
        /// <param name="source">The <see cref="T:System.Collections.IDictionary" /> source.</param>
        /// <param name="target">The target object.</param>
        /// <param name="settings">The settings to use when copying properties.</param>
        public static void Copy(IDictionary<string, object> source, object target, ObjectCopierSettings settings)
        {
            if (source == null)
            {
                throw new ArgumentNullException("source", "Source object can not be Null.");
            }
            if (target == null)
            {
                throw new ArgumentNullException("target", "Target object can not be Null.");
            }
            if (settings == null)
            {
                settings = new ObjectCopierSettings();
            }
            foreach (KeyValuePair<string, object> pair in source)
            {
                if (!settings.IgnoreList.Contains(pair.Key))
                {
                    try
                    {
                        SetPropertyValue(target, pair.Key, pair.Value, settings.UseDynamicCache);
                    }
                    catch (Exception exception)
                    {
                        if (!settings.SuppressExceptions)
                        {
                            throw new ArgumentException(string.Format("Property '{0}' copy failed.", pair.Key), exception);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Copies values from the source into the target <see cref="T:System.Collections.IDictionary" />.
        /// </summary>
        /// <param name="source">The source object.</param>
        /// <param name="target">The target <see cref="T:System.Collections.IDictionary" />.</param>
        /// <param name="settings">The settings to use when copying properties.</param>
        public static void Copy(object source, IDictionary<string, object> target, ObjectCopierSettings settings)
        {
            string[] cachedPropertyNames;
            if (source == null)
            {
                throw new ArgumentNullException("source", "Source object can not be Null.");
            }
            if (target == null)
            {
                throw new ArgumentNullException("target", "Target object can not be Null.");
            }
            if (settings == null)
            {
                settings = new ObjectCopierSettings();
            }
            if (settings.UseDynamicCache)
            {
                cachedPropertyNames = MethodCaller.GetCachedPropertyNames(source.GetType());
            }
            else
            {
                cachedPropertyNames = MethodCaller.GetPropertyNames(source.GetType());
            }
            foreach (string str in cachedPropertyNames)
            {
                if (!settings.IgnoreList.Contains(str))
                {
                    try
                    {
                        object obj2 = GetPropertyValue(source, str, settings.UseDynamicCache);
                        target.Add(str, obj2);
                    }
                    catch (Exception exception)
                    {
                        if (!settings.SuppressExceptions)
                        {
                            throw new ArgumentException(string.Format("Property '{0}' copy failed.", str), exception);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Copies values from the <see cref="T:System.Collections.Specialized.NameValueCollection" /> into the properties of the target.
        /// </summary>
        /// <param name="source">The <see cref="T:System.Collections.Specialized.NameValueCollection" /> source.</param>
        /// <param name="target">The target object.</param>
        /// <param name="ignoreList">A list of property names to ignore. 
        /// These properties will not be set on the target object.</param>
        /// <param name="suppressExceptions">If <see langword="true" />, any exceptions will be suppressed.</param>
        public static void Copy(NameValueCollection source, object target, bool suppressExceptions, params string[] ignoreList)
        {
            ObjectCopierSettings settings = new ObjectCopierSettings {
                SuppressExceptions = suppressExceptions,
                IgnoreList = new List<string>(ignoreList)
            };
            Copy(source, target, settings);
        }

        /// <summary>
        /// Copies values from the source into the target <see cref="T:System.Collections.IDictionary" />.
        /// </summary>
        /// <param name="source">The source object.</param>
        /// <param name="target">The target <see cref="T:System.Collections.IDictionary" />.</param>
        /// <param name="ignoreList">A list of property names to ignore. 
        /// These properties will not be added to the targeted <see cref="T:System.Collections.IDictionary" />.</param>
        /// <param name="suppressExceptions">If <see langword="true" />, any exceptions will be suppressed.</param>
        public static void Copy(object source, IDictionary<string, object> target, bool suppressExceptions, params string[] ignoreList)
        {
            ObjectCopierSettings settings = new ObjectCopierSettings {
                SuppressExceptions = suppressExceptions,
                IgnoreList = new List<string>(ignoreList)
            };
            Copy(source, target, settings);
        }

        /// <summary>
        /// Copies values from the source into the properties of the target.
        /// </summary>
        /// <param name="source">An object containing the source values.</param>
        /// <param name="target">An object with properties to be set from the source.</param>
        /// <param name="ignoreList">A list of property names to ignore. 
        /// These properties will not be set on the target object.</param>
        /// <param name="suppressExceptions">If <see langword="true" />, any exceptions will be suppressed.</param>
        /// <remarks>
        /// <para>
        /// The property names and types of the source object must match the property names and types
        /// on the target object. Source properties may not be indexed. 
        /// Target properties may not be readonly or indexed.
        /// </para><para>
        /// Properties to copy are determined based on the source object. Any properties
        /// on the source object marked with the <see cref="T:System.ComponentModel.BrowsableAttribute" /> equal
        /// to false are ignored.
        /// </para>
        /// </remarks>
        public static void Copy(object source, object target, bool suppressExceptions, params string[] ignoreList)
        {
            ObjectCopierSettings settings = new ObjectCopierSettings {
                SuppressExceptions = suppressExceptions,
                IgnoreList = new List<string>(ignoreList)
            };
            Copy(source, target, settings);
        }

        /// <summary>
        /// Copies values from the <see cref="T:System.Collections.IDictionary" /> into the properties of the target.
        /// </summary>
        /// <param name="source">The <see cref="T:System.Collections.IDictionary" /> source.</param>
        /// <param name="target">The target object.</param>
        /// <param name="ignoreList">A list of property names to ignore. 
        /// These properties will not be set on the target object.</param>
        /// <param name="suppressExceptions">If <see langword="true" />, any exceptions will be suppressed.</param>
        public static void Copy(IDictionary<string, object> source, object target, bool suppressExceptions, params string[] ignoreList)
        {
            ObjectCopierSettings settings = new ObjectCopierSettings {
                SuppressExceptions = suppressExceptions,
                IgnoreList = new List<string>(ignoreList)
            };
            Copy(source, target, settings);
        }

        /// <summary>
        /// Finds a <see cref="T:System.Reflection.PropertyInfo" /> by name ignoring case.
        /// </summary>
        /// <param name="type">The type to search.</param>
        /// <param name="propertyName">Name of the property.</param>
        /// <returns>A <see cref="T:System.Reflection.PropertyInfo" /> matching the property name.</returns>
        /// <remarks>
        /// FindProperty will first try to get a property matching the name and case of the 
        /// property name specified.  If a property cannot be found, all the properties will
        /// be searched ignoring the case of the name.
        /// </remarks>
        public static PropertyInfo FindProperty(Type type, string propertyName)
        {
            return MethodCaller.FindProperty(type, propertyName);
        }

        /// <summary>
        /// Gets an object's property value by name.
        /// </summary>
        /// <param name="target">Object containing the property to get.</param>
        /// <param name="propertyName">Name of the property.</param>
        /// <returns>The value of the property.</returns>
        public static object GetPropertyValue(object target, string propertyName)
        {
            return GetPropertyValue(target, propertyName, true);
        }

        /// <summary>
        /// Gets an object's property value by name.
        /// </summary>
        /// <param name="target">Object containing the property to get.</param>
        /// <param name="propertyName">Name of the property.</param>
        /// <param name="useCache">if set to <c>true</c> use dynamic cache.</param>
        /// <returns>The value of the property.</returns>
        public static object GetPropertyValue(object target, string propertyName, bool useCache)
        {
            if (target == null)
            {
                throw new ArgumentNullException("target");
            }
            if (string.IsNullOrEmpty(propertyName))
            {
                throw new ArgumentNullException("propertyName");
            }
            if (useCache)
            {
                return MethodCaller.GetCachedProperty(target.GetType(), propertyName).DynamicMemberGet(target);
            }
            return MethodCaller.FindProperty(target.GetType(), propertyName).GetValue(target, null);
        }

        /// <summary>
        /// Gets the underlying type dealing with <see cref="T:System.Nullable" />.
        /// </summary>
        /// <param name="type">The type.</param>
        /// <returns>Returns a type dealing with <see cref="T:System.Nullable" />.</returns>
        public static Type GetUnderlyingType(Type type)
        {
            Type nullableType = type;
            if (nullableType.IsGenericType && (nullableType.GetGenericTypeDefinition() == NullableType))
            {
                return Nullable.GetUnderlyingType(nullableType);
            }
            return nullableType;
        }

        /// <summary>
        /// Sets an object's property with the specified value,
        /// converting that value to the appropriate type if possible.
        /// </summary>
        /// <param name="target">Object containing the property to set.</param>
        /// <param name="propertyName">Name of the property to set.</param>
        /// <param name="value">Value to set into the property.</param>
        public static void SetPropertyValue(object target, string propertyName, object value)
        {
            SetPropertyValue(target, propertyName, value, true);
        }

        /// <summary>
        /// Sets an object's property with the specified value,
        /// converting that value to the appropriate type if possible.
        /// </summary>
        /// <param name="target">Object containing the property to set.</param>
        /// <param name="propertyName">Name of the property to set.</param>
        /// <param name="value">Value to set into the property.</param>
        /// <param name="useCache">if set to <c>true</c> use dynamic cache.</param>
        public static void SetPropertyValue(object target, string propertyName, object value, bool useCache)
        {
            if (target == null)
            {
                throw new ArgumentNullException("target", "Target object can not be Null.");
            }
            if (useCache)
            {
                DynamicMemberHandle cachedProperty = MethodCaller.GetCachedProperty(target.GetType(), propertyName);
                if (cachedProperty != null)
                {
                    SetValueWithCoercion(target, cachedProperty, value);
                }
            }
            else
            {
                PropertyInfo handle = MethodCaller.FindProperty(target.GetType(), propertyName);
                if (handle != null)
                {
                    SetValueWithCoercion(target, handle, value);
                }
            }
        }

        private static void SetValueWithCoercion(object target, DynamicMemberHandle handle, object value)
        {
            if (value != null)
            {
                Type memberType = handle.MemberType;
                Type underlyingType = GetUnderlyingType(value.GetType());
                object arg = CoerceValue(memberType, underlyingType, value);
                if (arg != null)
                {
                    handle.DynamicMemberSet(target, arg);
                }
            }
        }

        private static void SetValueWithCoercion(object target, PropertyInfo handle, object value)
        {
            if (value != null)
            {
                Type propertyType = handle.PropertyType;
                Type underlyingType = GetUnderlyingType(value.GetType());
                object obj2 = CoerceValue(propertyType, underlyingType, value);
                if (obj2 != null)
                {
                    handle.SetValue(target, obj2, null);
                }
            }
        }
    }
}

