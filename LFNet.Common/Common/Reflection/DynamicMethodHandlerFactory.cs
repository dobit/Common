using System;
using System.Reflection;
using System.Reflection.Emit;

namespace LFNet.Common.Reflection
{
    internal static class DynamicMethodHandlerFactory
    {
        public static DynamicConstructor CreateConstructor(ConstructorInfo constructor)
        {
            if (constructor == null)
            {
                throw new ArgumentNullException("constructor");
            }
            if (constructor.GetParameters().Length > 0)
            {
                throw new NotSupportedException("Constructor with parameters are not supported.");
            }
            DynamicMethod dynamicMethod = new DynamicMethod("ctor", MethodAttributes.FamANDAssem | MethodAttributes.Family | MethodAttributes.Static, CallingConventions.Standard, constructor.DeclaringType, Type.EmptyTypes, (Type)null, true);
            ILGenerator iLGenerator = dynamicMethod.GetILGenerator();
            iLGenerator.Emit(OpCodes.Nop);
            iLGenerator.Emit(OpCodes.Newobj, constructor);
            iLGenerator.Emit(OpCodes.Ret);
            return (DynamicConstructor)dynamicMethod.CreateDelegate(typeof(DynamicConstructor));
        }

        public static DynamicMemberGetter CreateFieldGetter(FieldInfo field)
        {
            if (field == null)
            {
                throw new ArgumentNullException("field");
            }
            DynamicMethod method = new DynamicMethod("fldg", typeof(object), new Type[] { typeof(object) }, field.DeclaringType, true);
            ILGenerator iLGenerator = method.GetILGenerator();
            if (!field.IsStatic)
            {
                iLGenerator.Emit(OpCodes.Ldarg_0);
                EmitCastToReference(iLGenerator, field.DeclaringType);
                iLGenerator.Emit(OpCodes.Ldfld, field);
            }
            else
            {
                iLGenerator.Emit(OpCodes.Ldsfld, field);
            }
            if (field.FieldType.IsValueType)
            {
                iLGenerator.Emit(OpCodes.Box, field.FieldType);
            }
            iLGenerator.Emit(OpCodes.Ret);
            return (DynamicMemberGetter) method.CreateDelegate(typeof(DynamicMemberGetter));
        }

        public static DynamicMemberSetter CreateFieldSetter(FieldInfo field)
        {
            if (field == null)
            {
                throw new ArgumentNullException("field");
            }
            DynamicMethod method = new DynamicMethod("flds", null, new Type[] { typeof(object), typeof(object) }, field.DeclaringType, true);
            ILGenerator iLGenerator = method.GetILGenerator();
            if (!field.IsStatic)
            {
                iLGenerator.Emit(OpCodes.Ldarg_0);
            }
            iLGenerator.Emit(OpCodes.Ldarg_1);
            EmitCastToReference(iLGenerator, field.FieldType);
            if (!field.IsStatic)
            {
                iLGenerator.Emit(OpCodes.Stfld, field);
            }
            else
            {
                iLGenerator.Emit(OpCodes.Stsfld, field);
            }
            iLGenerator.Emit(OpCodes.Ret);
            return (DynamicMemberSetter) method.CreateDelegate(typeof(DynamicMemberSetter));
        }

