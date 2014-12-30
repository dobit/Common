using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Reflection;

namespace LFNet.Common.Helpers.ObjectDumperStrategy
{
    internal class TextWriterStrategy : DumperWriterStrategyBase
    {
        private List<string> exclusions;
        private int pos;
        private TextWriter writer;

        public TextWriterStrategy(TextWriter writer) : this(0, writer)
        {
        }

        public TextWriterStrategy(int depth, TextWriter writer) : this(depth, new List<string>(), writer)
        {
        }

        public TextWriterStrategy(int depth, IEnumerable<string> exclusions, TextWriter writer)
        {
            this.writer = writer;
            base.Depth = depth;
            this.exclusions = this.GetListOfExclusions(exclusions);
        }

        private List<string> GetListOfExclusions(IEnumerable<string> items)
        {
            List<string> list = new List<string>();
            if (items != null)
            {
                foreach (string str in items)
                {
                    list.Add(str);
                }
            }
            return list;
        }

        private bool IsExcludedMember(string name)
        {
            foreach (string str in this.exclusions)
            {
                if (str.Contains(name))
                {
                    return true;
                }
            }
            return false;
        }

        public override void Write(object o)
        {
            this.Write(null, o);
        }

        private void Write(object prefix, object o)
        {
            string str = prefix as string;
            this.WriteObject(str, o);
        }

        private void WriteIndent()
        {
            for (int i = 0; i < base.Level; i++)
            {
                this.writer.Write("  ");
            }
        }

        private void WriteLine()
        {
            this.writer.WriteLine();
            this.pos = 0;
        }

        private void WriteObject(string prefix, object o)
        {
            if (((o == null) || (o is ValueType)) || (o is string))
            {
                this.WriteIndent();
                this.WriteString(prefix);
                this.WriteValue(o);
                this.WriteLine();
            }
            else if (o is IEnumerable)
            {
                foreach (object obj2 in (IEnumerable) o)
                {
                    if ((obj2 is IEnumerable) && !(obj2 is string))
                    {
                        this.WriteIndent();
                        this.WriteString(prefix);
                        this.WriteString("...");
                        this.WriteLine();
                        if (base.Level < base.Depth)
                        {
                            base.Level++;
                            this.WriteObject(prefix, obj2);
                            base.Level--;
                        }
                    }
                    else
                    {
                        this.WriteObject(prefix, obj2);
                    }
                }
            }
            else
            {
                MemberInfo[] members = o.GetType().GetMembers(BindingFlags.Public | BindingFlags.Instance);
                this.WriteIndent();
                this.WriteString(prefix);
                bool flag = false;
                foreach (MemberInfo info in members)
                {
                    FieldInfo info2 = info as FieldInfo;
                    PropertyInfo info3 = info as PropertyInfo;
                    if ((info2 != null) || (info3 != null))
                    {
                        bool flag2 = false;
                        if ((null != info2) && (null == info3))
                        {
                            flag2 = this.IsExcludedMember(info2.Name);
                        }
                        if ((null != info3) && (null == info2))
                        {
                            flag2 = this.IsExcludedMember(info3.Name);
                        }
                        if (!flag2)
                        {
                            if (flag)
                            {
                                this.WriteTab();
                            }
                            else
                            {
                                flag = true;
                            }
                            this.WriteString(info.Name);
                            this.WriteString("=");
                            Type c = (info2 != null) ? info2.FieldType : info3.PropertyType;
                            if (c.IsValueType || (c == typeof(string)))
                            {
                                this.WriteValue((info2 != null) ? info2.GetValue(o) : info3.GetValue(o, null));
                            }
                            else if (typeof(IEnumerable).IsAssignableFrom(c))
                            {
                                this.WriteString("...");
                            }
                            else
                            {
                                this.WriteString("[ ]");
                            }
                        }
                    }
                }
                if (flag)
                {
                    this.WriteLine();
                }
                if (base.Level < base.Depth)
                {
                    foreach (MemberInfo info4 in members)
                    {
                        FieldInfo info5 = info4 as FieldInfo;
                        PropertyInfo info6 = info4 as PropertyInfo;
                        if ((info5 != null) || (info6 != null))
                        {
                            Type type2 = (info5 != null) ? info5.FieldType : info6.PropertyType;
                            if (!type2.IsValueType && !(type2 == typeof(string)))
                            {
                                object obj3 = (info5 != null) ? info5.GetValue(o) : info6.GetValue(o, null);
                                if (obj3 != null)
                                {
                                    base.Level++;
                                    this.WriteObject(info4.Name + ": ", obj3);
                                    base.Level--;
                                }
                            }
                        }
                    }
                }
            }
        }

        private void WriteString(string s)
        {
            if (s != null)
            {
                this.writer.Write(s);
                this.pos += s.Length;
            }
        }

        private void WriteTab()
        {
            this.WriteString("  ");
            while ((this.pos % 8) != 0)
            {
                this.WriteString(" ");
            }
        }

        private void WriteValue(object o)
        {
            if (o == null)
            {
                this.WriteString("null");
            }
            else if (o is DateTime)
            {
                this.WriteString(((DateTime) o).ToShortDateString());
            }
            else if ((o is ValueType) || (o is string))
            {
                this.WriteString(o.ToString());
            }
            else if (o is IEnumerable)
            {
                this.WriteString("...");
            }
            else
            {
                this.WriteString("{ }");
            }
        }
    }
}

