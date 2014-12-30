using System.Collections.Generic;
using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;

namespace LFNet.Common.Collections
{
    [XmlRoot("dictionary")]
    public class SerializableDictionary<TKey, TValue> : Dictionary<TKey, TValue>, IXmlSerializable
    {
        public SerializableDictionary()
        {
        }

        public SerializableDictionary(IDictionary<TKey, TValue> dictionary) : base(dictionary)
        {
        }

        public SerializableDictionary(int capacity) : base(capacity)
        {
        }

        public SerializableDictionary(IEqualityComparer<TKey> comparer) : base(comparer)
        {
        }

        public SerializableDictionary(int capacity, IEqualityComparer<TKey> comparer) : base(capacity, comparer)
        {
        }

        public SerializableDictionary(IDictionary<TKey, TValue> dictionary, IEqualityComparer<TKey> comparer) : base(dictionary, comparer)
        {
        }

        public XmlSchema GetSchema()
        {
            return null;
        }

        public void ReadXml(XmlReader reader)
        {
            XmlSerializer serializer = new XmlSerializer(typeof(TKey));
            XmlSerializer serializer2 = new XmlSerializer(typeof(TValue));
            bool isEmptyElement = reader.IsEmptyElement;
            reader.Read();
            if (!isEmptyElement)
            {
                while (reader.NodeType != XmlNodeType.EndElement)
                {
                    reader.ReadStartElement("item");
                    reader.ReadStartElement("key");
                    TKey key = (TKey) serializer.Deserialize(reader);
                    reader.ReadEndElement();
                    reader.ReadStartElement("value");
                    TValue local2 = (TValue) serializer2.Deserialize(reader);
                    reader.ReadEndElement();
                    base.Add(key, local2);
                    reader.ReadEndElement();
                    reader.MoveToContent();
                }
                reader.ReadEndElement();
            }
        }

        public void WriteXml(XmlWriter writer)
        {
            XmlSerializer serializer = new XmlSerializer(typeof(TKey));
            XmlSerializer serializer2 = new XmlSerializer(typeof(TValue));
            foreach (TKey local in base.Keys)
            {
                writer.WriteStartElement("item");
                writer.WriteStartElement("key");
                serializer.Serialize(writer, local);
                writer.WriteEndElement();
                writer.WriteStartElement("value");
                TValue o = base[local];
                serializer2.Serialize(writer, o);
                writer.WriteEndElement();
                writer.WriteEndElement();
            }
        }
    }
}

