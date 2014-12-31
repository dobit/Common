using System;
using System.IO;
using System.Net;
using System.Reflection;
using System.Security.Cryptography.X509Certificates;
using System.Security.Permissions;
//#define NET4 
using System.Threading;
using LFNet.Configuration;

namespace LFNet.Common.Net
{
    /// <summary>
    /// 可以减少传输次数的web请求
    /// </summary>
    public class JHttpWebRequest
    {
        private static FieldInfo _isFromCacheFieldInfo;
        private static PropertyInfo _responseStreamPropertyInfo;

        static JHttpWebRequest()
        {
            Type responseType = typeof(HttpWebResponse);
            Type responsebaseType = typeof(WebResponse);
            _isFromCacheFieldInfo = responsebaseType.GetField("m_IsFromCache", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.DeclaredOnly | BindingFlags.SetField);
            _responseStreamPropertyInfo = responseType.GetProperty("ResponseStream",
                                                                   BindingFlags.Instance | BindingFlags.NonPublic |
                                                                   BindingFlags.DeclaredOnly | BindingFlags.SetField);
        }
        private HttpWebRequest _webRequest;
        private HttpWebResponse _response;
        private string _localFileName;
        public string LocalFileName
        {
            get
            {
                if (string.IsNullOrEmpty(_localFileName))
                    _localFileName = GetfileName(this.RequestUri.ToString());
                return _localFileName;
            }
            set { _localFileName = value; }
        }

        public JHttpWebRequest(Uri requestUri)
        {
            if (!requestUri.Scheme.StartsWith("http:", StringComparison.OrdinalIgnoreCase) && !requestUri.Scheme.StartsWith("https:", StringComparison.OrdinalIgnoreCase))
            {
                throw new NotSupportedException("不支持");
            }
            _webRequest = (HttpWebRequest)WebRequest.Create(requestUri);
        }

        public JHttpWebRequest(string requestUriString)
        {
            if (!requestUriString.StartsWith("http://", StringComparison.OrdinalIgnoreCase) && !requestUriString.StartsWith("https://", StringComparison.OrdinalIgnoreCase))
            {
                throw new NotSupportedException("不支持");
            }
            _webRequest = (HttpWebRequest)WebRequest.Create(requestUriString);
        }
        public bool AllowAutoRedirect
        {
            get { return _webRequest.AllowAutoRedirect; }
            set
            {
                _webRequest.AllowAutoRedirect = value;
            }
        }
        public bool AllowWriteStreamBuffering
        {
            get
            {
                return _webRequest.AllowWriteStreamBuffering;
            }
            set
            {
                _webRequest.AllowWriteStreamBuffering = value;
            }
        }

        //public bool AllowReadStreamBuffering
        //{
        //    get
        //    {
        //        return false;
        //    }
        //    set { _webRequest.AllowReadStreamBuffering = value; }
        //}

        public bool HaveResponse
        {
            get { return _webRequest.HaveResponse; }
        }

        public bool KeepAlive
        {
            get
            {
                return _webRequest.KeepAlive;
            }
            set
            {
                _webRequest.KeepAlive = value;
            }
        }

        public bool Pipelined
        {
            get
            {
                return _webRequest.Pipelined;
            }
            set
            {
                _webRequest.Pipelined = value;
            }
        }
        public bool PreAuthenticate
        {
            get
            {
                return _webRequest.PreAuthenticate;
            }
            set
            {
                _webRequest.PreAuthenticate = value;
            }
        }

        public bool UnsafeAuthenticatedConnectionSharing
        {
            get
            {
                return _webRequest.UnsafeAuthenticatedConnectionSharing;
            }
            set
            {
                _webRequest.UnsafeAuthenticatedConnectionSharing = value;
            }
        }

        public bool SendChunked
        {
            get
            {
                return _webRequest.SendChunked;
            }
            set { _webRequest.SendChunked = value; }
        }

        public DecompressionMethods AutomaticDecompression
        {
            get
            {
                return _webRequest.AutomaticDecompression;
            }
            set
            {
                _webRequest.AutomaticDecompression = value;
            }
        }

        public int MaximumResponseHeadersLength
        {
            get
            {
                return _webRequest.MaximumResponseHeadersLength;
            }
            set
            {
                _webRequest.MaximumResponseHeadersLength = value;
            }
        }

