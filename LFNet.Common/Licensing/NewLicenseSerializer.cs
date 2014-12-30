using System;
using System.IO;
using System.Text;

namespace LFNet.Licensing
{
    public class NewLicenseSerializer
    {
        public License Deserialize(string key, IDecoder decoder)
        {
            byte[] data = Base32Decoder.Decode(key);
            byte[] buffer = decoder.Decode(data);
            License result;
            using (MemoryStream memoryStream = new MemoryStream(buffer))
            {
                License license = NewLicenseSerializer.ReadLicenseFromStream(memoryStream);
                result = license;
            }
            return result;
        }
        public string Serialize(License license, IEncoder encoder)
        {
            byte[] data;
            using (MemoryStream memoryStream = new MemoryStream())
            {
                NewLicenseSerializer.WriteLicenseToStream(license, memoryStream);
                data = memoryStream.ToArray();
            }
            byte[] data2 = encoder.Encode(data);
            return Base32Encoder.Encode(data2);
        }
        private static void WriteLicenseToStream(License license, Stream stream)
        {
            BinaryWriter binaryWriter = new BinaryWriter(stream, Encoding.UTF8);
            binaryWriter.Write(license.Version);
            binaryWriter.Write((byte)license.Type);
            binaryWriter.Write((byte)license.Binding);
            binaryWriter.Write(license.Capacity);
            NewLicenseSerializer.WriteBytePrefixedString(binaryWriter, license.LicensedTo);
            NewLicenseSerializer.WriteDateTime(binaryWriter, license.StartTime);
            NewLicenseSerializer.WriteDateTime(binaryWriter, license.EndTime);
            NewLicenseSerializer.WriteBytePrefixedString(binaryWriter, license.LicenseId.ToString());
            NewLicenseSerializer.WriteBytePrefixedString(binaryWriter, license.PurchaseId);
            NewLicenseSerializer.WriteDateTime(binaryWriter, license.PurchaseDate);
        }
        private static License ReadLicenseFromStream(Stream stream)
        {
            BinaryReader binaryReader = new BinaryReader(stream, Encoding.UTF8);
            return new License
                       {
                           Version = binaryReader.ReadByte(),
                           Type = (LicenseType)binaryReader.ReadByte(),
                           Binding = (LicenseBinding)binaryReader.ReadByte(),
                           Capacity = binaryReader.ReadInt32(),
                           LicensedTo = NewLicenseSerializer.ReadBytePrefixedString(binaryReader),
                           StartTime = NewLicenseSerializer.ReadDateTime(binaryReader),
                           EndTime = NewLicenseSerializer.ReadDateTime(binaryReader),
                           LicenseId = new Guid(NewLicenseSerializer.ReadBytePrefixedString(binaryReader)),
                           PurchaseId = NewLicenseSerializer.ReadBytePrefixedString(binaryReader),
                           PurchaseDate = NewLicenseSerializer.ReadDateTime(binaryReader)
                       };
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
            if (str.Length > 255)
            {
                throw new ArgumentOutOfRangeException("str", str, "String is too long.");
            }
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