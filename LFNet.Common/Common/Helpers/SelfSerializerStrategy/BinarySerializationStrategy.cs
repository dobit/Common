using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

namespace LFNet.Common.Helpers.SelfSerializerStrategy
{
    internal class BinarySerializationStrategy<T> : ISerializationStrategy<T>
    {
        public T Deserialize(Stream stream)
        {
            BinaryFormatter formatter = new BinaryFormatter();
            return (T) formatter.Deserialize(stream);
        }

        public Stream Serialize(Stream s, T t)
        {
            new BinaryFormatter().Serialize(s, t);
            return s;
        }
    }
}

