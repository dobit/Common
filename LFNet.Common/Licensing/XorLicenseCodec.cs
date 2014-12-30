namespace LFNet.Licensing
{
    public class XorLicenseCodec : IEncoder, IDecoder
    {
        private byte[] xorKey = new byte[]
                                    {
                                        20,
                                        177,
                                        126,
                                        47,
                                        49
                                    };
        public XorLicenseCodec(byte[] xorKey)
        {
            this.xorKey = xorKey;
        }
        public XorLicenseCodec()
        {
        }
        public byte[] Encode(byte[] data)
        {
            byte[] array = new byte[data.Length];
            for (int i = 0; i < data.Length; i++)
            {
                array[i] = (byte)(data[i] ^ this.xorKey[i % this.xorKey.Length]);
            }
            return array;
        }
        public byte[] Decode(byte[] data)
        {
            return this.Encode(data);
        }
    }
}