        public static DynamicMemberMethod CreateMethod(MethodInfo method)
        {
            ParameterInfo[] parameters = method.GetParameters();
            DynamicMethod method2 = new DynamicMethod("dm", typeof(object), new Type[] { typeof(object), typeof(object[]) }, typeof(DynamicMethodHandlerFactory), true);
            ILGenerator iLGenerator = method2.GetILGenerator();
            iLGenerator.Emit(OpCodes.Ldarg_0);
            for (int i = 0; i < parameters.Length; i++)
            {
                iLGenerator.Emit(OpCodes.Ldarg_1);
                iLGenerator.Emit(OpCodes.Ldc_I4, i);
                Type parameterType = parameters[i].ParameterType;
                if (parameterType.IsByRef)
                {
                    parameterType = parameterType.GetElementType();
                    if (parameterType.IsValueType)
                    {
                        iLGenerator.Emit(OpCodes.Ldelem_Ref);
                        iLGenerator.Emit(OpCodes.Unbox, parameterType);
                    }
                    else
                    {
                        iLGenerator.Emit(OpCodes.Ldelema, parameterType);
                    }
                }
                else
                {
                    iLGenerator.Emit(OpCodes.Ldelem_Ref);
                    if (parameterType.IsValueType)
                    {
                        iLGenerator.Emit(OpCodes.Unbox, parameterType);
                        iLGenerator.Emit(OpCodes.Ldobj, parameterType);
                    }
                }
            }
            if ((method.IsAbstract || method.IsVirtual) && (!method.IsFinal && !method.DeclaringType.IsSealed))
            {
                iLGenerator.Emit(OpCodes.Callvirt, method);
            }
            else
            {
                iLGenerator.Emit(OpCodes.Call, method);
            }
            if (method.ReturnType == typeof(void))
            {
                iLGenerator.Emit(OpCodes.Ldnull);
            }
            else if (method.ReturnType.IsValueType)
            {
                iLGenerator.Emit(OpCodes.Box, method.ReturnType);
            }
            iLGenerator.Emit(OpCodes.Ret);
            return (DynamicMemberMethod) method2.CreateDelegate(typeof(DynamicMemberMethod));
        }

        public static DynamicMemberGetter CreatePropertyGetter(PropertyInfo property)
        {
            if (property == null)
            {
                throw new ArgumentNullException("property");
            }
            if (!property.CanRead)
            {
                return null;
            }
            MethodInfo getMethod = property.GetGetMethod();
            if (getMethod == null)
            {
                getMethod = property.GetGetMethod(true);
            }
            DynamicMethod method = new DynamicMethod("propg", typeof(object), new Type[] { typeof(object) }, property.DeclaringType, true);
            ILGenerator iLGenerator = method.GetILGenerator();
            if (!getMethod.IsStatic)
            {
                iLGenerator.Emit(OpCodes.Ldarg_0);
                iLGenerator.EmitCall(OpCodes.Callvirt, getMethod, null);
            }
            else
            {
                iLGenerator.EmitCall(OpCodes.Call, getMethod, null);
            }
            if (property.PropertyType.IsValueType)
            {
                iLGenerator.Emit(OpCodes.Box, property.PropertyType);
            }
            iLGenerator.Emit(OpCodes.Ret);
            return (DynamicMemberGetter) method.CreateDelegate(typeof(DynamicMemberGetter));
        }

        public static DynamicMemberSetter CreatePropertySetter(PropertyInfo property)
        {
            if (property == null)
            {
                throw new ArgumentNullException("property");
            }
            if (!property.CanWrite)
            {
                return null;
            }
            MethodInfo setMethod = property.GetSetMethod();
            if (setMethod == null)
            {
                setMethod = property.GetSetMethod(true);
            }
            DynamicMethod method = new DynamicMethod("props", null, new Type[] { typeof(object), typeof(object) }, property.DeclaringType, true);
            ILGenerator iLGenerator = method.GetILGenerator();
            if (!setMethod.IsStatic)
            {
                iLGenerator.Emit(OpCodes.Ldarg_0);
            }
            iLGenerator.Emit(OpCodes.Ldarg_1);
            EmitCastToReference(iLGenerator, property.PropertyType);
            if (!setMethod.IsStatic && !property.DeclaringType.IsValueType)
            {
                iLGenerator.EmitCall(OpCodes.Callvirt, setMethod, null);
            }
            else
            {
                iLGenerator.EmitCall(OpCodes.Call, setMethod, null);
            }
            iLGenerator.Emit(OpCodes.Ret);
            return (DynamicMemberSetter) method.CreateDelegate(typeof(DynamicMemberSetter));
        }

        private static void EmitCastToReference(ILGenerator il, Type type)
        {
            if (type.IsValueType)
            {
                il.Emit(OpCodes.Unbox_Any, type);
            }
            else
            {
                il.Emit(OpCodes.Castclass, type);
            }
        }
    }
}

