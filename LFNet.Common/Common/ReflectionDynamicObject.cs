
#if NET4
using System.Dynamic;
using System.Globalization;
using System.Reflection;

namespace LFNet.Common
{
    public sealed class ReflectionDynamicObject : DynamicObject
    {
        private object RealObject { get; set; }

        public override bool TryConvert(ConvertBinder binder, out object result)
        {
            result = this.RealObject;
            return true;
        }

        public override bool TryGetMember(GetMemberBinder binder, out object result)
        {
            PropertyInfo property = this.RealObject.GetType().GetProperty(binder.Name, BindingFlags.GetProperty |
                                                                                       BindingFlags.Public |
                                                                                       BindingFlags.Instance);
            if (property == null)
            {
                result = null;
            }
            else
            {
                result = property.GetValue(this.RealObject, null);
                result = WrapObjectIfInternal(result);
            }
            return true;
        }

        public override bool TryInvokeMember(InvokeMemberBinder binder, object[] args, out object result)
        {
            result = this.RealObject.GetType().InvokeMember(binder.Name,
                                                            BindingFlags.InvokeMethod | BindingFlags.NonPublic |
                                                            BindingFlags.Public | BindingFlags.Instance, null,
                                                            this.RealObject, args, CultureInfo.InvariantCulture);
            return true;
        }

        public static object WrapObjectIfInternal(object o)
        {
            if (o == null) return null;
            if (o.GetType().IsPublic) return o;
            return new ReflectionDynamicObject {RealObject = o};
        }

        public override string ToString()
        {
            return this.RealObject.ToString();
        }
    }

}
#endif