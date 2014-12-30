using System;
using System.Collections.Concurrent;
using System.Reflection;

namespace LFNet.Common.Reflection
{
    /// <summary>
    /// A class for late bound operations on a type.
    /// </summary>
    public static class LateBinder
    {
        private static readonly ConcurrentDictionary<Type, TypeAccessor> _accessorCache = new ConcurrentDictionary<Type, TypeAccessor>();

        /// <summary>
        /// Creates an instance of the specified type.
        /// </summary>
        /// <param name="type">The type to create.</param>
        /// <returns>A new instance of the specified type.</returns>
        public static object CreateInstance(Type type)
        {
            if (type == null)
            {
                throw new ArgumentNullException("type");
            }
            TypeAccessor accessor = GetAccessor(type);
            if (accessor == null)
            {
                throw new InvalidOperationException(string.Format("Could not find constructor for {0}.", type.Name));
            }
            return accessor.Create();
        }

        /// <summary>
        /// Searches for the field with the specified name.
        /// </summary>
        /// <param name="type">The <see cref="T:System.Type" /> to search for the field in.</param>
        /// <param name="name">The name of the field to find.</param>
        /// <returns>
        /// An <see cref="T:LFNet.Common.Reflection.IMemberAccessor" /> instance for the field if found; otherwise <c>null</c>.
        /// </returns>
        public static IMemberAccessor FindField(Type type, string name)
        {
            return FindField(type, name, BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance);
        }

        /// <summary>
        /// Searches for the field, using the specified binding constraints.
        /// </summary>
        /// <param name="type">The <see cref="T:System.Type" /> to search for the field in.</param>
        /// <param name="name">The name of the field to find.</param>
        /// <param name="flags">A bitmask comprised of one or more <see cref="T:System.Reflection.BindingFlags" /> that specify how the search is conducted.</param>
        /// <returns>
        /// An <see cref="T:LFNet.Common.Reflection.IMemberAccessor" /> instance for the field if found; otherwise <c>null</c>.
        /// </returns>
        public static IMemberAccessor FindField(Type type, string name, BindingFlags flags)
        {
            if (type == null)
            {
                throw new ArgumentNullException("type");
            }
            if (string.IsNullOrEmpty(name))
            {
                throw new ArgumentNullException("name");
            }
            return GetAccessor(type).FindField(name, flags);
        }

        /// <summary>
        /// Searches for the public property with the specified name.
        /// </summary>
        /// <param name="type">The <see cref="T:System.Type" /> to search for the property in.</param>
        /// <param name="name">The name of the property to find.</param>
        /// <returns>
        /// An <see cref="T:LFNet.Common.Reflection.IMemberAccessor" /> instance for the property if found; otherwise <c>null</c>.
        /// </returns>
        public static IMemberAccessor FindProperty(Type type, string name)
        {
            return FindProperty(type, name, BindingFlags.FlattenHierarchy | BindingFlags.Public | BindingFlags.Instance);
        }

        /// <summary>
        /// Searches for the specified property, using the specified binding constraints.
        /// </summary>
        /// <param name="type">The <see cref="T:System.Type" /> to search for the property in.</param>
        /// <param name="name">The name of the property to find.</param>
        /// <param name="flags">A bitmask comprised of one or more <see cref="T:System.Reflection.BindingFlags" /> that specify how the search is conducted.</param>
        /// <returns>
        /// An <see cref="T:LFNet.Common.Reflection.IMemberAccessor" /> instance for the property if found; otherwise <c>null</c>.
        /// </returns>
        public static IMemberAccessor FindProperty(Type type, string name, BindingFlags flags)
        {
            if (type == null)
            {
                throw new ArgumentNullException("type");
            }
            if (string.IsNullOrEmpty(name))
            {
                throw new ArgumentNullException("name");
            }
            Type memberType = type;
            IMemberAccessor accessor2 = null;
            foreach (string str in name.Split(new char[] { '.' }))
            {
                if (accessor2 != null)
                {
                    memberType = accessor2.MemberType;
                }
                accessor2 = GetAccessor(memberType).FindProperty(str, flags);
            }
            return accessor2;
        }

