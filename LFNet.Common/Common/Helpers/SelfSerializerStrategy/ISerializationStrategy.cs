using System.IO;

namespace LFNet.Common.Helpers.SelfSerializerStrategy
{
    internal interface ISerializationStrategy<T>
    {
        T Deserialize(Stream stream);
        Stream Serialize(Stream s, T type);
    }
}

