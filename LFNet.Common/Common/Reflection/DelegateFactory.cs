using System;
using System.Reflection;
using System.Reflection.Emit;
using LFNet.Common.Extensions;

namespace LFNet.Common.Reflection
{
    public static class DelegateFactory
    {
        public static LateBoundConstructor CreateConstructor(Type type)
        {
            DynamicMethod method = CreateDynamicMethod("Create" + type.FullName, typeof(object), Type.EmptyTypes, type);
            method.InitLocals = true;
            ILGenerator iLGenerator = method.GetILGenerator();
            if (type.IsValueType)
            {
                iLGenerator.DeclareLocal(type);
                iLGenerator.Emit(OpCodes.Ldloc_0);
                iLGenerator.Emit(OpCodes.Box, type);
            }
            else
            {
                ConstructorInfo con = type.GetConstructor(BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance, null, Type.EmptyTypes, null);
                if (con == null)
                {
                    throw new InvalidOperationException(string.Format("Could not get constructor for {0}.", type));
                }
                iLGenerator.Emit(OpCodes.Newobj, con);
            }
            iLGenerator.Return();
            return (LateBoundConstructor) method.CreateDelegate(typeof(LateBoundConstructor));
        }

        private static DynamicMethod CreateDynamicMethod(string name, Type returnType, Type[] parameterTypes, Type owner)
        {
            return (!owner.IsInterface ? new DynamicMethod(name, returnType, parameterTypes, owner, true) : new DynamicMethod(name, returnType, parameterTypes, owner.Assembly.ManifestModule, true));
        }

        public static LateBoundGet CreateGet(FieldInfo fieldInfo)
        {
            DynamicMethod method = CreateDynamicMethod("Get" + fieldInfo.Name, typeof(object), new Type[] { typeof(object) }, fieldInfo.DeclaringType);
            ILGenerator iLGenerator = method.GetILGenerator();
            if (!fieldInfo.IsStatic)
            {
                iLGenerator.PushInstance(fieldInfo.DeclaringType);
            }
            iLGenerator.Emit(OpCodes.Ldfld, fieldInfo);
            iLGenerator.BoxIfNeeded(fieldInfo.FieldType);
            iLGenerator.Return();
            return (LateBoundGet) method.CreateDelegate(typeof(LateBoundGet));
        }

        public static LateBoundGet CreateGet(PropertyInfo propertyInfo)
        {
            MethodInfo getMethod = propertyInfo.GetGetMethod(true);
            if (getMethod == null)
            {
                throw new InvalidOperationException(string.Format("Property '{0}' does not have a getter.", propertyInfo.Name));
            }
            DynamicMethod method = CreateDynamicMethod("Get" + propertyInfo.Name, typeof(object), new Type[] { typeof(object) }, propertyInfo.DeclaringType);
            ILGenerator iLGenerator = method.GetILGenerator();
            if (!getMethod.IsStatic)
            {
                iLGenerator.PushInstance(propertyInfo.DeclaringType);
            }
            iLGenerator.CallMethod(getMethod);
            iLGenerator.BoxIfNeeded(propertyInfo.PropertyType);
            iLGenerator.Return();
            return (LateBoundGet) method.CreateDelegate(typeof(LateBoundGet));
        }

        public static LateBoundMethod CreateMethod(MethodBase method)
        {
            DynamicMethod method2 = CreateDynamicMethod(method.ToString(), typeof(object), new Type[] { typeof(object), typeof(object[]) }, method.DeclaringType);
            ILGenerator iLGenerator = method2.GetILGenerator();
            ParameterInfo[] parameters = method.GetParameters();
            Label label = iLGenerator.DefineLabel();
            iLGenerator.Emit(OpCodes.Ldarg_1);
            iLGenerator.Emit(OpCodes.Ldlen);
            iLGenerator.Emit(OpCodes.Ldc_I4, parameters.Length);
            iLGenerator.Emit(OpCodes.Beq, label);
            iLGenerator.Emit(OpCodes.Newobj, typeof(TargetParameterCountException).GetConstructor(Type.EmptyTypes));
            iLGenerator.Emit(OpCodes.Throw);
            iLGenerator.MarkLabel(label);
            if (!method.IsConstructor && !method.IsStatic)
            {
                iLGenerator.PushInstance(method.DeclaringType);
            }
            for (int i = 0; i < parameters.Length; i++)
            {
                iLGenerator.Emit(OpCodes.Ldarg_1);
                iLGenerator.Emit(OpCodes.Ldc_I4, i);
                iLGenerator.Emit(OpCodes.Ldelem_Ref);
                iLGenerator.UnboxIfNeeded(parameters[i].ParameterType);
            }
            if (method.IsConstructor)
            {
                iLGenerator.Emit(OpCodes.Newobj, (ConstructorInfo) method);
            }
            else if (method.IsFinal || !method.IsVirtual)
            {
                iLGenerator.CallMethod((MethodInfo) method);
            }
            Type type = method.IsConstructor ? method.DeclaringType : ((MethodInfo) method).ReturnType;
            if (type != typeof(void))
            {
                iLGenerator.BoxIfNeeded(type);
            }
            else
            {
                iLGenerator.Emit(OpCodes.Ldnull);
            }
            iLGenerator.Return();
            return (LateBoundMethod) method2.CreateDelegate(typeof(LateBoundMethod));
        }

        public static LateBoundSet CreateSet(FieldInfo fieldInfo)
        {
            DynamicMethod method = CreateDynamicMethod("Set" + fieldInfo.Name, null, new Type[] { typeof(object), typeof(object) }, fieldInfo.DeclaringType);
            ILGenerator iLGenerator = method.GetILGenerator();
            if (!fieldInfo.IsStatic)
            {
                iLGenerator.PushInstance(fieldInfo.DeclaringType);
            }
            iLGenerator.Emit(OpCodes.Ldarg_1);
            iLGenerator.UnboxIfNeeded(fieldInfo.FieldType);
            iLGenerator.Emit(OpCodes.Stfld, fieldInfo);
            iLGenerator.Return();
            return (LateBoundSet) method.CreateDelegate(typeof(LateBoundSet));
        }

        public static LateBoundSet CreateSet(PropertyInfo propertyInfo)
        {
            MethodInfo setMethod = propertyInfo.GetSetMethod(true);
            if (setMethod == null)
            {
                throw new InvalidOperationException(string.Format("Property '{0}' does not have a setter.", propertyInfo.Name));
            }
            DynamicMethod method = CreateDynamicMethod("Set" + propertyInfo.Name, null, new Type[] { typeof(object), typeof(object) }, propertyInfo.DeclaringType);
            ILGenerator iLGenerator = method.GetILGenerator();
            if (!setMethod.IsStatic)
            {
                iLGenerator.PushInstance(propertyInfo.DeclaringType);
            }
            iLGenerator.Emit(OpCodes.Ldarg_1);
            iLGenerator.UnboxIfNeeded(propertyInfo.PropertyType);
            iLGenerator.CallMethod(setMethod);
            iLGenerator.Return();
            return (LateBoundSet) method.CreateDelegate(typeof(LateBoundSet));
        }
    }
}

