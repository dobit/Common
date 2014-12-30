using System;
using System.IO;
using System.Net;
using System.Threading.Tasks;

namespace System.Net
{
    /// <summary>Extension methods for working with WebRequest asynchronously.</summary>
    public static class WebRequestExtensions
    {
        /// <summary>Creates a Task that respresents downloading all of the data from a WebRequest.</summary>
        /// <param name="webRequest">The WebRequest.</param>
        /// <returns>A Task containing the downloaded content.</returns>
        public static Task<byte[]> DownloadDataAsync(this WebRequest webRequest)
        {
            return webRequest.GetResponseAsync().ContinueWith<Task<byte[]>>(response => response.Result.GetResponseStream().ReadAllBytesAsync()).Unwrap<byte[]>();
        }

        /// <summary>Creates a Task that represents an asynchronous request to GetRequestStream.</summary>
        /// <param name="webRequest">The WebRequest.</param>
        /// <returns>A Task containing the retrieved Stream.</returns>
        public static Task<Stream> GetRequestStreamAsync(this WebRequest webRequest)
        {
            if (webRequest == null)
            {
                throw new ArgumentNullException("webRequest");
            }
            return Task<Stream>.Factory.FromAsync(new Func<AsyncCallback, object, IAsyncResult>(webRequest.BeginGetRequestStream), new Func<IAsyncResult, Stream>(webRequest.EndGetRequestStream), webRequest);
        }

        /// <summary>Creates a Task that represents an asynchronous request to GetResponse.</summary>
        /// <param name="webRequest">The WebRequest.</param>
        /// <returns>A Task containing the retrieved WebResponse.</returns>
        public static Task<WebResponse> GetResponseAsync(this WebRequest webRequest)
        {
            if (webRequest == null)
            {
                throw new ArgumentNullException("webRequest");
            }
            return Task<WebResponse>.Factory.FromAsync(new Func<AsyncCallback, object, IAsyncResult>(webRequest.BeginGetResponse), new Func<IAsyncResult, WebResponse>(webRequest.EndGetResponse), webRequest);
        }
    }
}

