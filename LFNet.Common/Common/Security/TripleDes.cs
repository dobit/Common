using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using LFNet.Common.Extensions;

namespace LFNet.Common.Security
{
    public class TripleDes
    {
        private static readonly byte[] keyBytes = new byte[]
      {
          0x30, 0x81, 0x89, 0x02, 0x81, 0x81, 0x00, 0xb3, 0xd8, 0xe4, 0xbf, 0x74, 0xb9, 0x38, 0x0f, 0x36,
          0xd1, 0x4c, 0x4c, 0xf6, 0x0f, 0x76, 0x9c, 0xb2, 0x50, 0xcc, 0x99, 0xf0, 0x77, 0x05, 0x4d, 0x99,
          0x18, 0x93, 0xb8, 0x66, 0x7b, 0x5e, 0x20, 0xfc, 0x88, 0xa3, 0x5b, 0x0c, 0x36, 0x3d, 0x3f, 0x81,
          0xe5, 0x44, 0x58, 0x51, 0x77, 0xe9, 0x5c, 0x82, 0x3b, 0x22, 0x8d, 0xfa, 0x88, 0x2b, 0xd1, 0x47,
          0x96, 0x01, 0x10, 0x89, 0xdd, 0xbc, 0xd5, 0x02, 0x2b, 0x68, 0x93, 0x75, 0x34, 0xaf, 0x5b, 0x94,
          0x51, 0x94, 0xe6, 0x65, 0x5e, 0x53, 0xd6, 0x0b, 0x49, 0xad, 0x9c, 0x2f, 0x32, 0xdd, 0xc0, 0xac,
          0xd7, 0x56, 0x66, 0x0a, 0x01, 0x56, 0x0c, 0x61, 0xe5, 0x42, 0xb9, 0x4e, 0x16, 0xd7, 0xf0, 0x96,
          0xf0, 0x3d, 0x05, 0xfe, 0x14, 0x8d, 0xd1, 0xeb, 0x17, 0x15, 0x6e, 0xe5, 0xa1, 0x8f, 0xce, 0x08,
          0x21, 0x93, 0x65, 0x27, 0xd6, 0x55, 0x01, 0x02, 0x03, 0x01, 0x00, 0x01
      };
        TripleDESCryptoServiceProvider desProvider = new TripleDESCryptoServiceProvider();
        public TripleDes()
            : this(keyBytes.Take(0x24).ToArray(), keyBytes.Skip(0x24).Take(0x08).ToArray())
        {

        }

        public TripleDes(string key, string iv)
            : this(Encoding.UTF8.GetBytes(key),Encoding.UTF8.GetBytes(iv))
        {
            
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="key"></param>
        /// <param name="iv"></param>
        public TripleDes(byte[] key, byte[] iv)
        {
            //desProvider = new TripleDESCryptoServiceProvider();
            
            //if(key.Length%8!=0)
            desProvider.Key = key;
            desProvider.IV = iv;
        }

        public byte[] Encode(byte[] data)
        {
            byte[] returnData = null;
            MemoryStream ms = new MemoryStream();
            ICryptoTransform encrypto = desProvider.CreateEncryptor();
            CryptoStream cs = new CryptoStream(ms, encrypto, CryptoStreamMode.Write);
            cs.Write(data, 0, data.Length);
            cs.FlushFinalBlock();
            returnData = ms.ToArray();
            return returnData;
        }


        public byte[] Decode(byte[] data)
        {
            byte[] returnData = null;
            MemoryStream ms = new MemoryStream();
            ICryptoTransform encrypto = desProvider.CreateDecryptor();
            CryptoStream cs = new CryptoStream(ms, encrypto, CryptoStreamMode.Write);
            cs.Write(data, 0, data.Length);
            cs.FlushFinalBlock();
            returnData = ms.ToArray();
            return returnData;
        }


        public string Encode(string str,bool toHex=false)
        {
            byte[] SourData = Encoding.UTF8.GetBytes(str);
            byte[] retData = Encode(SourData);
            return toHex?retData.ToHex():Convert.ToBase64String(retData, 0, retData.Length);
        }


        public string Decode(string str,bool fromHex=false)
        {
            byte[] SourData =fromHex?str.ToByteArray(): Convert.FromBase64String(str);
            byte[] retData = Decode(SourData);
            return Encoding.UTF8.GetString(retData, 0, retData.Length);

        }
     

        public byte[] Key
        {
            get
            {
                return desProvider.Key;
            }
        }

        public byte[] IV
        {
            get
            {
                return desProvider.IV;
            }
        }
    }

   
}
