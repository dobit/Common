using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Xml;

namespace LFNet.Common.Helpers.ObjectDumperStrategy
{
    internal class XmlWriterStrategy : DumperWriterStrategyBase
    {
        private readonly List<string> _exclusions;
        private readonly XmlWriter _writer;

        public XmlWriterStrategy(XmlWriter writer) : this(0, new List<string>(), writer)
        {
        }

        public XmlWriterStrategy(int depth, IEnumerable<string> exclusions, XmlWriter writer)
        {
            base.Depth = depth;
            this._writer = writer;
            this._exclusions = this.GetListOfExclusions(exclusions);
        }

        private List<string> GetListOfExclusions(IEnumerable<string> items)
        {
            List<string> list = new List<string>();
            if (items != null)
            {
                list.AddRange(items);
            }
            return list;
        }

        private bool IsEnumeratorEmpty(object o)
        {
            if (o == null)
            {
                return true;
            }
            if (!(o is IEnumerable))
            {
                return false;
            }
            IEnumerable enumerable = o as IEnumerable;
            return !enumerable.GetEnumerator().MoveNext();
        }

        private bool IsExcludedMember(string name)
        {
            return this._exclusions.Any<string>(s => s.Contains(name));
        }

        private void ProcessFields(object root, IEnumerable<FieldInfo> fields)
        {
            foreach (FieldInfo info in fields)
            {
                this.WriteField(root, info);
            }
        }

        private void ProcessProperties(object root, IEnumerable<PropertyInfo> properties)
        {
            foreach (PropertyInfo info in properties)
            {
                this.WriteProperty(root, info);
            }
        }

        public override void Write(object o)
        {
            if (((o == null) || (o is ValueType)) || (o is string))
            {
                this.WriteValue(o);
            }
            else if (o is IEnumerable)
            {
                IEnumerable root = o as IEnumerable;
                this.WriteEnumerable(root);
            }
            else
            {
                this.WriteObjectStart(o);
                this.WriteMembers(o);
                this.WriteObjectEnd();
            }
        }

        private void WriteEnumerable(IEnumerable root)
        {
            if (!this.IsEnumeratorEmpty(root))
            {
                this._writer.WriteStartElement("items");
                foreach (object obj2 in root)
                {
                    if (obj2 != null)
                    {
                        if ((obj2 is ValueType) || (obj2 is string))
                        {
                            this._writer.WriteStartElement("item");
                            this._writer.WriteString(obj2.ToString());
                            this._writer.WriteEndElement();
                        }
                        else
                        {
                            this.Write(obj2);
                        }
                    }
                }
                this._writer.WriteEndElement();
            }
        }

        private void WriteField(object root, MemberInfo member)
        {
            FieldInfo info = member as FieldInfo;
            if (info != null)
            {
                Type fieldType = info.FieldType;
                Type type2 = fieldType.GetInterface(typeof(IEnumerable).FullName, false);
                object val = info.GetValue(root);
                if (val != null)
                {
                    if (fieldType.IsValueType || (fieldType == typeof(string)))
                    {
                        this.WriteStringOrValueTypeField(info.Name, fieldType.Name, val);
                    }
                    else
                    {
                        base.Level++;
                        if (base.Level <= base.Depth)
                        {
                            this.WriteFieldRecursive(info.Name, (null != type2) ? type2.Name : fieldType.Name, val);
                        }
                    }
                }
            }
        }

        private void WriteFieldRecursive(string name, string typeName, object val)
        {
            if ((val != null) && !this.IsEnumeratorEmpty(val))
            {
                this._writer.WriteStartElement("f");
                this._writer.WriteAttributeString("name", name);
                this._writer.WriteAttributeString("type", typeName);
                this.Write(val);
                this._writer.WriteEndElement();
            }
        }

        private void WriteMembers(object root)
        {
            MemberInfo[] members = root.GetType().GetMembers(BindingFlags.Public | BindingFlags.Instance);
            List<FieldInfo> fields = new List<FieldInfo>();
            List<PropertyInfo> properties = new List<PropertyInfo>();
            int num = 0;
            foreach (MemberInfo info in members)
            {
                if ((info.MemberType == MemberTypes.Field) && !this.IsExcludedMember(info.Name))
                {
                    fields.Add(info as FieldInfo);
                    num++;
                }
                if ((info.MemberType == MemberTypes.Property) && !this.IsExcludedMember(info.Name))
                {
                    properties.Add(info as PropertyInfo);
                    num++;
                }
            }
            if (this._exclusions.Count != num)
            {
                this.ProcessFields(root, fields);
                this.ProcessProperties(root, properties);
            }
        }

        private void WriteObjectEnd()
        {
            this._writer.WriteEndElement();
        }

        private void WriteObjectStart(object type)
        {
            this._writer.WriteStartElement("object", "http://schemas.codesmithtools.com/insight/objectdata");
            this._writer.WriteAttributeString("name", type.GetType().Name);
            this._writer.WriteAttributeString("namespace", type.GetType().Namespace);
        }

        private void WriteProperty(object root, MemberInfo member)
        {
            PropertyInfo info = member as PropertyInfo;
            if (info != null)
            {
                Type propertyType = info.PropertyType;
                Type type2 = propertyType.GetInterface(typeof(IEnumerable).FullName, false);
                if (info.GetIndexParameters().Length <= 0)
                {
                    object val = info.GetValue(root, null);
                    if (propertyType.IsValueType || (propertyType == typeof(string)))
                    {
                        this.WriteStringOrValueTypeProperty(info.Name, propertyType.Name, val);
                    }
                    else if (val != null)
                    {
                        base.Level++;
                        if (base.Level <= base.Depth)
                        {
                            this.WritePropertyRecursive(info.Name, (null != type2) ? type2.Name : propertyType.Name, val);
                        }
                    }
                }
            }
        }

        private void WritePropertyRecursive(string name, string typeName, object val)
        {
            if ((val != null) && !this.IsEnumeratorEmpty(val))
            {
                this._writer.WriteStartElement("p");
                this._writer.WriteAttributeString("name", name);
                this._writer.WriteAttributeString("type", typeName);
                this.Write(val);
                this._writer.WriteEndElement();
            }
        }

        private void WriteString(string s)
        {
            this._writer.WriteString(s);
        }

        private void WriteStringOrValueTypeField(string name, string typeName, object val)
        {
            if (val != null)
            {
                this._writer.WriteStartElement("f");
                this._writer.WriteAttributeString("name", name);
                this._writer.WriteAttributeString("type", typeName);
                this.WriteValue(val);
                this._writer.WriteEndElement();
            }
        }

        private void WriteStringOrValueTypeProperty(string name, string typeName, object val)
        {
            if (val != null)
            {
                this._writer.WriteStartElement("p");
                this._writer.WriteAttributeString("name", name);
                this._writer.WriteAttributeString("type", typeName);
                this.WriteValue(val);
                this._writer.WriteEndElement();
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

