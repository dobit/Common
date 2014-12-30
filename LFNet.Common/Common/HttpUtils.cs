using System;
using System.Collections.Specialized;
using System.Net;
using System.Text;
using System.Threading;
using System.Web;

namespace LFNet.Common
{
    /// <summary>
    /// http工具类
    /// </summary>
    /// <remarks>
    /// dobit 2011-6-3 封装
    /// </remarks>
    public static class HttpUtils
    {
        #region 同步方法

        /// <summary>
        /// Downloads the string.Use GET Method
        /// </summary>
        /// <param name="url">The URL.</param>
        /// <returns></returns>
        /// <remarks></remarks>
        public static string DownloadString(string url)
        {
            return DownloadString(url, HttpMethod.GET, "", Encoding.UTF8, Encoding.UTF8);
        }

        /// <summary>
        /// Downloads the string.
        /// </summary>
        /// <param name="url">The URL.</param>
        /// <param name="httpMethod">The HTTP method.</param>
        /// <returns></returns>
        /// <remarks></remarks>
        public static string DownloadString(string url, HttpMethod httpMethod)
        {
            return DownloadString(url, httpMethod, "", Encoding.UTF8, Encoding.UTF8);
        }

        /// <summary>
        /// Downloads the string.
        /// </summary>
        /// <param name="url">The URL.</param>
        /// <param name="httpMethod">The HTTP method.</param>
        /// <param name="request">The request.</param>
        /// <returns></returns>
        /// <remarks></remarks>
        public static string DownloadString(string url, HttpMethod httpMethod, NameValueCollection request)
        {
            return DownloadString(url, httpMethod, RequestToString(request), Encoding.UTF8, Encoding.UTF8);
        }

        /// <summary>
        /// Downloads the string.
        /// </summary>
        /// <param name="url">The URL.</param>
        /// <param name="httpMethod">The HTTP method.</param>
        /// <param name="request">The request.</param>
        /// <param name="requestEncoding">The request encoding.</param>
        /// <param name="responseEncoding">The response encoding.</param>
        /// <returns></returns>
        /// <remarks></remarks>
        public static string DownloadString(string url, HttpMethod httpMethod, NameValueCollection request,
                                            Encoding requestEncoding, Encoding responseEncoding)
        {
            return DownloadString(url, httpMethod, RequestToString(request, requestEncoding), requestEncoding,
                                  responseEncoding);
        }

        /// <summary>
        /// Downloads the string.
        /// </summary>
        /// <param name="url">The URL.</param>
        /// <param name="httpMethod">The HTTP method.</param>
        /// <param name="requestData">The request data.</param>
        /// <returns></returns>
        /// <remarks></remarks>
        public static string DownloadString(string url, HttpMethod httpMethod, string requestData)
        {
            return DownloadString(url, httpMethod, requestData, Encoding.UTF8, Encoding.UTF8);
        }


        /// <summary>
        /// 下载网站内容
        /// </summary>
        /// <param name="url">The URL.</param>
        /// <param name="httpMethod">The HTTP method.</param>
        /// <param name="requestData">The request data.</param>
        /// <param name="requestEncoding">The request encoding.</param>
        /// <param name="responseEncoding">The response encoding.</param>
        /// <returns></returns>
        /// <remarks></remarks>
        public static string DownloadString(string url, HttpMethod httpMethod, string requestData,
                                            Encoding requestEncoding, Encoding responseEncoding)
        {
            return responseEncoding.GetString(DownloadData(url, httpMethod, requestData, requestEncoding));
        }

