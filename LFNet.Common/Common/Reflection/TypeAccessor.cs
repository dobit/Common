using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Reflection;

namespace LFNet.Common.Reflection
{
    /// <summary>
    /// A class holding all the accessors for a <see cref="P:LFNet.Common.Reflection.TypeAccessor.Type" />.
    /// </summary>
    internal class TypeAccessor
    {
        private readonly Lazy<LateBoundConstructor> _lateBoundConstructor;
        private readonly ConcurrentDictionary<string, IMemberAccessor> _memberCache;
        private readonly global::System.Type _type;

        /// <summary>
        /// Initializes a new instance of the <see cref="T:LFNet.Common.Reflection.TypeAccessor" /> class.
        /// </summary>
        /// <param name="type">The <see cref="P:LFNet.Common.Reflection.TypeAccessor.Type" /> this accessor is for.</param>
        public TypeAccessor(global::System.Type type)
        {
            Func<LateBoundConstructor> valueFactory = null;
            this._memberCache = new ConcurrentDictionary<string, IMemberAccessor>();
            this._type = type;
            if (valueFactory == null)
            {
                valueFactory = () => DelegateFactory.CreateConstructor(this._type);
            }
            this._lateBoundConstructor = new Lazy<LateBoundConstructor>(valueFactory);
        }

        /// <summary>
        /// Creates a new instance of accessors type.
        /// </summary>
        /// <returns>A new instance of accessors type.</returns>
        public object Create()
        {
            LateBoundConstructor constructor = this._lateBoundConstructor.Value;
            if (constructor == null)
            {
                throw new InvalidOperationException(string.Format("Could not find constructor for '{0}'.", this.Type.Name));
            }
            return constructor();
        }

        private IMemberAccessor CreateFieldAccessor(string name, BindingFlags flags)
        {
            FieldInfo fieldInfo = FindField(this.Type, name, flags);
            if (fieldInfo != null)
            {
                return GetMemberAccessor(fieldInfo);
            }
            return null;
        }

        private IMemberAccessor CreatePropertyAccessor(string name, BindingFlags flags)
        {
            PropertyInfo propertyInfo = FindProperty(this.Type, name, flags);
            if (propertyInfo != null)
            {
                return GetMemberAccessor(propertyInfo);
            }
            return null;
        }

        /// <summary>
        /// Searches for the specified field with the specified name.
        /// </summary>
        /// <param name="name">The name of the field to find.</param>
        /// <returns>
        /// An <see cref="T:LFNet.Common.Reflection.IMemberAccessor" /> instance for the field if found; otherwise <c>null</c>.
        /// </returns>
        public IMemberAccessor FindField(string name)
        {
            return this.FindField(name, BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance);
        }

        /// <summary>
        /// Searches for the specified field, using the specified binding constraints.
        /// </summary>
        /// <param name="name">The name of the field to find.</param>
        /// <param name="flags">A bitmask comprised of one or more <see cref="T:System.Reflection.BindingFlags" /> that specify how the search is conducted.</param>
        /// <returns>
        /// An <see cref="T:LFNet.Common.Reflection.IMemberAccessor" /> instance for the field if found; otherwise <c>null</c>.
        /// </returns>
        public IMemberAccessor FindField(string name, BindingFlags flags)
        {
            return this._memberCache.GetOrAdd(name, n => this.CreateFieldAccessor(n, flags));
        }

        private static FieldInfo FindField(global::System.Type type, string name, BindingFlags flags)
        {
            if (type == null)
            {
                throw new ArgumentNullException("type");
            }
            if (name == null)
            {
                throw new ArgumentNullException("name");
            }
            FieldInfo field = type.GetField(name, flags);
            if (field != null)
            {
                return field;
            }
            return type.GetFields(flags).FirstOrDefault<FieldInfo>(f => f.Name.Equals(name, StringComparison.OrdinalIgnoreCase));
        }

        /// <summary>
        /// Searches for the public property with the specified name.
        /// </summary>
        /// <param name="name">The name of the property to find.</param>
        /// <returns>An <see cref="T:LFNet.Common.Reflection.IMemberAccessor" /> instance for the property if found; otherwise <c>null</c>.</returns>
        public IMemberAccessor FindProperty(string name)
        {
            return this.FindProperty(name, BindingFlags.FlattenHierarchy | BindingFlags.Public | BindingFlags.Instance);
        }

        /// <summary>
        /// Searches for the specified property, using the specified binding constraints.
        /// </summary>
        /// <param name="name">The name of the property to find.</param>
        /// <param name="flags">A bitmask comprised of one or more <see cref="T:System.Reflection.BindingFlags" /> that specify how the search is conducted.</param>
        /// <returns>
        /// An <see cref="T:LFNet.Common.Reflection.IMemberAccessor" /> instance for the property if found; otherwise <c>null</c>.
        /// </returns>
        public IMemberAccessor FindProperty(string name, BindingFlags flags)
        {
            return this._memberCache.GetOrAdd(name, n => this.CreatePropertyAccessor(n, flags));
        }

        private static PropertyInfo FindProperty(global::System.Type type, string name, BindingFlags flags)
        {
            if (type == null)
            {
                throw new ArgumentNullException("type");
            }
            if (name == null)
            {
                throw new ArgumentNullException("name");
            }
            PropertyInfo property = type.GetProperty(name, flags);
            if (property != null)
            {
                return property;
            }
            return type.GetProperties(flags).FirstOrDefault<PropertyInfo>(p => p.Name.Equals(name, StringComparison.OrdinalIgnoreCase));
        }

        private static IMemberAccessor GetMemberAccessor(FieldInfo fieldInfo)
        {
            if (fieldInfo != null)
            {
                return new FieldAccessor(fieldInfo);
            }
            return null;
        }

        private static IMemberAccessor GetMemberAccessor(PropertyInfo propertyInfo)
        {
            if (propertyInfo != null)
            {
                return new PropertyAccessor(propertyInfo);
            }
            return null;
        }

        /// <summary>
        /// Gets the <see cref="P:LFNet.Common.Reflection.TypeAccessor.Type" /> this accessor is for.
        /// </summary>
        /// <value>The <see cref="P:LFNet.Common.Reflection.TypeAccessor.Type" /> this accessor is for.</value>
        public global::System.Type Type
        {
            get
            {
                return this._type;
            }
        }
    }
}

