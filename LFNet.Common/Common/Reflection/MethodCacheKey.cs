using System;

namespace LFNet.Common.Reflection
{
    internal class MethodCacheKey
    {
        private readonly int _hashKey;

        public MethodCacheKey(string typeName, string methodName, Type[] paramTypes)
        {
            this.TypeName = typeName;
            this.MethodName = methodName;
            this.ParamTypes = paramTypes;
            this._hashKey = typeName.GetHashCode();
            this._hashKey ^= methodName.GetHashCode();
            foreach (Type type in paramTypes)
            {
                this._hashKey ^= type.Name.GetHashCode();
            }
        }

        private bool ArrayEquals(Type[] a1, Type[] a2)
        {
            if (a1.Length != a2.Length)
            {
                return false;
            }
            for (int i = 0; i < a1.Length; i++)
            {
                if (a1[i] != a2[i])
                {
                    return false;
                }
            }
            return true;
        }

        public override bool Equals(object obj)
        {
            MethodCacheKey key = obj as MethodCacheKey;
            return (((key != null) && (key.TypeName == this.TypeName)) && ((key.MethodName == this.MethodName) && this.ArrayEquals(key.ParamTypes, this.ParamTypes)));
        }

        public override int GetHashCode()
        {
            return this._hashKey;
        }

        public string MethodName { get; private set; }

        public Type[] ParamTypes { get; private set; }

        public string TypeName { get; private set; }
    }
}

