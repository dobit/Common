using System;
using System.IO;
using System.Text;

namespace LFNet.Licensing
{
    public class OldLicenseSerializer
    {
        private enum OldLicenseType
        {
            Evaluation,
            Real
        }
        private const int EvaluationPeriod = 30;
        public License Deserialize(string key, IDecoder decoder)
        {
            byte[] array = Base32Decoder.Decode(key);
            if (OldLicenseSerializer.IsEmpty(array))
            {
                return null;
            }
            byte[] array2 = decoder.Decode(array);
            if (OldLicenseSerializer.IsEmpty(array2))
            {
                return null;
            }
            License result;
            using (MemoryStream memoryStream = new MemoryStream(array2))
            {
                License license = OldLicenseSerializer.ReadLicenseFromStream(memoryStream);
                result = license;
            }
            return result;
        }
        public string Serialize(License license, IEncoder encoder)
        {
            byte[] data;
            using (MemoryStream memoryStream = new MemoryStream())
            {
                OldLicenseSerializer.WriteLicenseToStream(license, memoryStream);
                data = memoryStream.ToArray();
            }
            byte[] data2 = encoder.Encode(data);
            return Base32Encoder.Encode(data2);
        }
        private static bool IsEmpty(Array value)
        {
            return value == null || value.Length == 0;
        }
        private static void WriteLicenseToStream(License license, Stream stream)
        {
            string text = license.LicensedTo;
            if (text == null)
            {
                text = "";
            }
            string str = "";
            BinaryWriter binaryWriter = new BinaryWriter(stream, Encoding.Unicode);
            binaryWriter.Write((byte)license.Type);
            binaryWriter.Write(0L);
            binaryWriter.Write(license.Version);
            if (license.Type == LicenseType.Personal)
            {
                OldLicenseSerializer.WriteDateTime(binaryWriter, license.PurchaseDate);
            }
            else
            {
                OldLicenseSerializer.WriteDateTime(binaryWriter, license.StartTime);
            }
            OldLicenseSerializer.WriteBytePrefixedString(binaryWriter, text);
            OldLicenseSerializer.WriteBytePrefixedString(binaryWriter, str);
        }
        private static License ReadLicenseFromStream(Stream stream)
        {
            License result;
            try
            {
                License license = new License();
                BinaryReader binaryReader = new BinaryReader(stream, Encoding.Unicode);
                OldLicenseSerializer.OldLicenseType oldLicenseType = (OldLicenseSerializer.OldLicenseType)binaryReader.ReadByte();
                license.PurchaseId = "shareit:" + binaryReader.ReadInt64().ToString();
                license.Version = binaryReader.ReadByte();
                DateTime dateTime = OldLicenseSerializer.ReadDateTime(binaryReader);
                string text = OldLicenseSerializer.ReadBytePrefixedString(binaryReader);
                string text2 = OldLicenseSerializer.ReadBytePrefixedString(binaryReader);
                license.LicensedTo = text;
                if (text2.Length > 0 && text2 != text)
                {
                    License expr_79 = license;
                    expr_79.LicensedTo = expr_79.LicensedTo + ", " + text2;
                }
                license.Capacity = 1;
                license.Binding = LicenseBinding.User;
                if (oldLicenseType == OldLicenseSerializer.OldLicenseType.Real)
                {
                    if (text2.Length > 0)
                    {
                        license.Type = LicenseType.Corporate;
                        license.Binding = LicenseBinding.Seat;
                    }
                    else
                    {
                        license.Type = LicenseType.Personal;
                    }
                    license.StartTime = DateTime.MinValue;
                    license.EndTime = DateTime.MaxValue;
                    license.PurchaseDate = dateTime;
                }
                else
                {
                    license.Type = LicenseType.Evaluation;
                    license.StartTime = dateTime;
                    license.EndTime = dateTime.AddDays(30.0);
                    license.PurchaseDate = dateTime;
                }
                result = license;
            }
            catch (Exception inner)
            {
                throw new LicensingException("Can not parse license.", inner);
            }
            return result;
        }
        private static void WriteDateTime(BinaryWriter writer, DateTime date)
        {
            writer.Write(date.Ticks);
        }
        private static DateTime ReadDateTime(BinaryReader reader)
        {
            return new DateTime(reader.ReadInt64());
        }
        private static void WriteBytePrefixedString(BinaryWriter writer, string str)
        {
            writer.Write((byte)str.Length);
            writer.Write(str.ToCharArray());
        }
        private static string ReadBytePrefixedString(BinaryReader reader)
        {
            int num = (int)reader.ReadByte();
            char[] array = reader.ReadChars(num);
            if (array.Length != num)
            {
                throw new EndOfStreamException();
            }
            return new string(array);
        }
    }
}