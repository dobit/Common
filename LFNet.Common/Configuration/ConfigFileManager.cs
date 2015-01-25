using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Text;
using System.Collections;
using System.Text.RegularExpressions;
namespace LFNet.Configuration
{
    /// <summary>
    /// 配置文件管理
    /// </summary>
    public static class ConfigFileManager
    {
        private static string _configPath;
        /// <summary>
        /// 配置文件默认保存路径
        /// </summary>
        public static string ConfigPath
        {
            get
            {
                if (_configPath == null)
                {
                    _configPath = Utils.AppendSlashToPathIfNeeded(Utils.GetMapPath("\\config"), '\\');
                    if (!Directory.Exists(_configPath))
                        Directory.CreateDirectory(_configPath);
                    fileSystemWatcher = new FileSystemWatcher(_configPath, "*." + configFileExtName);
                    fileSystemWatcher.NotifyFilter = NotifyFilters.LastWrite;
                    fileSystemWatcher.Changed += new FileSystemEventHandler(fileSystemWatcher_Changed);
                    fileSystemWatcher.EnableRaisingEvents = true;
                }
                return _configPath;
            }
        }

        /// <summary>
        /// 配置文件的默认扩展名
        /// </summary>
        internal const string configFileExtName = "config";

        private static Dictionary<string, object> configs = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase);
        private static object locker = new object();


        private static System.IO.FileSystemWatcher fileSystemWatcher;
        static ConfigFileManager()
        {



        }

        static void fileSystemWatcher_Changed(object sender, FileSystemEventArgs e)
        {
            if (configs.ContainsKey(e.FullPath))
            {
                System.Threading.Thread.Sleep(1000);
                configs[e.FullPath] = Utils.Load(configs[e.FullPath].GetType(), e.FullPath);
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="ifNotExistsCreate">如果不存在则创建一个</param>
        /// <returns></returns>
        public static T GetConfig<T>(bool ifNotExistsCreate = false)
        {
            Type type = typeof(T);

            return GetConfig<T>(GetConfigFile(type), ifNotExistsCreate);
        }

        private static Regex m_regex = new Regex(@"(,[^\]]*)", RegexOptions.Compiled);


        internal static string GetConfigFile(Type objType)
        {
            string filename = ConfigPath + objType.FullName + "." + configFileExtName;
            filename = m_regex.Replace(filename, "");
            return filename;
        }

        /// <summary>
        /// 读取配置文件
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="filename"></param>
        /// <param name="ifNotExistsCreate">如果不存在则创建一个</param>
        /// <exception cref="System.IO.FileNotFoundException"></exception>
        /// <exception cref="System.IO.DirectoryNotFoundException"></exception>
        /// <returns></returns>
        public static T GetConfig<T>(string filename, bool ifNotExistsCreate = false)
        {
            if (configs.ContainsKey(filename))
            {
                return (T)configs[filename];
            }
            else
            {
                lock (locker)
                {
                    if (configs.ContainsKey(filename))
                    {
                        return (T)configs[filename];
                    }

                    object obj;
                    try
                    {
                        obj = Utils.Load(typeof (T), filename);
                    }
                    catch (FileNotFoundException ex)
                    {
                        if (ifNotExistsCreate)
                        {
                            obj = Activator.CreateInstance<T>();
                            SaveConfig(obj, filename);
                        }
                        else
                        {
                            throw ex;
                        }


                    }
                    catch (DirectoryNotFoundException ex)
                    {
                        if (ifNotExistsCreate)
                        {
                            obj = Activator.CreateInstance<T>();
                            SaveConfig(obj, filename);
                        }
                        else
                        {
                            throw ex;
                        }
                    }
                    T instance = (T)obj;
                    configs.Add(filename, obj);
                    return instance;
                }

            }
        }

        public static void SaveConfig<T>() where T : new()
        {
            string filename = GetConfigFile(typeof(T));
            if (configs.ContainsKey(filename))
            {
                SaveConfig(configs[filename], filename);
            }
            else
            {
                SaveConfig(new T(), filename);
            }
        }

        /// <summary>
        /// 保存配置文件
        /// </summary>
        /// <param name="instance"></param>
        public static void SaveConfig(this object instance)
        {
            string filename = GetConfigFile(instance.GetType());
            SaveConfig(instance, filename);
        }

        public static void SaveConfig(this object instance, string filename)
        {
            Utils.Save(instance, filename);
            if (configs.ContainsKey(filename))
            {
                configs[filename] = instance;
            }
        }


    }
}
