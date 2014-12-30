using System;
using System.Reflection;

namespace LFNet.Common.Reflection
{
    internal class DynamicMethodHandle
    {
        public DynamicMethodHandle(MethodInfo info, params object[] parameters)
        {
            if (info == null)
            {
                this.DynamicMethod = null;
            }
            else
            {
                this.MethodName = info.Name;
                ParameterInfo[] infoArray = info.GetParameters();
                int length = infoArray.Length;
                if ((length > 0) && (((length == 1) && infoArray[0].ParameterType.IsArray) || (infoArray[length - 1].GetCustomAttributes(typeof(ParamArrayAttribute), true).Length > 0)))
                {
                    this.HasFinalArrayParam = true;
                    this.MethodParamsLength = length;
                    this.FinalArrayElementType = infoArray[length - 1].ParameterType;
                }
                this.DynamicMethod = DynamicMethodHandlerFactory.CreateMethod(info);
            }
        }

        public DynamicMemberMethod DynamicMethod { get; private set; }

        public Type FinalArrayElementType { get; private set; }

        public bool HasFinalArrayParam { get; private set; }

        public string MethodName { get; private set; }

        public int MethodParamsLength { get; private set; }
    }
}