        public X509CertificateCollection ClientCertificates
        {
            get
            {

                return _webRequest.ClientCertificates;
            }
            set
            {
                if (value == null)
                {
                    throw new ArgumentNullException("value");
                }
                _webRequest.ClientCertificates = value;
            }
        }
        public CookieContainer CookieContainer
        {
            get
            {
                return _webRequest.CookieContainer;
            }
            set
            {
                _webRequest.CookieContainer = value;
            }
        }
        //public bool SupportsCookieContainer
        //{
        //    get
        //    {
        //        return _webRequest.SupportsCookieContainer;
        //    }
        //}
        public Uri RequestUri
        {
            get
            {
                return _webRequest.RequestUri;
            }
        }
        public long ContentLength
        {
            get
            {
                return _webRequest.ContentLength;
            }
            set
            {

                _webRequest.ContentLength = value;
            }
        }
        public int Timeout
        {
            get
            {
                return _webRequest.Timeout;
            }
            set { _webRequest.Timeout = value; }
        }

        public int ReadWriteTimeout
        {
            get
            {
                return _webRequest.ReadWriteTimeout;
            }
            set
            {
                _webRequest.ReadWriteTimeout = value;
            }
        }
        public Uri Address
        {
            get
            {
                return _webRequest.Address;
            }
        }
        public HttpContinueDelegate ContinueDelegate
        {
            get
            {
                return _webRequest.ContinueDelegate;
            }
            set
            {
                _webRequest.ContinueDelegate = value;
            }
        }
        public ServicePoint ServicePoint
        {
            get
            {
                return _webRequest.ServicePoint;
            }
        }
        //public string Host
        //{
        //    get
        //    {
        //        return _webRequest.Host;
        //    }
        //    set
        //    {
        //        _webRequest.Host=value;
        //    }
        //}

        public int MaximumAutomaticRedirections
        {
            get
            {
                return _webRequest.MaximumAutomaticRedirections;
            }
            set
            {

                _webRequest.MaximumAutomaticRedirections = value;
            }
        }
        public string Method
        {
            get
            {
                return _webRequest.Method;
            }
            set
            {

                _webRequest.Method = value;
            }
        }

        public ICredentials Credentials
        {
            get
            {
                return _webRequest.Credentials;
            }
            set
            {
                _webRequest.Credentials = value;
            }
        }
        public bool UseDefaultCredentials
        {
            get
            {
                return _webRequest.UseDefaultCredentials;
            }
            set
            {
                _webRequest.UseDefaultCredentials = value;
            }
        }

        public string ConnectionGroupName
        {
            get
            {
                return _webRequest.ConnectionGroupName;
            }
            set
            {
                _webRequest.ConnectionGroupName = value;
            }
        }

        public WebHeaderCollection Headers
        {
            get
            {
                return _webRequest.Headers;
            }
            set { _webRequest.Headers = value; }
        }
        public IWebProxy Proxy
        {
            get
            {

                return _webRequest.Proxy;
            }
            set
            {

                _webRequest.Proxy = value;
            }
        }

        public Version ProtocolVersion
        {
            get
            {

                return _webRequest.ProtocolVersion;
            }
            set { _webRequest.ProtocolVersion = value; }
        }
        public string ContentType
        {
            get { return _webRequest.ContentType; }
            set { _webRequest.ContentType = value; }
        }
        public string MediaType
        {
            get
            {
                return _webRequest.MediaType;
            }
            set
            {
                _webRequest.MediaType = value;
            }
        }
        public string TransferEncoding
        {
            get
            {
                return _webRequest.TransferEncoding;
            }
            set { _webRequest.TransferEncoding = value; }
        }
        public string Connection
        {
            get
            {
                return _webRequest.Connection;
            }
            set
            {
                _webRequest.Connection = value;
            }
        }
        public string Accept
        {
            get
            {
                return _webRequest.Accept;
            }
            set
            {
                _webRequest.Accept = value;
            }
        }
        public string Referer
        {
            get { return _webRequest.Referer; }
            set { _webRequest.Referer = value; }
        }
        public string UserAgent
        {
            get
            {
                return _webRequest.UserAgent;
            }
            set { _webRequest.UserAgent = value; }
        }
        public string Expect
        {
            get { return _webRequest.Expect; }
            set { _webRequest.Expect = value; }
        }
        public DateTime IfModifiedSince
        {
            get { return _webRequest.IfModifiedSince; }
            set
            {
                _webRequest.IfModifiedSince = value;
            }
        }
        //public DateTime Date
        //{
        //    get
        //    {
        //        return _webRequest.Date;
        //    }
        //    set { _webRequest.Date = value; }
        //}

