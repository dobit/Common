namespace LFNet.Licensing
{
    public interface IDecoder
    {
        byte[] Decode(byte[] data);
    }
}