        private static TypeAccessor GetAccessor(Type type)
        {
            return _accessorCache.GetOrAdd(type, t => new TypeAccessor(t));
        }

        /// <summary>
        /// Returns the value of the field with the specified name.
        /// </summary>
        /// <param name="target">The object whose field value will be returned.</param>
        /// <param name="name">The name of the field to read.</param>
        /// <returns>The value of the field.</returns>
        public static object GetField(object target, string name)
        {
            if (target == null)
            {
                throw new ArgumentNullException("target");
            }
            if (string.IsNullOrEmpty(name))
            {
                throw new ArgumentNullException("name");
            }
            Type type = target.GetType();
            IMemberAccessor accessor = FindField(type, name);
            if (accessor == null)
            {
                throw new InvalidOperationException(string.Format("Could not find field '{0}' in type '{1}'.", name, type.Name));
            }
            return accessor.GetValue(target);
        }

        /// <summary>
        /// Returns the value of the property with the specified name.
        /// </summary>
        /// <param name="target">The object whose property value will be returned.</param>
        /// <param name="name">The name of the property to read.</param>
        /// <returns>The value of the property.</returns>
        public static object GetProperty(object target, string name)
        {
            if (target == null)
            {
                throw new ArgumentNullException("target");
            }
            if (string.IsNullOrEmpty(name))
            {
                throw new ArgumentNullException("name");
            }
            Type type = target.GetType();
            Type memberType = type;
            object instance = target;
            IMemberAccessor accessor2 = null;
            foreach (string str in name.Split(new char[] { '.' }))
            {
                if (accessor2 != null)
                {
                    instance = accessor2.GetValue(instance);
                    memberType = accessor2.MemberType;
                }
                accessor2 = GetAccessor(memberType).FindProperty(str);
            }
            if (accessor2 == null)
            {
                throw new InvalidOperationException(string.Format("Could not find property '{0}' in type '{1}'.", name, type.Name));
            }
            return accessor2.GetValue(instance);
        }

        /// <summary>
        /// Sets the field value with the specified name.
        /// </summary>
        /// <param name="target">The object whose field value will be set.</param>
        /// <param name="name">The name of the field to set.</param>
        /// <param name="value">The new value to be set.</param>
        public static void SetField(object target, string name, object value)
        {
            if (target == null)
            {
                throw new ArgumentNullException("target");
            }
            if (string.IsNullOrEmpty(name))
            {
                throw new ArgumentNullException("name");
            }
            Type type = target.GetType();
            IMemberAccessor accessor = FindField(type, name);
            if (accessor == null)
            {
                throw new InvalidOperationException(string.Format("Could not find field '{0}' in type '{1}'.", name, type.Name));
            }
            accessor.SetValue(target, value);
        }

        /// <summary>
        /// Sets the property value with the specified name.
        /// </summary>
        /// <param name="target">The object whose property value will be set.</param>
        /// <param name="name">The name of the property to set.</param>
        /// <param name="value">The new value to be set.</param>
        public static void SetProperty(object target, string name, object value)
        {
            if (target == null)
            {
                throw new ArgumentNullException("target");
            }
            if (string.IsNullOrEmpty(name))
            {
                throw new ArgumentNullException("name");
            }
            Type type = target.GetType();
            Type memberType = type;
            object instance = target;
            IMemberAccessor accessor2 = null;
            foreach (string str in name.Split(new char[] { '.' }))
            {
                if (accessor2 != null)
                {
                    instance = accessor2.GetValue(instance);
                    memberType = accessor2.MemberType;
                }
                accessor2 = GetAccessor(memberType).FindProperty(str);
            }
            if (accessor2 == null)
            {
                throw new InvalidOperationException(string.Format("Could not find property '{0}' in type '{1}'.", name, type.Name));
            }
            accessor2.SetValue(instance, value);
        }
    }
}