        /// <summary>
        /// Downloads the data.
        /// </summary>
        /// <param name="url">The URL.</param>
        /// <param name="httpMethod">The HTTP method.</param>
        /// <param name="requestData">The request data.</param>
        /// <param name="requestEncoding">The request encoding.</param>
        /// <returns></returns>
        /// <remarks></remarks>
        public static byte[] DownloadData(string url, HttpMethod httpMethod, string requestData,
                                          Encoding requestEncoding)
        {
            if (httpMethod == HttpMethod.POST)
            {
                return DownloadData(url, httpMethod, requestEncoding.GetBytes(requestData));
            }

            if (!url.Contains("?") && !string.IsNullOrEmpty(requestData))
                url = url + "?" + HttpUtility.ParseQueryString(requestData, requestEncoding);
            else if (url.Contains("?") && !string.IsNullOrEmpty(requestData))
            {
                url = url + "&" + HttpUtility.ParseQueryString(requestData, requestEncoding);
            }
            return DownloadData(url, httpMethod, new byte[] {});
        }

        /// <summary>
        /// Downloads the data.
        /// </summary>
        /// <param name="url">The URL.</param>
        /// <param name="httpMethod">The HTTP method.</param>
        /// <param name="requestData">The request data.</param>
        /// <returns></returns>
        /// <remarks></remarks>
        public static byte[] DownloadData(string url, HttpMethod httpMethod, byte[] requestData)
        {
            using (var web = new WebClient())
            {
                web.Headers.Add("User-Agent", "Mozilla/5.0 (compatible; MSIE 9.0; Windows NT 6.1; WOW64; Trident/5.0; BOIE9;ZHCN)");
                //web.Encoding = responseEncoding;
                if (httpMethod == HttpMethod.POST)
                {
                    web.Headers.Add("Content-Type", "application/x-www-form-urlencoded");
                    return web.UploadData(url, "POST", requestData);
                }
                return web.DownloadData(url);
            }
        }

        #endregion

        #region 异步方法

        /// <summary>
        /// Downloads the string.Use GET Method
        /// </summary>
        /// <param name="url">The URL.</param>
        /// <param name="callback">The callback.</param>
        /// <param name="userState">State of the user.</param>
        /// <returns></returns>
        /// <remarks></remarks>
        public static AsyncResult DownloadStringAsync(string url, AsyncStringCallback callback, object userState)
        {
            return DownloadStringAsync(url, HttpMethod.GET, "", Encoding.UTF8, Encoding.UTF8, callback, userState);
        }

        /// <summary>
        /// Downloads the string.
        /// </summary>
        /// <param name="url">The URL.</param>
        /// <param name="httpMethod">The HTTP method.</param>
        /// <param name="callback">The callback.</param>
        /// <param name="userState">State of the user.</param>
        /// <returns></returns>
        /// <remarks></remarks>
        public static AsyncResult DownloadStringAsync(string url, HttpMethod httpMethod, AsyncStringCallback callback,
                                                      object userState)
        {
            return DownloadStringAsync(url, httpMethod, "", Encoding.UTF8, Encoding.UTF8, callback, userState);
        }

        /// <summary>
        /// Downloads the string.
        /// </summary>
        /// <param name="url">The URL.</param>
        /// <param name="httpMethod">The HTTP method.</param>
        /// <param name="request">The request.</param>
        /// <param name="callback">The callback.</param>
        /// <param name="userState">State of the user.</param>
        /// <returns></returns>
        /// <remarks></remarks>
        public static AsyncResult DownloadStringAsync(string url, HttpMethod httpMethod, NameValueCollection request,
                                                      AsyncStringCallback callback, object userState)
        {
            return DownloadStringAsync(url, httpMethod, RequestToString(request), Encoding.UTF8, Encoding.UTF8, callback,
                                       userState);
        }

        /// <summary>
        /// Downloads the string.
        /// </summary>
        /// <param name="url">The URL.</param>
        /// <param name="httpMethod">The HTTP method.</param>
        /// <param name="request">The request.</param>
        /// <param name="requestEncoding">The request encoding.</param>
        /// <param name="responseEncoding">The response encoding.</param>
        /// <param name="callback">The callback.</param>
        /// <param name="userState">State of the user.</param>
        /// <returns></returns>
        /// <remarks></remarks>
        public static AsyncResult DownloadStringAsync(string url, HttpMethod httpMethod, NameValueCollection request,
                                                      Encoding requestEncoding, Encoding responseEncoding,
                                                      AsyncStringCallback callback, object userState)
        {
            return DownloadStringAsync(url, httpMethod, RequestToString(request, requestEncoding), requestEncoding,
                                       responseEncoding, callback, userState);
        }

