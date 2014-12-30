using System;
using System.Collections.Generic;
using System.Text;
using System.Web;
using System.Configuration;
namespace LFNet.Configuration
{
  public static class ConfigurationManager
    {
      private const string _webConfigFile="~/Web.config";
      private static System.Configuration.Configuration _configuration;

      public static System.Configuration.Configuration Configuration
      {
          get
          {
              if (_configuration == null)
              {
                  _configuration = GetConfiguration();
              }
              return ConfigurationManager._configuration;
          }
          set { ConfigurationManager._configuration = value; }
      }
      
      static ConfigurationManager()
      { 
        
      }

      /// <summary>
      /// 从配置文件加载设置
      /// </summary>
      /// <param name="sectionName"></param>
      /// <returns></returns>
      public static object GetSection(string sectionName)
      {
          if (HttpContext.Current != null)
          {
              return System.Web.Configuration.WebConfigurationManager.GetSection(sectionName);// Configuration.GetSection(sectionName);
          }
          else
          {
              return System.Configuration.ConfigurationManager.GetSection(sectionName);
          }
      }

      /// <summary>
      /// 返回当前执行环境的配置文件
      /// </summary>
      /// <returns></returns>
      private static System.Configuration.Configuration GetConfiguration()
      {
          if (HttpContext.Current != null)
          {
              System.IO.File.Copy(HttpContext.Current.Server.MapPath(_webConfigFile), HttpContext.Current.Server.MapPath(_webConfigFile + ".config"),true);
              return System.Web.Configuration.WebConfigurationManager.OpenWebConfiguration(_webConfigFile + ".config");
          }
          return System.Configuration.ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
               
      }
      /// <summary>
      /// 保存配置文件
      /// </summary>
      public static void Save()
      {
          if (HttpContext.Current != null)
          {
              Configuration.SaveAs(HttpContext.Current.Server.MapPath(_webConfigFile));
          }
          else
          {

              Configuration.Save();
          }
      }
    }
}
