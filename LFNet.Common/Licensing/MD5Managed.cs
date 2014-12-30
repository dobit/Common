using System;
using System.Security.Cryptography;

namespace LFNet.Licensing
{
    public class MD5Managed : HashAlgorithm
    {
        private class MD5_CTX
        {
            public readonly uint[] state;
            public readonly uint[] count;
            public readonly byte[] buffer;
            public MD5_CTX()
            {
                this.state = new uint[4];
                this.count = new uint[2];
                this.buffer = new byte[64];
            }
            public void Clear()
            {
                Array.Clear(this.state, 0, this.state.Length);
                Array.Clear(this.count, 0, this.count.Length);
                Array.Clear(this.buffer, 0, this.buffer.Length);
            }
        }
        private const int S11 = 7;
        private const int S12 = 12;
        private const int S13 = 17;
        private const int S14 = 22;
        private const int S21 = 5;
        private const int S22 = 9;
        private const int S23 = 14;
        private const int S24 = 20;
        private const int S31 = 4;
        private const int S32 = 11;
        private const int S33 = 16;
        private const int S34 = 23;
        private const int S41 = 6;
        private const int S42 = 10;
        private const int S43 = 15;
        private const int S44 = 21;
        private readonly MD5Managed.MD5_CTX _context = new MD5Managed.MD5_CTX();
        private readonly byte[] _digest = new byte[16];
        private static byte[] PADDING;
        public override byte[] Hash
        {
            get
            {
                return this._digest;
            }
        }
        public override int HashSize
        {
            get
            {
                return this._digest.Length * 8;
            }
        }
        public MD5Managed()
        {
            MD5Managed.MD5Init(this._context);
        }
        public override void Initialize()
        {
            MD5Managed.MD5Init(this._context);
        }
        protected override void HashCore(byte[] array, int ibStart, int cbSize)
        {
            MD5Managed.MD5Update(this._context, array, (uint)ibStart, (uint)cbSize);
        }
        protected override byte[] HashFinal()
        {
            MD5Managed.MD5Final(this._digest, this._context);
            return this.Hash;
        }
        static MD5Managed()
        {
            MD5Managed.PADDING = new byte[64];
            MD5Managed.PADDING[0] = 128;
        }
        private static uint F(uint x, uint y, uint z)
        {
            return (x & y) | (~x & z);
        }
        private static uint G(uint x, uint y, uint z)
        {
            return (x & z) | (y & ~z);
        }
        private static uint H(uint x, uint y, uint z)
        {
            return x ^ y ^ z;
        }
        private static uint I(uint x, uint y, uint z)
        {
            return y ^ (x | ~z);
        }
        private static uint ROTATE_LEFT(uint x, int n)
        {
            return x << n | x >> 32 - n;
        }
        private static void FF(ref uint a, uint b, uint c, uint d, uint x, int s, uint ac)
        {
            a += MD5Managed.F(b, c, d) + x + ac;
            a = MD5Managed.ROTATE_LEFT(a, s);
            a += b;
        }
        private static void GG(ref uint a, uint b, uint c, uint d, uint x, int s, uint ac)
        {
            a += MD5Managed.G(b, c, d) + x + ac;
            a = MD5Managed.ROTATE_LEFT(a, s);
            a += b;
        }
        private static void HH(ref uint a, uint b, uint c, uint d, uint x, int s, uint ac)
        {
            a += MD5Managed.H(b, c, d) + x + ac;
            a = MD5Managed.ROTATE_LEFT(a, s);
            a += b;
        }
        private static void II(ref uint a, uint b, uint c, uint d, uint x, int s, uint ac)
        {
            a += MD5Managed.I(b, c, d) + x + ac;
            a = MD5Managed.ROTATE_LEFT(a, s);
            a += b;
        }
        private static void MD5Init(MD5Managed.MD5_CTX context)
        {
            context.count[0] = (context.count[1] = 0u);
            context.state[0] = 1732584193u;
            context.state[1] = 4023233417u;
            context.state[2] = 2562383102u;
            context.state[3] = 271733878u;
        }
        private static void MD5Update(MD5Managed.MD5_CTX context, byte[] input, uint inputIndex, uint inputLen)
        {
            uint num = context.count[0] >> 3 & 63u;
            if ((context.count[0] += inputLen << 3) < inputLen << 3)
            {
                context.count[1] += 1u;
            }
            context.count[1] += inputLen >> 29;
            uint num2 = 64u - num;
            uint num3 = 0u;
            if (inputLen >= num2)
            {
                Buffer.BlockCopy(input, (int)inputIndex, context.buffer, (int)num, (int)num2);
                MD5Managed.MD5Transform(context.state, context.buffer, 0u);
                num3 = num2;
                while (num3 + 63u < inputLen)
                {
                    MD5Managed.MD5Transform(context.state, input, num3);
                    num3 += 64u;
                }
                num = 0u;
            }
            Buffer.BlockCopy(input, (int)(inputIndex + num3), context.buffer, (int)num, (int)(inputLen - num3));
        }
        private static void MD5Final(byte[] digest, MD5Managed.MD5_CTX context)
        {
            byte[] array = new byte[8];
            MD5Managed.Encode(array, context.count, 8u);
            uint num = context.count[0] >> 3 & 63u;
            uint inputLen = (num < 56u) ? (56u - num) : (120u - num);
            MD5Managed.MD5Update(context, MD5Managed.PADDING, 0u, inputLen);
            MD5Managed.MD5Update(context, array, 0u, 8u);
            MD5Managed.Encode(digest, context.state, 16u);
            context.Clear();
        }
        private static void MD5Transform(uint[] state, byte[] block, uint blockIndex)
        {
            uint num = state[0];
            uint num2 = state[1];
            uint num3 = state[2];
            uint num4 = state[3];
            uint[] array = new uint[16];
            MD5Managed.Decode(array, block, blockIndex, 64u);
            MD5Managed.FF(ref num, num2, num3, num4, array[0], 7, 3614090360u);
            MD5Managed.FF(ref num4, num, num2, num3, array[1], 12, 3905402710u);
            MD5Managed.FF(ref num3, num4, num, num2, array[2], 17, 606105819u);
            MD5Managed.FF(ref num2, num3, num4, num, array[3], 22, 3250441966u);
            MD5Managed.FF(ref num, num2, num3, num4, array[4], 7, 4118548399u);
            MD5Managed.FF(ref num4, num, num2, num3, array[5], 12, 1200080426u);
            MD5Managed.FF(ref num3, num4, num, num2, array[6], 17, 2821735955u);
            MD5Managed.FF(ref num2, num3, num4, num, array[7], 22, 4249261313u);
            MD5Managed.FF(ref num, num2, num3, num4, array[8], 7, 1770035416u);
            MD5Managed.FF(ref num4, num, num2, num3, array[9], 12, 2336552879u);
            MD5Managed.FF(ref num3, num4, num, num2, array[10], 17, 4294925233u);
            MD5Managed.FF(ref num2, num3, num4, num, array[11], 22, 2304563134u);
            MD5Managed.FF(ref num, num2, num3, num4, array[12], 7, 1804603682u);
            MD5Managed.FF(ref num4, num, num2, num3, array[13], 12, 4254626195u);
            MD5Managed.FF(ref num3, num4, num, num2, array[14], 17, 2792965006u);
            MD5Managed.FF(ref num2, num3, num4, num, array[15], 22, 1236535329u);
            MD5Managed.GG(ref num, num2, num3, num4, array[1], 5, 4129170786u);
            MD5Managed.GG(ref num4, num, num2, num3, array[6], 9, 3225465664u);
            MD5Managed.GG(ref num3, num4, num, num2, array[11], 14, 643717713u);
            MD5Managed.GG(ref num2, num3, num4, num, array[0], 20, 3921069994u);
            MD5Managed.GG(ref num, num2, num3, num4, array[5], 5, 3593408605u);
            MD5Managed.GG(ref num4, num, num2, num3, array[10], 9, 38016083u);
            MD5Managed.GG(ref num3, num4, num, num2, array[15], 14, 3634488961u);
            MD5Managed.GG(ref num2, num3, num4, num, array[4], 20, 3889429448u);
            MD5Managed.GG(ref num, num2, num3, num4, array[9], 5, 568446438u);
            MD5Managed.GG(ref num4, num, num2, num3, array[14], 9, 3275163606u);
            MD5Managed.GG(ref num3, num4, num, num2, array[3], 14, 4107603335u);
            MD5Managed.GG(ref num2, num3, num4, num, array[8], 20, 1163531501u);
            MD5Managed.GG(ref num, num2, num3, num4, array[13], 5, 2850285829u);
            MD5Managed.GG(ref num4, num, num2, num3, array[2], 9, 4243563512u);
            MD5Managed.GG(ref num3, num4, num, num2, array[7], 14, 1735328473u);
            MD5Managed.GG(ref num2, num3, num4, num, array[12], 20, 2368359562u);
            MD5Managed.HH(ref num, num2, num3, num4, array[5], 4, 4294588738u);
            MD5Managed.HH(ref num4, num, num2, num3, array[8], 11, 2272392833u);
            MD5Managed.HH(ref num3, num4, num, num2, array[11], 16, 1839030562u);
            MD5Managed.HH(ref num2, num3, num4, num, array[14], 23, 4259657740u);
            MD5Managed.HH(ref num, num2, num3, num4, array[1], 4, 2763975236u);
            MD5Managed.HH(ref num4, num, num2, num3, array[4], 11, 1272893353u);
            MD5Managed.HH(ref num3, num4, num, num2, array[7], 16, 4139469664u);
            MD5Managed.HH(ref num2, num3, num4, num, array[10], 23, 3200236656u);
            MD5Managed.HH(ref num, num2, num3, num4, array[13], 4, 681279174u);
            MD5Managed.HH(ref num4, num, num2, num3, array[0], 11, 3936430074u);
            MD5Managed.HH(ref num3, num4, num, num2, array[3], 16, 3572445317u);
            MD5Managed.HH(ref num2, num3, num4, num, array[6], 23, 76029189u);
            MD5Managed.HH(ref num, num2, num3, num4, array[9], 4, 3654602809u);
            MD5Managed.HH(ref num4, num, num2, num3, array[12], 11, 3873151461u);
            MD5Managed.HH(ref num3, num4, num, num2, array[15], 16, 530742520u);
            MD5Managed.HH(ref num2, num3, num4, num, array[2], 23, 3299628645u);
            MD5Managed.II(ref num, num2, num3, num4, array[0], 6, 4096336452u);
            MD5Managed.II(ref num4, num, num2, num3, array[7], 10, 1126891415u);
            MD5Managed.II(ref num3, num4, num, num2, array[14], 15, 2878612391u);
            MD5Managed.II(ref num2, num3, num4, num, array[5], 21, 4237533241u);
            MD5Managed.II(ref num, num2, num3, num4, array[12], 6, 1700485571u);
            MD5Managed.II(ref num4, num, num2, num3, array[3], 10, 2399980690u);
            MD5Managed.II(ref num3, num4, num, num2, array[10], 15, 4293915773u);
            MD5Managed.II(ref num2, num3, num4, num, array[1], 21, 2240044497u);
            MD5Managed.II(ref num, num2, num3, num4, array[8], 6, 1873313359u);
            MD5Managed.II(ref num4, num, num2, num3, array[15], 10, 4264355552u);
            MD5Managed.II(ref num3, num4, num, num2, array[6], 15, 2734768916u);
            MD5Managed.II(ref num2, num3, num4, num, array[13], 21, 1309151649u);
            MD5Managed.II(ref num, num2, num3, num4, array[4], 6, 4149444226u);
            MD5Managed.II(ref num4, num, num2, num3, array[11], 10, 3174756917u);
            MD5Managed.II(ref num3, num4, num, num2, array[2], 15, 718787259u);
            MD5Managed.II(ref num2, num3, num4, num, array[9], 21, 3951481745u);
            state[0] += num;
            state[1] += num2;
            state[2] += num3;
            state[3] += num4;
            Array.Clear(array, 0, array.Length);
        }
        private static void Encode(byte[] output, uint[] input, uint len)
        {
            uint num = 0u;
            for (uint num2 = 0u; num2 < len; num2 += 4u)
            {
                output[(int)((UIntPtr)num2)] = (byte)(input[(int)((UIntPtr)num)] & 255u);
                output[(int)((UIntPtr)(num2 + 1u))] = (byte)(input[(int)((UIntPtr)num)] >> 8 & 255u);
                output[(int)((UIntPtr)(num2 + 2u))] = (byte)(input[(int)((UIntPtr)num)] >> 16 & 255u);
                output[(int)((UIntPtr)(num2 + 3u))] = (byte)(input[(int)((UIntPtr)num)] >> 24 & 255u);
                num += 1u;
            }
        }
        private static void Decode(uint[] output, byte[] input, uint inputIndex, uint len)
        {
            uint num = 0u;
            for (uint num2 = 0u; num2 < len; num2 += 4u)
            {
                output[(int)((UIntPtr)num)] = (uint)((int)input[(int)((UIntPtr)(inputIndex + num2))] | (int)input[(int)((UIntPtr)(inputIndex + num2 + 1u))] << 8 | (int)input[(int)((UIntPtr)(inputIndex + num2 + 2u))] << 16 | (int)input[(int)((UIntPtr)(inputIndex + num2 + 3u))] << 24);
                num += 1u;
            }
        }
    }
}