        /// <summary>
        /// Downloads the string.
        /// </summary>
        /// <param name="url">The URL.</param>
        /// <param name="httpMethod">The HTTP method.</param>
        /// <param name="requestData">The request data.</param>
        /// <param name="callback">The callback.</param>
        /// <param name="userState">State of the user.</param>
        /// <returns></returns>
        /// <remarks></remarks>
        public static AsyncResult DownloadStringAsync(string url, HttpMethod httpMethod, string requestData,
                                                      AsyncStringCallback callback, object userState)
        {
            return DownloadStringAsync(url, httpMethod, requestData, Encoding.UTF8, Encoding.UTF8, callback, userState);
        }


        /// <summary>
        /// 下载网站内容
        /// </summary>
        /// <param name="url">The URL.</param>
        /// <param name="httpMethod">The HTTP method.</param>
        /// <param name="requestData">The request data.</param>
        /// <param name="requestEncoding">The request encoding.</param>
        /// <param name="responseEncoding">The response encoding.</param>
        /// <param name="callback">The callback.</param>
        /// <param name="userState">State of the user.</param>
        /// <returns></returns>
        /// <remarks></remarks>
        public static AsyncResult DownloadStringAsync(string url, HttpMethod httpMethod, string requestData,
                                                      Encoding requestEncoding, Encoding responseEncoding,
                                                      AsyncStringCallback callback, object userState)
        {
            var wrap = new StringWrap(callback, userState, responseEncoding);

            return DownloadDataAsync(url, httpMethod, requestData, requestEncoding, DownloadDataCallback, wrap);
        }

        private static void DownloadDataCallback(AsyncDataCompletedEventArgs e)
        {
            var wrap = e.UserState as StringWrap;
            if (wrap != null)
            {
                wrap.Callback(new AsyncStringCompletedEventArgs(e, wrap.ResponseEncoding, wrap.UserState));
            }
        }

        /// <summary>
        /// Downloads the data.
        /// </summary>
        /// <param name="url">The URL.</param>
        /// <param name="httpMethod">The HTTP method.</param>
        /// <param name="requestData">The request data.</param>
        /// <param name="requestEncoding">The request encoding.</param>
        /// <param name="callback">The callback.</param>
        /// <param name="userState">State of the user.</param>
        /// <returns></returns>
        /// <remarks></remarks>
        public static AsyncResult DownloadDataAsync(string url, HttpMethod httpMethod, string requestData,
                                                    Encoding requestEncoding, AsyncDataCallback callback,
                                                    object userState)
        {
            if (httpMethod == HttpMethod.POST)
            {
                return DownloadDataAsync(new Uri(url), httpMethod, requestEncoding.GetBytes(requestData), callback,
                                         userState);
            }
            if (!url.Contains("?") && !string.IsNullOrEmpty(requestData))
                url = url + "?" + HttpUtility.ParseQueryString(requestData, requestEncoding);
            else if (url.Contains("?") && !string.IsNullOrEmpty(requestData))
            {
                url = url + "&" + HttpUtility.ParseQueryString(requestData, requestEncoding);
            }
            return DownloadDataAsync(new Uri(url), httpMethod, new byte[] {}, callback, userState);
        }


