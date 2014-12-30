using System;
using System.Reflection;

namespace LFNet.Common.Reflection
{
    /// <summary>
    /// An accessor class for <see cref="T:System.Reflection.FieldInfo" />.
    /// </summary>
    internal class FieldAccessor : MemberAccessor
    {
        private readonly FieldInfo _fieldInfo;
        private readonly bool _hasGetter;
        private readonly bool _hasSetter;
        private readonly Lazy<LateBoundGet> _lateBoundGet;
        private readonly Lazy<LateBoundSet> _lateBoundSet;
        private readonly Type _memberType;
        private readonly string _name;

        /// <summary>
        /// Initializes a new instance of the <see cref="T:LFNet.Common.Reflection.FieldAccessor" /> class.
        /// </summary>
        /// <param name="fieldInfo">The <see cref="T:System.Reflection.FieldInfo" /> instance to use for this accessor.</param>
        public FieldAccessor(FieldInfo fieldInfo)
        {
            Func<LateBoundGet> valueFactory = null;
            Func<LateBoundSet> func2 = null;
            this._fieldInfo = fieldInfo;
            this._name = fieldInfo.Name;
            this._memberType = fieldInfo.FieldType;
            this._hasGetter = true;
            if (valueFactory == null)
            {
                valueFactory = () => DelegateFactory.CreateGet(this._fieldInfo);
            }
            this._lateBoundGet = new Lazy<LateBoundGet>(valueFactory);
            this._hasSetter = !fieldInfo.IsInitOnly && !fieldInfo.IsLiteral;
            if (this._hasSetter)
            {
                if (func2 == null)
                {
                    func2 = () => DelegateFactory.CreateSet(this._fieldInfo);
                }
                this._lateBoundSet = new Lazy<LateBoundSet>(func2);
            }
        }

        /// <summary>
        /// Returns the value of the member.
        /// </summary>
        /// <param name="instance">The object whose member value will be returned.</param>
        /// <returns>
        /// The member value for the instance parameter.
        /// </returns>
        public override object GetValue(object instance)
        {
            if ((this._lateBoundGet == null) || !this.HasGetter)
            {
                throw new InvalidOperationException(string.Format("Field '{0}' does not have a getter.", this.Name));
            }
            LateBoundGet get = this._lateBoundGet.Value;
            if (get == null)
            {
                throw new InvalidOperationException(string.Format("Field '{0}' does not have a getter.", this.Name));
            }
            return get(instance);
        }

        /// <summary>
        /// Sets the value of the member.
        /// </summary>
        /// <param name="instance">The object whose member value will be set.</param>
        /// <param name="value">The new value for this member.</param>
        public override void SetValue(object instance, object value)
        {
            if ((this._lateBoundSet == null) || !this.HasSetter)
            {
                throw new InvalidOperationException(string.Format("Field '{0}' does not have a setter.", this.Name));
            }
            LateBoundSet set = this._lateBoundSet.Value;
            if (set == null)
            {
                throw new InvalidOperationException(string.Format("Field '{0}' does not have a setter.", this.Name));
            }
            set(instance, value);
        }

        /// <summary>
        /// Gets a value indicating whether this member has getter.
        /// </summary>
        /// <value><c>true</c> if this member has getter; otherwise, <c>false</c>.</value>
        public override bool HasGetter
        {
            get
            {
                return this._hasGetter;
            }
        }

        /// <summary>
        /// Gets a value indicating whether this member has setter.
        /// </summary>
        /// <value><c>true</c> if this member has setter; otherwise, <c>false</c>.</value>
        public override bool HasSetter
        {
            get
            {
                return this._hasSetter;
            }
        }

        /// <summary>
        /// Gets the member info.
        /// </summary>
        /// <value>The member info.</value>
        public override global::System.Reflection.MemberInfo MemberInfo
        {
            get
            {
                return this._fieldInfo;
            }
        }

        /// <summary>
        /// Gets the type of the member.
        /// </summary>
        /// <value>The type of the member.</value>
        public override Type MemberType
        {
            get
            {
                return this._memberType;
            }
        }

        /// <summary>
        /// Gets the name of the member.
        /// </summary>
        /// <value>The name of the member.</value>
        public override string Name
        {
            get
            {
                return this._name;
            }
        }
    }
}

