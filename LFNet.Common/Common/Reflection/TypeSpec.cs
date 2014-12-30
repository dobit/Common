using System;
using System.Collections.Generic;
using System.Linq;

namespace LFNet.Common.Reflection
{
    /// <summary>
    /// A class to parse an assembly-qualified name of a Type into its parts.
    /// </summary>
    public class TypeSpec
    {
        private List<ArraySpec> array_spec;
        private string assembly_name;
        private List<TypeSpec> generic_params;
        private bool is_byref;
        private string name;
        private List<string> nested;
        private int pointer_level;

        private void AddArray(ArraySpec array)
        {
            if (this.array_spec == null)
            {
                this.array_spec = new List<ArraySpec>();
            }
            this.array_spec.Add(array);
        }

        private void AddName(string type_name)
        {
            if (this.name == null)
            {
                this.name = type_name;
            }
            else
            {
                if (this.nested == null)
                {
                    this.nested = new List<string>();
                }
                this.nested.Add(type_name);
            }
        }

        public string GetAssemblyName()
        {
            return this.assembly_name;
        }

        public string GetFullName()
        {
            string name = this.name;
            if (this.nested != null)
            {
                foreach (string str2 in this.nested)
                {
                    name = name + "+" + str2;
                }
            }
            if (this.generic_params != null)
            {
                name = name + "[";
                for (int j = 0; j < this.generic_params.Count; j++)
                {
                    if (j > 0)
                    {
                        name = name + ", ";
                    }
                    if (this.generic_params[j].assembly_name != null)
                    {
                        object obj2 = name;
                        name = string.Concat(new object[] { obj2, "[", this.generic_params[j], "]" });
                    }
                    else
                    {
                        name = name + this.generic_params[j];
                    }
                }
                name = name + "]";
            }
            if (this.array_spec != null)
            {
                foreach (ArraySpec spec in this.array_spec)
                {
                    name = name + spec;
                }
            }
            for (int i = 0; i < this.pointer_level; i++)
            {
                name = name + "*";
            }
            if (this.is_byref)
            {
                name = name + "&";
            }
            return name;
        }

        public TypeSpec[] GetGenericParameters()
        {
            return this.generic_params.ToArray();
        }

        public string GetName()
        {
            if ((this.nested != null) && (this.nested.Count > 0))
            {
                return this.nested.Last<string>();
            }
            if (string.IsNullOrEmpty(this.name))
            {
                return string.Empty;
            }
            return (from s in this.name.Split(new char[] { Type.Delimiter }) select s.Trim()).Last<string>();
        }

        public bool IsArray()
        {
            return (this.array_spec != null);
        }

        public bool IsGenericType()
        {
            return (this.generic_params != null);
        }

        /// <summary>
        /// Parses an assembly-qualified name of a Type into its parts.
        /// </summary>
        /// <param name="typeName">The assembly-qualified name of the Type.</param>
        /// <returns></returns>
        public static TypeSpec Parse(string typeName)
        {
            int p = 0;
            if (typeName == null)
            {
                throw new ArgumentNullException("typeName");
            }
            TypeSpec spec = Parse(typeName, ref p, false, false);
            if (p < typeName.Length)
            {
                throw new ArgumentException("Count not parse the whole type name", "typeName");
            }
            return spec;
        }

