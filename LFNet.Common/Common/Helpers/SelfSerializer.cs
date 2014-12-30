using System.IO;
using LFNet.Common.Helpers.SelfSerializerStrategy;

namespace LFNet.Common.Helpers
{
    public class SelfSerializer<T>
    {
        public T BinaryDeserialize(Stream stream)
        {
            ISerializationStrategy<T> strategy = new BinarySerializationStrategy<T>();
            return strategy.Deserialize(stream);
        }

        public Stream BinarySerialize(T t)
        {
            return this.BinarySerializer(t, new MemoryStream());
        }

        public Stream BinarySerializer(T t, Stream s)
        {
            ISerializationStrategy<T> strategy = new BinarySerializationStrategy<T>();
            return strategy.Serialize(s, t);
        }

        public T XmlDeserialize(Stream stream)
        {
            ISerializationStrategy<T> strategy = new XmlSerializationStrategy<T>();
            return strategy.Deserialize(stream);
        }

        public Stream XmlSerialize(T t)
        {
            return this.XmlSerialize(t, new MemoryStream());
        }

        public Stream XmlSerialize(T t, Stream s)
        {
            ISerializationStrategy<T> strategy = new XmlSerializationStrategy<T>();
            return strategy.Serialize(s, t);
        }

        public static SelfSerializer<T> Current
        {
            get
            {
                return Nested.Current;
            }
        }

        private class Nested
        {
            internal static readonly SelfSerializer<T> Current;

            static Nested()
            {
                SelfSerializer<T>.Nested.Current = new SelfSerializer<T>();
            }
        }
    }
}

