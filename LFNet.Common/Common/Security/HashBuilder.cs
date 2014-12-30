using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using LFNet.Common.Component;
using LFNet.Common.Extensions;

namespace LFNet.Common.Security
{
    public class HashBuilder : DisposableBase
    {
        public HashBuilder()
        {
            this.BufferStream = new MemoryStream();
            this.Writer = new BinaryWriter(this.BufferStream, Encoding.Unicode);
        }

        public void Append(bool value)
        {
            this.Writer.Write(value);
        }

        public void Append(byte value)
        {
            this.Writer.Write(value);
        }

        public void Append(char value)
        {
            this.Writer.Write(value);
        }

        public void Append(DateTime value)
        {
            long num = value.ToBinary();
            this.Writer.Write(num);
        }

        public void Append(decimal value)
        {
            this.Writer.Write(value);
        }

        public void Append(double value)
        {
            this.Writer.Write(value);
        }

        public void Append(Guid value)
        {
            byte[] buffer = value.ToByteArray();
            this.Writer.Write(buffer);
        }

        public void Append(short value)
        {
            this.Writer.Write(value);
        }

        public void Append(int value)
        {
            this.Writer.Write(value);
        }

        public void Append(long value)
        {
            this.Writer.Write(value);
        }

        public void Append(float value)
        {
            this.Writer.Write(value);
        }

        public void Append(string value)
        {
            if (!value.IsNullOrEmpty())
            {
                this.Writer.Write(value);
            }
        }

        public byte[] ComputeHash()
        {
            if (this.HashAlgorithm == null)
            {
                this.HashAlgorithm = SHA1.Create();
            }
            this.Writer.Flush();
            byte[] buffer = this.BufferStream.ToArray();
            return this.HashAlgorithm.ComputeHash(buffer);
        }

        protected override void DisposeManagedResources()
        {
            this.Writer.Dispose();
            this.BufferStream.Dispose();
        }

        public string GetHash()
        {
            return this.ComputeHash().ToHex();
        }

        public override string ToString()
        {
            return this.GetHash();
        }

        protected MemoryStream BufferStream { get; set; }

        public global::System.Security.Cryptography.HashAlgorithm HashAlgorithm { get; set; }

        protected BinaryWriter Writer { get; set; }
    }
}

