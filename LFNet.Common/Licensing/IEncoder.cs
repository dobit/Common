namespace LFNet.Licensing
{
    public interface IEncoder
    {
        byte[] Encode(byte[] data);
    }
}