using System;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Web;
using System.Xml.Serialization;

namespace LFNet.Configuration
{
    class Utils
    {
        /// <summary>
        /// 获得当前绝对路径
        /// </summary>
        /// <param name="strPath">指定的路径</param>
        /// <returns>绝对路径</returns>
        public static string GetMapPath(string strPath)
        {
            //if(!string.IsNullOrEmpty(HttpRuntime.AppDomainId))
            if (HttpContext.Current != null)
            {
                strPath = strPath.Replace("\\", "/");
                if (strPath.StartsWith("/")) strPath = "~" + strPath;
                return HttpContext.Current.Server.MapPath(strPath);
            }
            else //非web程序引用
            {
                strPath = strPath.Replace("/", "\\");
                if (strPath.StartsWith("\\"))
                {
                    //strPath = strPath.Substring(strPath.IndexOf('\\', 1)).TrimStart('\\');
                    strPath = strPath.TrimStart('\\');
                }
                return System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, strPath);
            }
        }

        /// <summary>
        /// 在路径后面添加'/'
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public static string AppendSlashToPathIfNeeded(string path)
        {
            if (path == null)
            {
                return null;
            }
            int length = path.Length;
            if ((length != 0) && (path[length - 1] != '/'))
            {
                path = path + '/';
            }
            return path;
        }

        /// <summary>
        /// 在路径后面添加'/','\'
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public static string AppendSlashToPathIfNeeded(string path, char slash)
        {
            if (path == null)
            {
                return null;
            }
            int length = path.Length;
            if ((length != 0) && (path[length - 1] != slash))
            {
                path = path + slash;
            }
            return path;
        }

        /// <summary>
        /// 反序列化
        /// </summary>
        /// <param name="type">对象类型</param>
        /// <param name="filename">文件路径</param>
        /// <exception cref="System.IO.FileNotFoundException"></exception>
        /// <exception cref="System.IO.DirectoryNotFoundException"></exception>
        /// <returns></returns>
        public static object Load(Type type, string filename)
        {
            FileStream fs = null;
            try
            {
                // open the stream...
                fs = new FileStream(filename, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
                XmlSerializer serializer = new XmlSerializer(type);
                return serializer.Deserialize(fs);
            }
            catch (Exception ex)
            {
                throw ex;
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
            bool success = false;

            FileStream fs = null;
            // serialize it...
            try
            {
                FileInfo fi = new FileInfo(filename);
                if (!fi.Directory.Exists) fi.Directory.Create();
                fi = null;
                fs = new FileStream(filename, FileMode.Create, FileAccess.Write, FileShare.ReadWrite);
                XmlSerializer serializer = new XmlSerializer(obj.GetType());
                serializer.Serialize(fs, obj);
                success = true;
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                if (fs != null)
                    fs.Close();
            }
            return success;

        }

        /// <summary>
        /// 深拷贝对象
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="item"></param>
        /// <returns></returns>
        public static T Clone<T>(T item)
        {
            BinaryFormatter bf=  new BinaryFormatter();
            MemoryStream stream = new MemoryStream();
            bf.Serialize(stream, item);
            T instance= (T)bf.Deserialize(stream);
            stream.Dispose();
            return instance;

        }
    }
}