        private static TypeSpec Parse(string name, ref int p, bool is_recurse, bool allow_aqn)
        {
            int pos = p;
            bool flag = false;
            TypeSpec spec = new TypeSpec();
            SkipSpace(name, ref pos);
            int startIndex = pos;
            while (pos < name.Length)
            {
                switch (name[pos])
                {
                    case '&':
                    case '*':
                    case '[':
                        if ((name[pos] != '[') && is_recurse)
                        {
                            throw new ArgumentException("Generic argument can't be byref or pointer type", "typeName");
                        }
                        spec.AddName(name.Substring(startIndex, pos - startIndex));
                        startIndex = pos + 1;
                        flag = true;
                        break;

                    case '+':
                        spec.AddName(name.Substring(startIndex, pos - startIndex));
                        startIndex = pos + 1;
                        break;

                    case ',':
                    case ']':
                        spec.AddName(name.Substring(startIndex, pos - startIndex));
                        startIndex = pos + 1;
                        flag = true;
                        if (!is_recurse || allow_aqn)
                        {
                            break;
                        }
                        p = pos;
                        return spec;
                }
                if (flag)
                {
                    break;
                }
                pos++;
            }
            if (startIndex < pos)
            {
                spec.AddName(name.Substring(startIndex, pos - startIndex));
            }
            if (flag)
            {
                while (pos < name.Length)
                {
                    int num3;
                    int num4;
                    switch (name[pos])
                    {
                        case '*':
                            if (spec.is_byref)
                            {
                                throw new ArgumentException("Can't have a pointer to a byref type", "typeName");
                            }
                            break;

                        case ',':
                            if (!is_recurse)
                            {
                                goto Label_01E8;
                            }
                            num3 = pos;
                            goto Label_019B;

                        case '&':
                            if (spec.is_byref)
                            {
                                throw new ArgumentException("Can't have a byref of a byref", "typeName");
                            }
                            spec.is_byref = true;
                            goto Label_0476;

                        case '[':
                            if (spec.is_byref)
                            {
                                throw new ArgumentException("Byref qualifier must be the last one of a type", "typeName");
                            }
                            goto Label_0220;

                        case ']':
                            if (!is_recurse)
                            {
                                throw new ArgumentException("Unmatched ']'", "typeName");
                            }
                            p = pos + 1;
                            return spec;

                        default:
                            throw new ArgumentException(string.Concat(new object[] { "Bad type def, can't handle '", name[pos], "' at ", pos }), "typeName");
                    }
                    spec.pointer_level++;
                    goto Label_0476;
                Label_0195:
                    num3++;
                Label_019B:
                    if ((num3 < name.Length) && (name[num3] != ']'))
                    {
                        goto Label_0195;
                    }
                    if (num3 >= name.Length)
                    {
                        throw new ArgumentException("Unmatched ']' while parsing generic argument assembly name");
                    }
                    spec.assembly_name = name.Substring(pos + 1, (num3 - pos) - 1).Trim();
                    p = num3 + 1;
                    return spec;
                Label_01E8:
                    spec.assembly_name = name.Substring(pos + 1).Trim();
                    pos = name.Length;
                    goto Label_0476;
                Label_0220:
                    pos++;
                    if (pos >= name.Length)
                    {
                        throw new ArgumentException("Invalid array/generic spec", "typeName");
                    }
                    SkipSpace(name, ref pos);
                    if (((name[pos] == ',') || (name[pos] == '*')) || (name[pos] == ']'))
                    {
                        goto Label_0353;
                    }
                    List<TypeSpec> list = new List<TypeSpec>();
                    if (!spec.IsArray())
                    {
                        goto Label_0316;
                    }
                    throw new ArgumentException("generic args after array spec", "typeName");
                Label_0291:
                    SkipSpace(name, ref pos);
                    bool flag2 = name[pos] == '[';
                    if (flag2)
                    {
                        pos++;
                    }
                    list.Add(Parse(name, ref pos, true, flag2));
                    if (pos >= name.Length)
                    {
                        throw new ArgumentException("Invalid generic arguments spec", "typeName");
                    }
                    if (name[pos] == ']')
                    {
                        goto Label_0322;
                    }
                    if (name[pos] != ',')
                    {
                        throw new ArgumentException("Invalid generic arguments separator " + name[pos], "typeName");
                    }
                    pos++;
                Label_0316:
                    if (pos < name.Length)
                    {
                        goto Label_0291;
                    }
                Label_0322:
                    if ((pos >= name.Length) || (name[pos] != ']'))
                    {
                        throw new ArgumentException("Error parsing generic params spec", "typeName");
                    }
                    spec.generic_params = list;
                    goto Label_0476;
                Label_0353:
                    num4 = 1;
                    bool bound = false;
                    while ((pos < name.Length) && (name[pos] != ']'))
                    {
                        if (name[pos] == '*')
                        {
                            if (bound)
                            {
                                throw new ArgumentException("Array spec cannot have 2 bound dimensions", "typeName");
                            }
                            bound = true;
                        }
                        else
                        {
                            if (name[pos] != ',')
                            {
                                throw new ArgumentException("Invalid character in array spec " + name[pos], "typeName");
                            }
                            num4++;
                        }
                        pos++;
                        SkipSpace(name, ref pos);
                    }
                    if (name[pos] != ']')
                    {
                        throw new ArgumentException("Error parsing array spec", "typeName");
                    }
                    if ((num4 > 1) && bound)
                    {
                        throw new ArgumentException("Invalid array spec, multi-dimensional array cannot be bound", "typeName");
                    }
                    spec.AddArray(new ArraySpec(num4, bound));
                Label_0476:
                    pos++;
                }
            }
            p = pos;
            return spec;
        }

        private static void SkipSpace(string name, ref int pos)
        {
            int num = pos;
            while ((num < name.Length) && char.IsWhiteSpace(name[num]))
            {
                num++;
            }
            pos = num;
        }

        public override string ToString()
        {
            string fullName = this.GetFullName();
            if (this.assembly_name != null)
            {
                fullName = fullName + ", " + this.assembly_name;
            }
            return fullName;
        }
    }
}

