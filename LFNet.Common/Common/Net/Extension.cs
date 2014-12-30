using System;
using System.IO;
using System.Net;

namespace LFNet.Common.Net
{
    public static class Extension
    {
        /// <summary>
        /// 获取uri的资源,支持http,https,ftp
        /// </summary>
        /// <param name="uri"></param>
        /// <returns></returns>
        public static string GetContent(this Uri uri)
        {
           
            using (var streamReader = new StreamReader(WebRequest.Create(uri).GetResponse().GetResponseStream()))
            {
                return streamReader.ReadToEnd();
            }
        }
    }
}