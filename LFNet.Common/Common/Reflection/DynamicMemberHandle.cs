using System;
using System.Reflection;

namespace LFNet.Common.Reflection
{
    internal class DynamicMemberHandle
    {
        public DynamicMemberHandle(FieldInfo info) : this(info.Name, info.FieldType, DynamicMethodHandlerFactory.CreateFieldGetter(info), DynamicMethodHandlerFactory.CreateFieldSetter(info))
        {
        }

        public DynamicMemberHandle(PropertyInfo info) : this(info.Name, info.PropertyType, DynamicMethodHandlerFactory.CreatePropertyGetter(info), DynamicMethodHandlerFactory.CreatePropertySetter(info))
        {
        }

        public DynamicMemberHandle(string memberName, Type memberType, DynamicMemberGetter dynamicMemberGet, DynamicMemberSetter dynamicMemberSet)
        {
            this.MemberName = memberName;
            this.MemberType = memberType;
            this.DynamicMemberGet = dynamicMemberGet;
            this.DynamicMemberSet = dynamicMemberSet;
        }

        public DynamicMemberGetter DynamicMemberGet { get; private set; }

        public DynamicMemberSetter DynamicMemberSet { get; private set; }

        public string MemberName { get; private set; }

        public Type MemberType { get; private set; }
    }
}

