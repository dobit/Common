using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Reflection;

namespace LFNet.Common.Reflection
{
	/// <summary>
	/// Provides methods to dynamically find and call methods.
	/// </summary>
	public static class MethodCaller
	{
		private const BindingFlags allLevelFlags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.FlattenHierarchy;
		private const BindingFlags ctorFlags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;
		private const BindingFlags fieldFlags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;
		private const BindingFlags oneLevelFlags = BindingFlags.DeclaredOnly | BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;
		private const BindingFlags propertyFlags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.FlattenHierarchy;
		private static readonly Dictionary<MethodCacheKey, DynamicMemberHandle> _memberCache = new Dictionary<MethodCacheKey, DynamicMemberHandle>();
		private static readonly Dictionary<MethodCacheKey, DynamicMethodHandle> _methodCache = new Dictionary<MethodCacheKey, DynamicMethodHandle>();
		private static readonly Dictionary<Type, DynamicConstructor> _ctorCache = new Dictionary<Type, DynamicConstructor>();
		private static readonly Dictionary<Type, string[]> _propertyNameCache = new Dictionary<Type, string[]>();
		private static DynamicMethodHandle GetCachedMethod(object obj, MethodInfo info, params object[] parameters)
		{
			MethodCacheKey key = new MethodCacheKey(obj.GetType().FullName, info.Name, MethodCaller.GetParameterTypes(parameters));
			DynamicMethodHandle dynamicMethodHandle = null;
			if (MethodCaller._methodCache.TryGetValue(key, out dynamicMethodHandle))
			{
				return dynamicMethodHandle;
			}
			lock (MethodCaller._methodCache)
			{
				if (!MethodCaller._methodCache.TryGetValue(key, out dynamicMethodHandle))
				{
					dynamicMethodHandle = new DynamicMethodHandle(info, parameters);
					MethodCaller._methodCache.Add(key, dynamicMethodHandle);
				}
			}
			return dynamicMethodHandle;
		}
		private static DynamicMethodHandle GetCachedMethod(object obj, string method, params object[] parameters)
		{
			MethodCacheKey key = new MethodCacheKey(obj.GetType().FullName, method, MethodCaller.GetParameterTypes(parameters));
			DynamicMethodHandle dynamicMethodHandle = null;
			if (MethodCaller._methodCache.TryGetValue(key, out dynamicMethodHandle))
			{
				return dynamicMethodHandle;
			}
			lock (MethodCaller._methodCache)
			{
				if (!MethodCaller._methodCache.TryGetValue(key, out dynamicMethodHandle))
				{
					MethodInfo method2 = MethodCaller.GetMethod(obj.GetType(), method, parameters);
					dynamicMethodHandle = new DynamicMethodHandle(method2, parameters);
					MethodCaller._methodCache.Add(key, dynamicMethodHandle);
				}
			}
			return dynamicMethodHandle;
		}
		private static DynamicConstructor GetCachedConstructor(Type objectType)
		{
			DynamicConstructor dynamicConstructor = null;
			if (MethodCaller._ctorCache.TryGetValue(objectType, out dynamicConstructor))
			{
				return dynamicConstructor;
			}
			lock (MethodCaller._ctorCache)
			{
				if (!MethodCaller._ctorCache.TryGetValue(objectType, out dynamicConstructor))
				{
					ConstructorInfo constructor = objectType.GetConstructor(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, Type.EmptyTypes, null);
					dynamicConstructor = DynamicMethodHandlerFactory.CreateConstructor(constructor);
					MethodCaller._ctorCache.Add(objectType, dynamicConstructor);
				}
			}
			return dynamicConstructor;
		}
		/// <summary>
		/// Uses reflection to create an object using its 
		/// default constructor.
		/// </summary>
		/// <param name="objectType">Type of object to create.</param>
		public static object CreateInstance(Type objectType)
		{
			DynamicConstructor cachedConstructor = MethodCaller.GetCachedConstructor(objectType);
			if (cachedConstructor == null)
			{
				throw new NotImplementedException("Default Constructor not implemented.");
			}
			return cachedConstructor();
		}
		internal static DynamicMemberHandle GetCachedProperty(Type objectType, string propertyName)
		{
			MethodCacheKey key = new MethodCacheKey(objectType.FullName, propertyName, MethodCaller.GetParameterTypes(null));
			DynamicMemberHandle dynamicMemberHandle = null;
			if (MethodCaller._memberCache.TryGetValue(key, out dynamicMemberHandle))
			{
				return dynamicMemberHandle;
			}
			lock (MethodCaller._memberCache)
			{
				if (!MethodCaller._memberCache.TryGetValue(key, out dynamicMemberHandle))
				{
					PropertyInfo propertyInfo = MethodCaller.FindProperty(objectType, propertyName);
					if (propertyInfo != null)
					{
						dynamicMemberHandle = new DynamicMemberHandle(propertyInfo);
					}
					MethodCaller._memberCache.Add(key, dynamicMemberHandle);
				}
			}
			return dynamicMemberHandle;
		}
		internal static DynamicMemberHandle GetCachedField(Type objectType, string fieldName)
		{
			MethodCacheKey key = new MethodCacheKey(objectType.FullName, fieldName, MethodCaller.GetParameterTypes(null));
			DynamicMemberHandle dynamicMemberHandle = null;
			if (MethodCaller._memberCache.TryGetValue(key, out dynamicMemberHandle))
			{
				return dynamicMemberHandle;
			}
			lock (MethodCaller._memberCache)
			{
				if (!MethodCaller._memberCache.TryGetValue(key, out dynamicMemberHandle))
				{
					FieldInfo field = objectType.GetField(fieldName, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
					if (field != null)
					{
						dynamicMemberHandle = new DynamicMemberHandle(field);
					}
					MethodCaller._memberCache.Add(key, dynamicMemberHandle);
				}
			}
			return dynamicMemberHandle;
		}
		internal static string[] GetCachedPropertyNames(Type objectType)
		{
			string[] propertyNames;
			if (MethodCaller._propertyNameCache.TryGetValue(objectType, out propertyNames))
			{
				return propertyNames;
			}
			lock (MethodCaller._propertyNameCache)
			{
				if (!MethodCaller._propertyNameCache.TryGetValue(objectType, out propertyNames))
				{
					propertyNames = MethodCaller.GetPropertyNames(objectType);
					MethodCaller._propertyNameCache.Add(objectType, propertyNames);
				}
			}
			return propertyNames;
		}
		internal static string[] GetPropertyNames(Type objectType)
		{
			List<string> list = new List<string>();
			PropertyDescriptorCollection properties = TypeDescriptor.GetProperties(objectType);
			foreach (PropertyDescriptor propertyDescriptor in properties)
			{
				if (propertyDescriptor.IsBrowsable)
				{
					list.Add(propertyDescriptor.Name);
				}
			}
			return list.ToArray();
		}
		/// <summary>
		/// Invokes a property getter using dynamic
		/// method invocation.
		/// </summary>
		/// <param name="obj">Target object.</param>
		/// <param name="property">Property to invoke.</param>
		/// <returns></returns>
		public static object CallPropertyGetter(object obj, string property)
		{
			DynamicMemberHandle cachedProperty = MethodCaller.GetCachedProperty(obj.GetType(), property);
			return cachedProperty.DynamicMemberGet(obj);
		}
		/// <summary>
		/// Invokes a property setter using dynamic
		/// method invocation.
		/// </summary>
		/// <param name="obj">Target object.</param>
		/// <param name="property">Property to invoke.</param>
		/// <param name="value">New value for property.</param>
		public static void CallPropertySetter(object obj, string property, object value)
		{
			DynamicMemberHandle cachedProperty = MethodCaller.GetCachedProperty(obj.GetType(), property);
			cachedProperty.DynamicMemberSet(obj, value);
		}
		/// <summary>
		/// Returns an array of Type objects corresponding
		/// to the type of parameters provided.
		/// </summary>
		/// <param name="parameters">
		/// Parameter values.
		/// </param>
		public static Type[] GetParameterTypes(object[] parameters)
		{
			List<Type> list = new List<Type>();
			if (parameters == null)
			{
				list.Add(typeof(object));
			}
			else
			{
				for (int i = 0; i < parameters.Length; i++)
				{
					object obj = parameters[i];
					list.Add((obj == null) ? typeof(object) : obj.GetType());
				}
			}
			return list.ToArray();
		}
		/// <summary>
		/// Uses reflection to dynamically invoke a method
		/// if that method is implemented on the target object.
		/// </summary>
		/// <param name="obj">
		/// Object containing method.
		/// </param>
		/// <param name="method">
		/// Name of the method.
		/// </param>
		/// <param name="parameters">
		/// Parameters to pass to method.
		/// </param>
		public static object CallMethodIfImplemented(object obj, string method, params object[] parameters)
		{
			DynamicMethodHandle cachedMethod = MethodCaller.GetCachedMethod(obj, method, parameters);
			if (cachedMethod == null || cachedMethod.DynamicMethod == null)
			{
				return null;
			}
			return MethodCaller.CallMethod(obj, cachedMethod, parameters);
		}
		/// <summary>
		/// Uses reflection to dynamically invoke a method,
		/// throwing an exception if it is not
		/// implemented on the target object.
		/// </summary>
		/// <param name="obj">
		/// Object containing method.
		/// </param>
		/// <param name="method">
		/// Name of the method.
		/// </param>
		/// <param name="parameters">
		/// Parameters to pass to method.
		/// </param>
		public static object CallMethod(object obj, string method, params object[] parameters)
		{
			DynamicMethodHandle cachedMethod = MethodCaller.GetCachedMethod(obj, method, parameters);
			if (cachedMethod == null || cachedMethod.DynamicMethod == null)
			{
				throw new NotImplementedException(method + " not implemented.");
			}
			return MethodCaller.CallMethod(obj, cachedMethod, parameters);
		}
		/// <summary>
		/// Uses reflection to dynamically invoke a method,
		/// throwing an exception if it is not
		/// implemented on the target object.
		/// </summary>
		/// <param name="obj">
		/// Object containing method.
		/// </param>
		/// <param name="info">
		/// MethodInfo for the method.
		/// </param>
		/// <param name="parameters">
		/// Parameters to pass to method.
		/// </param>
		public static object CallMethod(object obj, MethodInfo info, params object[] parameters)
		{
			DynamicMethodHandle cachedMethod = MethodCaller.GetCachedMethod(obj, info, parameters);
			if (cachedMethod == null || cachedMethod.DynamicMethod == null)
			{
				throw new NotImplementedException(info.Name + " not implemented.");
			}
			return MethodCaller.CallMethod(obj, cachedMethod, parameters);
		}
		/// <summary>
		/// Uses reflection to dynamically invoke a method,
		/// throwing an exception if it is not implemented
		/// on the target object.
		/// </summary>
		/// <param name="obj">
		/// Object containing method.
		/// </param>
		/// <param name="methodHandle">
		/// MethodHandle for the method.
		/// </param>
		/// <param name="parameters">
		/// Parameters to pass to method.
		/// </param>
		private static object CallMethod(object obj, DynamicMethodHandle methodHandle, params object[] parameters)
		{
			object result = null;
			DynamicMemberMethod arg_08_0 = methodHandle.DynamicMethod;
			object[] array2;
			if (parameters == null)
			{
				object[] array = new object[1];
				array2 = array;
			}
			else
			{
				array2 = parameters;
			}
			if (methodHandle.HasFinalArrayParam)
			{
				int methodParamsLength = methodHandle.MethodParamsLength;
				int num = array2.Length - (methodParamsLength - 1);
				object[] extrasArray = MethodCaller.GetExtrasArray(num, methodHandle.FinalArrayElementType);
				Array.Copy(array2, extrasArray, num);
				object[] array3 = new object[methodParamsLength];
				for (int i = 0; i <= methodParamsLength - 2; i++)
				{
					array3[i] = parameters[i];
				}
				array3[array3.Length - 1] = extrasArray;
				array2 = array3;
			}
			try
			{
				result = methodHandle.DynamicMethod(obj, array2);
			}
			catch (Exception ex)
			{
				throw new CallMethodException(methodHandle.MethodName + " method call failed.", ex);
			}
			return result;
		}
		private static object[] GetExtrasArray(int count, Type arrayType)
		{
			return (object[])Array.CreateInstance(arrayType.GetElementType(), count);
		}
		/// <summary>
		/// Uses reflection to locate a matching method
		/// on the target object.
		/// </summary>
		/// <param name="objectType">
		/// Type of object containing method.
		/// </param>
		/// <param name="method">
		/// Name of the method.
		/// </param>
		/// <param name="parameters">
		/// Parameters to pass to method.
		/// </param>
		public static MethodInfo GetMethod(Type objectType, string method, params object[] parameters)
		{
			MethodInfo methodInfo = null;
			object[] array = null;
			object[] arg_11_0 = parameters;
			if (parameters == null)
			{
				object[] array2 = new object[1];
				arg_11_0 = array2;
			}
			array = arg_11_0;
			methodInfo = MethodCaller.FindMethod(objectType, method, MethodCaller.GetParameterTypes(array));
			if (methodInfo == null)
			{
				try
				{
					methodInfo = MethodCaller.FindMethod(objectType, method, array.Length);
				}
				catch (AmbiguousMatchException)
				{
					methodInfo = MethodCaller.FindMethodUsingFuzzyMatching(objectType, method, array);
				}
			}
			if (methodInfo == null)
			{
				methodInfo = objectType.GetMethod(method, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.FlattenHierarchy);
			}
			return methodInfo;
		}
		private static MethodInfo FindMethodUsingFuzzyMatching(Type objectType, string method, object[] parameters)
		{
			MethodInfo methodInfo = null;
			Type type = objectType;
			do
			{
				MethodInfo[] methods = type.GetMethods(BindingFlags.DeclaredOnly | BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
				int num = parameters.Length;
				MethodInfo[] array = methods;
				for (int i = 0; i < array.Length; i++)
				{
					MethodInfo methodInfo2 = array[i];
					if (!(methodInfo2.Name != method))
					{
						ParameterInfo[] parameters2 = methodInfo2.GetParameters();
						int num2 = parameters2.Length;
						if (num2 > 0 && ((num2 == 1 && parameters2[0].ParameterType.IsArray) || parameters2[num2 - 1].GetCustomAttributes(typeof(ParamArrayAttribute), true).Length > 0) && num >= num2 - 1)
						{
							methodInfo = methodInfo2;
							break;
						}
					}
				}
				if (methodInfo == null)
				{
					MethodInfo[] array2 = methods;
					for (int j = 0; j < array2.Length; j++)
					{
						MethodInfo methodInfo3 = array2[j];
						if (!(methodInfo3.Name != method) && methodInfo3.GetParameters().Length == num)
						{
							methodInfo = methodInfo3;
							break;
						}
					}
				}
				if (methodInfo != null)
				{
					break;
				}
				type = type.BaseType;
			}
			while (type != null);
			return methodInfo;
		}
		/// <summary>
		/// Returns information about the specified
		/// method, even if the parameter types are
		/// generic and are located in an abstract
		/// generic base class.
		/// </summary>
		/// <param name="objectType">
		/// Type of object containing method.
		/// </param>
		/// <param name="method">
		/// Name of the method.
		/// </param>
		/// <param name="types">
		/// Parameter types to pass to method.
		/// </param>
		public static MethodInfo FindMethod(Type objectType, string method, Type[] types)
		{
			MethodInfo method2;
			do
			{
				method2 = objectType.GetMethod(method, BindingFlags.DeclaredOnly | BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, types, null);
				if (method2 != null)
				{
					break;
				}
				objectType = objectType.BaseType;
			}
			while (objectType != null);
			return method2;
		}
		/// <summary>
		/// Returns information about the specified
		/// method, finding the method based purely
		/// on the method name and number of parameters.
		/// </summary>
		/// <param name="objectType">
		/// Type of object containing method.
		/// </param>
		/// <param name="method">
		/// Name of the method.
		/// </param>
		/// <param name="parameterCount">
		/// Number of parameters to pass to method.
		/// </param>
		public static MethodInfo FindMethod(Type objectType, string method, int parameterCount)
		{
			MethodInfo result = null;
			Type type = objectType;
			MethodInfo method2;
			while (true)
			{
				method2 = type.GetMethod(method, BindingFlags.DeclaredOnly | BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
				if (method2 != null)
				{
					ParameterInfo[] parameters = method2.GetParameters();
					int num = parameters.Length;
					if (num > 0 && ((num == 1 && parameters[0].ParameterType.IsArray) || parameters[num - 1].GetCustomAttributes(typeof(ParamArrayAttribute), true).Length > 0))
					{
						if (parameterCount >= num - 1)
						{
							break;
						}
					}
					else
					{
						if (num == parameterCount)
						{
							goto Block_5;
						}
					}
				}
				type = type.BaseType;
				if (!(type != null))
				{
					return result;
				}
			}
			result = method2;
			return result;
			Block_5:
			result = method2;
			return result;
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
			if (type == null)
			{
				throw new ArgumentNullException("type");
			}
			if (propertyName == null)
			{
				throw new ArgumentNullException("propertyName");
			}
			PropertyInfo property = type.GetProperty(propertyName, BindingFlags.Instance | BindingFlags.Public | BindingFlags.FlattenHierarchy);
			if (property != null)
			{
				return property;
			}
			PropertyInfo[] properties = type.GetProperties();
			for (int i = 0; i < properties.Length; i++)
			{
				PropertyInfo propertyInfo = properties[i];
				if (propertyInfo.Name.Equals(propertyName, StringComparison.OrdinalIgnoreCase))
				{
					return propertyInfo;
				}
			}
			return null;
		}
	}
}
