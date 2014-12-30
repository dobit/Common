namespace LFNet.Licensing
{
    public class EncoderSequence : IEncoder
    {
        private IEncoder[] encoders;
        public EncoderSequence(params IEncoder[] encoders)
        {
            this.encoders = encoders;
        }
        public byte[] Encode(byte[] data)
        {
            byte[] array = data;
            for (int i = 0; i < this.encoders.Length; i++)
            {
                array = this.encoders[i].Encode(array);
            }
            return array;
        }
    }
}