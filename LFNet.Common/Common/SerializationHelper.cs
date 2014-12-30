using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Xml;
using System.Xml.Serialization;

namespace LFNet.Common
{
    /// <summary>
    /// 序列化反序列化类
    /// </summary>
    public static class SerializationHelper
    {
        private static readonly Dictionary<int, XmlSerializer> SerializerDict = new Dictionary<int, XmlSerializer>();

       

        /// <summary>
        /// Gets the serializer.
        /// </summary>
        /// <param name="t">The t.</param>
        /// <returns></returns>
        /// <remarks></remarks>
        public static XmlSerializer GetSerializer(Type t)
        {
            int typeHash = t.GetHashCode();

            if (!SerializerDict.ContainsKey(typeHash))
                SerializerDict.Add(typeHash, new XmlSerializer(t));

            return SerializerDict[typeHash];
        }


        /// <summary>
        /// 反序列化
        /// </summary>
        /// <param name="type">对象类型</param>
        /// <param name="filename">文件路径</param>
        /// <returns></returns>
        public static object Load(Type type, string filename)
        {
            FileStream fs = null;
            try
            {
                // open the stream...
                fs = new FileStream(filename, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
                var serializer = new XmlSerializer(type);
                return serializer.Deserialize(fs);
            }
            finally
            {
                if (fs != null)
                    fs.Close();
            }
        }


        /// <summary>
        /// 序列化
        /// </summary>
        /// <param name="obj">对象</param>
        /// <param name="filename">文件路径</param>
        public static bool Save(object obj, string filename)
        {
            FileStream fs = null;
            // serialize it...
            try
            {
                var fi = new FileInfo(filename);
                if (!fi.Directory.Exists) fi.Directory.Create();
                fs = new FileStream(filename, FileMode.Create, FileAccess.Write, FileShare.ReadWrite);
                var serializer = new XmlSerializer(obj.GetType());
                serializer.Serialize(fs, obj);
            }
            finally
            {
                if (fs != null)
                    fs.Close();
            }
            return true;
        }
        /// <summary>
        ///  xml序列化成字符串
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public static string ToXmlString(this object obj)
        {
            return obj.ToXml();
            //return Serialize(obj);
        }

        /// <summary>
        ///  xml序列化成字符串
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="format">格式化文档，默认不格式化文档</param>
        /// <returns></returns>
        public static string ToXmlString(this object obj,bool format)
        {
            return obj.ToXml(format);
            //return Serialize(obj);
        }
        /// <summary>
        /// xml序列化成字符串
        /// </summary>
        /// <param name="obj">对象</param>
        /// <returns>xml字符串</returns>
        public static string Serialize(object obj)
        {
            string returnStr;
            XmlSerializer serializer = GetSerializer(obj.GetType());
            var ms = new MemoryStream();
            XmlTextWriter xtw = null;
            StreamReader sr = null;
            try
            {
                xtw = new XmlTextWriter(ms, Encoding.UTF8) {Formatting = Formatting.Indented};
                serializer.Serialize(xtw, obj);
                ms.Seek(0, SeekOrigin.Begin);
                sr = new StreamReader(ms);
                returnStr = sr.ReadToEnd();
            }
            finally
            {
                if (xtw != null)
                    xtw.Close();
                if (sr != null)
                    sr.Close();
                ms.Close();
            }
            return returnStr;
        }

        /// <summary>
        /// 反序列化.
        /// </summary>
        /// <param name="type">The type.</param>
        /// <param name="s">The s.</param>
        /// <returns></returns>
        /// <remarks></remarks>
        public static object DeSerialize(Type type, string s)
        {
            byte[] b = Encoding.UTF8.GetBytes(s);

            XmlSerializer serializer = GetSerializer(type);
            return serializer.Deserialize(new MemoryStream(b));
        }
    }
}