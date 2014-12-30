using System;
using System.Collections.Generic;
using System.Text;
using LFNet.Common;
using LFNet.Common.Logs;

namespace LFNet.Configuration
{
    /// <summary>
    /// <para>配置文件基类</para>
    /// <para>继承该泛型后无需实例化可直接调用</para>
    /// <code>
    /// <para>public class ConfigA:BaseConfig&lt;ConfigA&gt;</para>
    /// <para>{</para>
    /// <para>    public string Name {get;set;}</para>
    /// <para>}</para>
    /// </code>
    /// <para>
    /// 例子：
    /// <example>
    /// string Name= ConfigA.ConfigInfo.Name;
    /// </example>
    /// </para>
    /// <para>或者直接调用</para>
    /// <code>
    /// BaseConfig&lt;ConfigA&gt;.Config;
    /// </code>
    /// </summary>
    /// <typeparam name="T"></typeparam>
    [Serializable]
    [Obsolete("该抽象类已经过期，请调用ConfigFileManager.GetConfig")]
    public abstract class BaseConfig<T> where T:new()
    {
        /// <summary>
        /// 配置文件地址
        /// </summary>
        public static string ConfigFilename { get; set; }


        /// <summary>
        /// 获取配置
        /// </summary>
        public static T Instance
        {
            get
            {
                if (string.IsNullOrEmpty(ConfigFilename)) ConfigFilename=ConfigFileManager.GetConfigFile(typeof (T));
                try
                {
                    return ConfigFileManager.GetConfig<T>(ConfigFilename);
                }
                catch(Exception ex)
                {
                   
                    if(ex is System.IO.DirectoryNotFoundException || ex is System.IO.FileNotFoundException)
                    {
                        var instance = new T();
                        ConfigFileManager.SaveConfig(instance, ConfigFilename);
                        return instance;
                    }
                    else
                    { 
                        LogUtil.Log(ex);throw;
                        
                    }
                    
                    

                }
            }

        }

        /// <summary>
        /// 保存配置
        /// </summary>
        public static void Save()
        {
           ConfigFileManager.SaveConfig<T>();// ConfigManager<T>.Save();
        }
       
    }


}