        /// <summary>
        /// Downloads the data.
        /// </summary>
        /// <param name="url">The URL.</param>
        /// <param name="httpMethod">The HTTP method.</param>
        /// <param name="requestData">The request data.</param>
        /// <param name="callback">The callback.</param>
        /// <param name="userState">State of the user.</param>
        /// <returns></returns>
        /// <remarks></remarks>
        public static AsyncResult DownloadDataAsync(Uri url, HttpMethod httpMethod, byte[] requestData,
                                                    AsyncDataCallback callback, object userState)
        {
            var web = new WebClient();

            web.UploadDataCompleted += WebUploadDataCompleted;
            web.DownloadDataCompleted += WebDownloadDataCompleted;
            var wrap = new Wrap(callback, userState);
            web.Headers.Add("User-Agent", "Mozilla/5.0 (compatible; MSIE 9.0; Windows NT 6.1; WOW64; Trident/5.0; BOIE9;ZHCN)");
            if (httpMethod == HttpMethod.POST)
            {
                web.Headers.Add("Content-Type", "application/x-www-form-urlencoded");
                web.UploadDataAsync(url, "POST", requestData, wrap);
            }
            else
                web.DownloadDataAsync(url, wrap);

            return new AsyncResult(web);
        }

        /// <summary>
        /// Handles the DownloadDataCompleted event of the web control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.Net.DownloadDataCompletedEventArgs"/> instance containing the event data.</param>
        /// <remarks></remarks>
        private static void WebDownloadDataCompleted(object sender, DownloadDataCompletedEventArgs e)
        {
            var wrap = e.UserState as Wrap;
            if (wrap != null)
            {
                wrap.Callback(new AsyncDataCompletedEventArgs(e));
            }
        }

        private static void WebUploadDataCompleted(object sender, UploadDataCompletedEventArgs e)
        {
            var wrap = e.UserState as Wrap;
            if (wrap != null)
            {
                wrap.Callback(new AsyncDataCompletedEventArgs(e));
            }
        }

        #endregion

        #region Delegates

        ///<summary>
        /// 异步回调
        ///</summary>
        ///<param name="asyncCompletedEventArgs"></param>
        public delegate void AsyncDataCallback(AsyncDataCompletedEventArgs asyncCompletedEventArgs);

        ///<summary>
        /// 异步回调
        ///</summary>
        ///<param name="asyncCompletedEventArgs"></param>
        public delegate void AsyncStringCallback(AsyncStringCompletedEventArgs asyncCompletedEventArgs);

        #endregion

        

        /// <summary>
        /// 将键值集合转化成编码后的Url查询(转化成：a=%2d%20%3e&b=2&....)
        /// UTF-8编码
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public static string RequestToString(NameValueCollection request)
        {
            return RequestToString(request, Encoding.UTF8);
        }

        /// <summary>
        /// 将键值集合转化成编码后的Url查询(转化成：a=%2d%20%3e&b=2&....)
        /// 
        /// </summary>
        /// <param name="request"></param>
        /// <param name="encoding">编码</param>
        /// <returns></returns>
        public static string RequestToString(NameValueCollection request, Encoding encoding)
        {
            var sb = new StringBuilder();

            foreach (string k in request.Keys)
            {
                if (sb.Length > 0)
                {
                    sb.Append("&");
                }
                sb.Append(HttpUtility.UrlEncode(k, encoding) + "=" + HttpUtility.UrlEncode(request[k], encoding));
            }
            return sb.ToString();
        }

        #region Nested type: AsyncDataCompletedEventArgs

        /// <summary>
        /// 异步完成参数
        /// </summary>
        public class AsyncDataCompletedEventArgs
        {
            internal AsyncDataCompletedEventArgs(DownloadDataCompletedEventArgs e)
            {
                Cancelled = e.Cancelled;
                Error = e.Error;
                Result = e.Result;
                UserState = e.Result;
            }

            internal AsyncDataCompletedEventArgs(UploadDataCompletedEventArgs e)
            {
                Cancelled = e.Cancelled;
                Error = e.Error;
                Result = e.Result;
                UserState = e.Result;
            }

            /// <summary>
            /// Gets a value indicating whether this <see cref="AsyncDataCompletedEventArgs"/> is cancelled.
            /// </summary>
            /// <remarks></remarks>
            public bool Cancelled { get; private set; }

            /// <summary>
            /// Gets the error.
            /// </summary>
            /// <remarks></remarks>
            public Exception Error { get; private set; }

            /// <summary>
            /// 用户状态值
            /// </summary>
            public object UserState { get; private set; }

