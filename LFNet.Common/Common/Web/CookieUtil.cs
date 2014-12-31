using System;
using System.Web;

namespace LFNet.Common.Web
{
    /// <summary>
    /// Cookies 工具类
    /// </summary>
    [Obsolete("该类已过期，请使用CookieHelper")]
    public static class CookieUtil
    {
        #region cookies

        /// <summary>
        /// 写cookie值
        /// </summary>
        /// <param name="strName">名称</param>
        /// <param name="strValue">值</param>
        public static void WriteCookie(string strName, string strValue)
        {
            CookieHelper.WriteCookie(strName, strValue);
        }

        /// <summary>
        /// 写cookie值
        /// </summary>
        /// <param name="strName">名称</param>
        /// <param name="key">键</param>
        /// <param name="strValue">值</param>
        /// <remarks></remarks>
        public static void WriteCookie(string strName, string key, string strValue)
        {
            CookieHelper.WriteCookie(strName,key, strValue);
        }

        /// <summary>
        /// 写cookie值
        /// </summary>
        /// <param name="strName">名称</param>
        /// <param name="strValue">值</param>
        /// <param name="expires">过期时间(分钟)</param>
        public static void WriteCookie(string strName, string strValue, int expires)
        {
            CookieHelper.WriteCookie(strName, strValue, expires);
        }

        public static void WriteCookie(string strName,string strValue,int expires,string domain)
        {
             CookieHelper.WriteCookie(strName, strValue, expires, domain);
        }

        /// <summary>
        /// 读cookie值
        /// </summary>
        /// <param name="strName">名称</param>
        /// <returns>cookie值</returns>
        public static string GetCookie(string strName)
        {
            return CookieHelper.GetCookie(strName);
        }

        /// <summary>
        /// 读cookie值
        /// </summary>
        /// <param name="strName">名称</param>
        /// <param name="key">The key.</param>
        /// <returns>cookie值</returns>
        /// <remarks></remarks>
        public static string GetCookie(string strName, string key)
        {
            return CookieHelper.GetCookie(strName, key);
        }

        #endregion
    }

    /// <summary>
    /// Cookies 工具类
    /// </summary>
    public static class CookieHelper 
    {
        #region cookies

        /// <summary>
        /// 写cookie值
        /// </summary>
        /// <param name="strName">名称</param>
        /// <param name="strValue">值</param>
        public static void WriteCookie(string strName, string strValue)
        {
            HttpCookie cookie = HttpContext.Current.Request.Cookies[strName] ?? new HttpCookie(strName);
            cookie.Value = strValue;
            HttpContext.Current.Response.AppendCookie(cookie);
        }

        /// <summary>
        /// 写cookie值
        /// </summary>
        /// <param name="strName">名称</param>
        /// <param name="key">键</param>
        /// <param name="strValue">值</param>
        /// <remarks></remarks>
        public static void WriteCookie(string strName, string key, string strValue)
        {
            HttpCookie cookie = HttpContext.Current.Request.Cookies[strName] ?? new HttpCookie(strName);
            cookie[key] = strValue;
            HttpContext.Current.Response.AppendCookie(cookie);
        }

        /// <summary>
        /// 写cookie值
        /// </summary>
        /// <param name="strName">名称</param>
        /// <param name="strValue">值</param>
        /// <param name="expires">过期时间(分钟)</param>
        public static void WriteCookie(string strName, string strValue, int expires)
        {
            HttpCookie cookie = HttpContext.Current.Request.Cookies[strName] ?? new HttpCookie(strName);
            cookie.Value = strValue;
            cookie.Expires = DateTime.Now.AddMinutes(expires);
            HttpContext.Current.Response.AppendCookie(cookie);
        }

        public static void WriteCookie(string strName, string strValue, int expires, string domain)
        {
            HttpCookie cookie = HttpContext.Current.Request.Cookies[strName] ?? new HttpCookie(strName);
            cookie.Value = strValue;
            cookie.Expires = DateTime.Now.AddMinutes(expires);
            if (!string.IsNullOrEmpty(domain))
                cookie.Domain = domain;
            HttpContext.Current.Response.AppendCookie(cookie);
        }

        /// <summary>
        /// 读cookie值
        /// </summary>
        /// <param name="strName">名称</param>
        /// <returns>cookie值</returns>
        public static string GetCookie(string strName)
        {
            if (HttpContext.Current.Request.Cookies[strName] != null)
                return HttpContext.Current.Request.Cookies[strName].Value;

            return "";
        }

        /// <summary>
        /// 读cookie值
        /// </summary>
        /// <param name="strName">名称</param>
        /// <param name="key">The key.</param>
        /// <returns>cookie值</returns>
        /// <remarks></remarks>
        public static string GetCookie(string strName, string key)
        {
            if (HttpContext.Current.Request.Cookies[strName] != null &&
                HttpContext.Current.Request.Cookies[strName][key] != null)
                return HttpContext.Current.Request.Cookies[strName][key];

            return "";
        }

        #endregion
    }
}