using System;
using System.IO;
using System.Security.Cryptography;

namespace LFNet.Licensing
{
    public class RSALicenseCodec : IEncoder, IDecoder
    {
        private const int KeyLen = 1024;
        private const int KeyByteLen = 128;
        private const int MaxRSABlockSize = 86;
        private const string hname = "MD5";
        private RSACryptoServiceProvider rsaProvider;
        public RSALicenseCodec(RSACryptoServiceProvider rsaProvider)
        {
            this.rsaProvider = rsaProvider;
        }
        public RSALicenseCodec(RSAParameters rsaParameters)
        {
            this.rsaProvider = this.CreateRSAProvider(rsaParameters);
        }
        public byte[] Encode(byte[] data)
        {
            byte[] result;
            using (MemoryStream memoryStream = new MemoryStream())
            {
                HashAlgorithm hashAlgorithm = new MD5CryptoServiceProvider();
                hashAlgorithm.ComputeHash(data);
                byte[] array = this.rsaProvider.SignData(data, hashAlgorithm);
                memoryStream.Write(data, 0, data.Length);
                memoryStream.Write(array, 0, array.Length);
                memoryStream.Flush();
                result = memoryStream.ToArray();
            }
            return result;
        }
        public byte[] Decode(byte[] data)
        {
            if (data.Length < 128)
            {
                throw new LicensingException("License key has invalid length.", null);
            }
            Stream input = new MemoryStream(data);
            byte[] result;
            using (BinaryReader binaryReader = new BinaryReader(input))
            {
                byte[] array = binaryReader.ReadBytes(data.Length - 128);
                byte[] rgbSignature = binaryReader.ReadBytes(128);
                HashAlgorithm hashAlgorithm = new MD5Managed();
                byte[] rgbHash = hashAlgorithm.ComputeHash(array);
                try
                {
                    if (!this.rsaProvider.VerifyHash(rgbHash, "MD5", rgbSignature))
                    {
                        throw new LicensingException("License key has invalid signature.", null);
                    }
                }
                catch (Exception ex)
                {
                    throw new LicensingException("License decode error (" + ex.Message + ")", ex);
                }
                result = array;
            }
            return result;
        }
        private RSACryptoServiceProvider CreateRSAProvider(RSAParameters rsaParameters)
        {
            RSACryptoServiceProvider rSACryptoServiceProvider = new RSACryptoServiceProvider(1024);
            RSAParameters parameters = this.DeepCopyRSAParameters(rsaParameters);
            rSACryptoServiceProvider.ImportParameters(parameters);
            return rSACryptoServiceProvider;
        }
        private byte[] CloneByteArray(byte[] original)
        {
            if (original == null)
            {
                return null;
            }
            return (byte[])original.Clone();
        }
        private RSAParameters DeepCopyRSAParameters(RSAParameters original)
        {
            return new RSAParameters
                       {
                           D = this.CloneByteArray(original.D),
                           DP = this.CloneByteArray(original.DP),
                           DQ = this.CloneByteArray(original.DQ),
                           Exponent = this.CloneByteArray(original.Exponent),
                           InverseQ = this.CloneByteArray(original.InverseQ),
                           Modulus = this.CloneByteArray(original.Modulus),
                           P = this.CloneByteArray(original.P),
                           Q = this.CloneByteArray(original.Q)
                       };
        }
    }
}