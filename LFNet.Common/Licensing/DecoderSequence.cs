namespace LFNet.Licensing
{
    public class DecoderSequence : IDecoder
    {
        private IDecoder[] decoders;
        public DecoderSequence(params IDecoder[] decoders)
        {
            this.decoders = decoders;
        }
        public byte[] Decode(byte[] data)
        {
            byte[] array = data;
            for (int i = this.decoders.Length - 1; i >= 0; i--)
            {
                array = this.decoders[i].Decode(array);
            }
            return array;
        }
    }
}