            /// <summary>
            /// Gets the result.
            /// </summary>
            /// <remarks></remarks>
            public byte[] Result { get; private set; }
        }

        #endregion

        #region Nested type: AsyncResult

        /// <summary>
        /// 异步回调结果
        /// </summary>
        public class AsyncResult : IDisposable
        {
            private readonly WebClient _webClient;

            internal AsyncResult(WebClient webClient)
            {
                _webClient = webClient;
            }

            /// <summary>
            /// 是否繁忙
            /// </summary>
            public bool IsBusy
            {
                get { return _webClient.IsBusy; }
            }

            #region IDisposable Members

            ///<summary>
            ///</summary>
            public void Dispose()
            {
                if (_webClient != null)
                {
                    _webClient.Dispose();
                }
            }

            #endregion

            /// <summary>
            /// 取消一个挂起的异步操作
            /// </summary>
            /// <remarks></remarks>
            public void Cancel()
            {
                _webClient.CancelAsync();
            }

            /// <summary>
            /// 终止线程
            /// </summary>
            public void Abort()
            {
                if (_webClient != null)
                {
                    _webClient.CancelAsync();
                    _webClient.Dispose();
                }
            }

            /// <summary>
            /// 等待线程结束
            /// </summary>
            public void Join(int millisecondsTimeout)
            {
                int milliseconds = 0;
                while (_webClient.IsBusy && milliseconds < millisecondsTimeout)
                {
                    Thread.Sleep(100);
                    milliseconds += 100;
                }
                Dispose();
            }
        }

        #endregion

        #region Nested type: AsyncStringCompletedEventArgs

        /// <summary>
        /// 异步完成参数
        /// </summary>
        public class AsyncStringCompletedEventArgs
        {
            internal AsyncStringCompletedEventArgs(AsyncDataCompletedEventArgs e, Encoding responseEncoding,
                                                   object userState)
            {
                Cancelled = e.Cancelled;
                Error = e.Error;
                Result = responseEncoding.GetString(e.Result);
                UserState = userState;
            }

            /// <summary>
            /// Gets a value indicating whether this <see cref="AsyncStringCompletedEventArgs"/> is cancelled.
            /// </summary>
            /// <remarks></remarks>
            public bool Cancelled { get; private set; }

            /// <summary>
            /// Gets the error.
            /// </summary>
            /// <remarks></remarks>
            public Exception Error { get; private set; }

            /// <summary>
            /// 用户状态值
            /// </summary>
            public object UserState { get; private set; }

            /// <summary>
            /// Gets the result.
            /// </summary>
            /// <remarks></remarks>
            public string Result { get; private set; }
        }

        #endregion

        #region Nested type: StringWrap

        /// <summary>
        /// 包装类
        /// </summary>
        internal class StringWrap
        {
            //public StringWrap(AsyncStringCallback callback, object userState)
            //{
            //    this.Callback = callback;
            //    this.UserState = userState;
            //}

            public StringWrap(AsyncStringCallback callback, object userState, Encoding responseEncoding)
            {
                Callback = callback;
                UserState = userState;
                ResponseEncoding = responseEncoding;
            }

            public Encoding ResponseEncoding { get; private set; }

            //public Encoding ResponseEncoding { get; set; }
            public object UserState { get; private set; }
            public AsyncStringCallback Callback { get; private set; }
        }

        #endregion

        #region Nested type: Wrap

        /// <summary>
        /// 包装类
        /// </summary>
        internal class Wrap
        {
            public Wrap(AsyncDataCallback callback, object userState)
            {
                Callback = callback;
                UserState = userState;
            }

            //public Encoding ResponseEncoding { get; set; }
            public object UserState { get; private set; }
            public AsyncDataCallback Callback { get; private set; }
        }

        #endregion
    }

    #region HttpMethod enum

    /// <summary>
    /// 请求方式
    /// </summary>
    public enum HttpMethod
    {
        ///<summary>
        /// GET
        ///</summary>
        GET,
        ///<summary>
        /// POST
        ///</summary>
        POST
    }

    #endregion
}