        public IAsyncResult BeginGetRequestStream(AsyncCallback callback, object state)
        {
            return _webRequest.BeginGetRequestStream(callback, state);
        }
        public Stream EndGetRequestStream(IAsyncResult asyncResult)
        {
            return _webRequest.EndGetRequestStream(asyncResult);
        }
        public Stream EndGetRequestStream(IAsyncResult asyncResult, out TransportContext context)
        {
            return _webRequest.EndGetRequestStream(asyncResult, out context); ;
        }
        public Stream GetRequestStream()
        {
            return _webRequest.GetRequestStream();
        }
        public Stream GetRequestStream(out TransportContext context)
        {
            return _webRequest.GetRequestStream(out  context);
        }

        [HostProtection(SecurityAction.LinkDemand, ExternalThreading = true)]
        public IAsyncResult BeginGetResponse(AsyncCallback callback, object state)
        {
            return _webRequest.BeginGetResponse(callback, state);
        }
        public WebResponse EndGetResponse(IAsyncResult asyncResult)
        {
            return _webRequest.EndGetResponse(asyncResult);
        }

        public void AddRange(int from, int to)
        {
            _webRequest.AddRange(from, to);
        }
        //public void AddRange(long from, long to)
        //{
        //    _webRequest.AddRange(from, to);
        //}
        public void AddRange(int range)
        {
            _webRequest.AddRange(range);
        }
        //public void AddRange(long range)
        //{
        //    _webRequest.AddRange(range);
        //}
        public void AddRange(string rangeSpecifier, int from, int to)
        {
            _webRequest.AddRange(rangeSpecifier, from, to);
        }
        //public void AddRange(string rangeSpecifier, long from, long to)
        //{
        //    _webRequest.AddRange(rangeSpecifier, from, to);
        //}
        public void AddRange(string rangeSpecifier, int range)
        {
            _webRequest.AddRange(rangeSpecifier, range);
        }
        //public void AddRange(string rangeSpecifier, long range)
        //{
        //    _webRequest.AddRange(rangeSpecifier, range);
        //}



        public HttpWebResponse GetResponse()
        {
            if (ConfigFileManager.GetConfig<NetConfig>().UseCache && _webRequest.Method.Equals("GET", StringComparison.OrdinalIgnoreCase))
            {
                global::System.IO.FileInfo fileInfo = new FileInfo(LocalFileName); //lockHttpFileHelper.FileInfo;
                if (fileInfo.Exists && fileInfo.Length > 0)
                {
                    _webRequest.IfModifiedSince = fileInfo.LastWriteTime;

                }
                global::System.Net.HttpWebResponse response;
                try
                {
                    response = (global::System.Net.HttpWebResponse)_webRequest.GetResponse();
                }
                catch (WebException exception)
                {
                    response = (HttpWebResponse)exception.Response;
                    if (response.StatusCode != HttpStatusCode.NotModified)
                    {
                        response.Close();
                        throw;
                    }
                }

                if (response.StatusCode == HttpStatusCode.NotModified) //未改变
                {

                    _isFromCacheFieldInfo.SetValue(response, true);
                    Stream fileStream = GetFileStream(fileInfo.FullName);
                   
                        _responseStreamPropertyInfo.SetValue(response, fileStream, null);
                }
                else
                {
                    if (!fileInfo.Directory.Exists)
                    {
                        fileInfo.Directory.Create();
                    }
                    HttpFileStream httpStream = new HttpFileStream(response, fileInfo);
                    _responseStreamPropertyInfo.SetValue(response, httpStream, null);

                }
                _response = response;
                return response;
            }

            _response= _webRequest.GetResponse() as HttpWebResponse;
            return _response;

        }

        private static Stream GetFileStream(string filename)
        {
            try
            {
                return new FileStream(filename,FileMode.Open,FileAccess.Read,FileShare.Read);
            }
            catch (Exception)
            {
                Thread.Sleep(10);
                return new FileStream(filename, FileMode.Open, FileAccess.Read, FileShare.Read);
               
            }
        }

        /// <summary>
        /// 直接返回流,如果文件存在且配置为不检查更新则会返回文件流
        /// </summary>
        /// <returns></returns>
        public Stream GetResponseStream()
        {
            if (!ConfigFileManager.GetConfig<NetConfig>().CheckUpdate && File.Exists(LocalFileName))
            {
                try
                {
                    return GetFileStream(LocalFileName);
                }
                catch (Exception)
                {
                }
            }
            return GetResponse().GetResponseStream();
        }

        private static string GetfileName(string url)
        {
            if (url.EndsWith("/")) url += ".default";
            return ConfigFileManager.GetConfig<NetConfig>().TempPath + url.Replace("http://", "");
        }

       
    }

   
}