using System.IO;
using System.Xml.Serialization;

namespace LFNet.Common.Helpers.SelfSerializerStrategy
{
    internal class XmlSerializationStrategy<T> : ISerializationStrategy<T>
    {
        public T Deserialize(Stream s)
        {
            XmlSerializer serializer = new XmlSerializer(typeof(T));
            return (T) serializer.Deserialize(s);
        }

        public Stream Serialize(Stream s, T t)
        {
            new XmlSerializer(typeof(T)).Serialize(s, t);
            return s;
        }
    }
}

