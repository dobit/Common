using System;
using System.ComponentModel;
using System.IO;
using System.Net;
using System.Threading.Tasks;
using System.Threading.Tasks;

namespace System.Net
{
    /// <summary>Extension methods for working with WebClient asynchronously.</summary>
    public static class WebClientExtensions
    {
        /// <summary>Downloads the resource with the specified URI as a byte array, asynchronously.</summary>
        /// <param name="webClient">The WebClient.</param>
        /// <param name="address">The URI from which to download data.</param>
        /// <returns>A Task that contains the downloaded data.</returns>
        public static Task<byte[]> DownloadDataTask(this WebClient webClient, string address)
        {
            return webClient.DownloadDataTask(new Uri(address));
        }

        /// <summary>Downloads the resource with the specified URI as a byte array, asynchronously.</summary>
        /// <param name="webClient">The WebClient.</param>
        /// <param name="address">The URI from which to download data.</param>
        /// <returns>A Task that contains the downloaded data.</returns>
        public static Task<byte[]> DownloadDataTask(this WebClient webClient, Uri address)
        {
            TaskCompletionSource<byte[]> tcs = new TaskCompletionSource<byte[]>(address);
            DownloadDataCompletedEventHandler handler = null;
            handler = delegate (object sender, DownloadDataCompletedEventArgs e) {
                EAPCommon.HandleCompletion<byte[]>(tcs, e, () => e.Result, delegate {
                    webClient.DownloadDataCompleted -= handler;
                });
            };
            webClient.DownloadDataCompleted += handler;
            try
            {
                webClient.DownloadDataAsync(address, tcs);
            }
            catch (Exception exception)
            {
                webClient.DownloadDataCompleted -= handler;
                tcs.TrySetException(exception);
            }
            return tcs.Task;
        }

        /// <summary>Downloads the resource with the specified URI to a local file, asynchronously.</summary>
        /// <param name="webClient">The WebClient.</param>
        /// <param name="address">The URI from which to download data.</param>
        /// <param name="fileName">The name of the local file that is to receive the data.</param>
        /// <returns>A Task that contains the downloaded data.</returns>
        public static Task DownloadFileTask(this WebClient webClient, string address, string fileName)
        {
            return webClient.DownloadFileTask(new Uri(address), fileName);
        }

        /// <summary>Downloads the resource with the specified URI to a local file, asynchronously.</summary>
        /// <param name="webClient">The WebClient.</param>
        /// <param name="address">The URI from which to download data.</param>
        /// <param name="fileName">The name of the local file that is to receive the data.</param>
        /// <returns>A Task that contains the downloaded data.</returns>
        public static Task DownloadFileTask(this WebClient webClient, Uri address, string fileName)
        {
            TaskCompletionSource<object> tcs = new TaskCompletionSource<object>(address);
            AsyncCompletedEventHandler handler = null;
            handler = delegate (object sender, AsyncCompletedEventArgs e) {
                EAPCommon.HandleCompletion<object>(tcs, e, () => null, delegate {
                    webClient.DownloadFileCompleted -= handler;
                });
            };
            webClient.DownloadFileCompleted += handler;
            try
            {
                webClient.DownloadFileAsync(address, fileName, tcs);
            }
            catch (Exception exception)
            {
                webClient.DownloadFileCompleted -= handler;
                tcs.TrySetException(exception);
            }
            return tcs.Task;
        }

        /// <summary>Downloads the resource with the specified URI as a string, asynchronously.</summary>
        /// <param name="webClient">The WebClient.</param>
        /// <param name="address">The URI from which to download data.</param>
        /// <returns>A Task that contains the downloaded string.</returns>
        public static Task<string> DownloadStringTask(this WebClient webClient, string address)
        {
            return webClient.DownloadStringTask(new Uri(address));
        }

        /// <summary>Downloads the resource with the specified URI as a string, asynchronously.</summary>
        /// <param name="webClient">The WebClient.</param>
        /// <param name="address">The URI from which to download data.</param>
        /// <returns>A Task that contains the downloaded string.</returns>
        public static Task<string> DownloadStringTask(this WebClient webClient, Uri address)
        {
            TaskCompletionSource<string> tcs = new TaskCompletionSource<string>(address);
            DownloadStringCompletedEventHandler handler = null;
            handler = delegate (object sender, DownloadStringCompletedEventArgs e) {
                EAPCommon.HandleCompletion<string>(tcs, e, () => e.Result, delegate {
                    webClient.DownloadStringCompleted -= handler;
                });
            };
            webClient.DownloadStringCompleted += handler;
            try
            {
                webClient.DownloadStringAsync(address, tcs);
            }
            catch (Exception exception)
            {
                webClient.DownloadStringCompleted -= handler;
                tcs.TrySetException(exception);
            }
            return tcs.Task;
        }

        /// <summary>Opens a readable stream for the data downloaded from a resource, asynchronously.</summary>
        /// <param name="webClient">The WebClient.</param>
        /// <param name="address">The URI for which the stream should be opened.</param>
        /// <returns>A Task that contains the opened stream.</returns>
        public static Task<Stream> OpenReadTask(this WebClient webClient, string address)
        {
            return webClient.OpenReadTask(new Uri(address));
        }

        /// <summary>Opens a readable stream for the data downloaded from a resource, asynchronously.</summary>
        /// <param name="webClient">The WebClient.</param>
        /// <param name="address">The URI for which the stream should be opened.</param>
        /// <returns>A Task that contains the opened stream.</returns>
        public static Task<Stream> OpenReadTask(this WebClient webClient, Uri address)
        {
            TaskCompletionSource<Stream> tcs = new TaskCompletionSource<Stream>(address);
            OpenReadCompletedEventHandler handler = null;
            handler = delegate (object sender, OpenReadCompletedEventArgs e) {
                EAPCommon.HandleCompletion<Stream>(tcs, e, () => e.Result, delegate {
                    webClient.OpenReadCompleted -= handler;
                });
            };
            webClient.OpenReadCompleted += handler;
            try
            {
                webClient.OpenReadAsync(address, tcs);
            }
            catch (Exception exception)
            {
                webClient.OpenReadCompleted -= handler;
                tcs.TrySetException(exception);
            }
            return tcs.Task;
        }

        /// <summary>Opens a writeable stream for uploading data to a resource, asynchronously.</summary>
        /// <param name="webClient">The WebClient.</param>
        /// <param name="address">The URI for which the stream should be opened.</param>
        /// <param name="method">The HTTP method that should be used to open the stream.</param>
        /// <returns>A Task that contains the opened stream.</returns>
        public static Task<Stream> OpenWriteTask(this WebClient webClient, string address, string method)
        {
            return webClient.OpenWriteTask(new Uri(address), method);
        }

        /// <summary>Opens a writeable stream for uploading data to a resource, asynchronously.</summary>
        /// <param name="webClient">The WebClient.</param>
        /// <param name="address">The URI for which the stream should be opened.</param>
        /// <param name="method">The HTTP method that should be used to open the stream.</param>
        /// <returns>A Task that contains the opened stream.</returns>
        public static Task<Stream> OpenWriteTask(this WebClient webClient, Uri address, string method)
        {
            TaskCompletionSource<Stream> tcs = new TaskCompletionSource<Stream>(address);
            OpenWriteCompletedEventHandler handler = null;
            handler = delegate (object sender, OpenWriteCompletedEventArgs e) {
                EAPCommon.HandleCompletion<Stream>(tcs, e, () => e.Result, delegate {
                    webClient.OpenWriteCompleted -= handler;
                });
            };
            webClient.OpenWriteCompleted += handler;
            try
            {
                webClient.OpenWriteAsync(address, method, tcs);
            }
            catch (Exception exception)
            {
                webClient.OpenWriteCompleted -= handler;
                tcs.TrySetException(exception);
            }
            return tcs.Task;
        }

        /// <summary>Uploads data to the specified resource, asynchronously.</summary>
        /// <param name="webClient">The WebClient.</param>
        /// <param name="address">The URI to which the data should be uploaded.</param>
        /// <param name="method">The HTTP method that should be used to upload the data.</param>
        /// <param name="data">The data to upload.</param>
        /// <returns>A Task containing the data in the response from the upload.</returns>
        public static Task<byte[]> UploadDataTask(this WebClient webClient, string address, string method, byte[] data)
        {
            return webClient.UploadDataTask(new Uri(address), method, data);
        }

        /// <summary>Uploads data to the specified resource, asynchronously.</summary>
        /// <param name="webClient">The WebClient.</param>
        /// <param name="address">The URI to which the data should be uploaded.</param>
        /// <param name="method">The HTTP method that should be used to upload the data.</param>
        /// <param name="data">The data to upload.</param>
        /// <returns>A Task containing the data in the response from the upload.</returns>
        public static Task<byte[]> UploadDataTask(this WebClient webClient, Uri address, string method, byte[] data)
        {
            TaskCompletionSource<byte[]> tcs = new TaskCompletionSource<byte[]>(address);
            UploadDataCompletedEventHandler handler = null;
            handler = delegate (object sender, UploadDataCompletedEventArgs e) {
                EAPCommon.HandleCompletion<byte[]>(tcs, e, () => e.Result, delegate {
                    webClient.UploadDataCompleted -= handler;
                });
            };
            webClient.UploadDataCompleted += handler;
            try
            {
                webClient.UploadDataAsync(address, method, data, tcs);
            }
            catch (Exception exception)
            {
                webClient.UploadDataCompleted -= handler;
                tcs.TrySetException(exception);
            }
            return tcs.Task;
        }

        /// <summary>Uploads a file to the specified resource, asynchronously.</summary>
        /// <param name="webClient">The WebClient.</param>
        /// <param name="address">The URI to which the file should be uploaded.</param>
        /// <param name="method">The HTTP method that should be used to upload the file.</param>
        /// <param name="fileName">A path to the file to upload.</param>
        /// <returns>A Task containing the data in the response from the upload.</returns>
        public static Task<byte[]> UploadFileTask(this WebClient webClient, string address, string method, string fileName)
        {
            return webClient.UploadFileTask(new Uri(address), method, fileName);
        }

        /// <summary>Uploads a file to the specified resource, asynchronously.</summary>
        /// <param name="webClient">The WebClient.</param>
        /// <param name="address">The URI to which the file should be uploaded.</param>
        /// <param name="method">The HTTP method that should be used to upload the file.</param>
        /// <param name="fileName">A path to the file to upload.</param>
        /// <returns>A Task containing the data in the response from the upload.</returns>
        public static Task<byte[]> UploadFileTask(this WebClient webClient, Uri address, string method, string fileName)
        {
            TaskCompletionSource<byte[]> tcs = new TaskCompletionSource<byte[]>(address);
            UploadFileCompletedEventHandler handler = null;
            handler = delegate (object sender, UploadFileCompletedEventArgs e) {
                EAPCommon.HandleCompletion<byte[]>(tcs, e, () => e.Result, delegate {
                    webClient.UploadFileCompleted -= handler;
                });
            };
            webClient.UploadFileCompleted += handler;
            try
            {
                webClient.UploadFileAsync(address, method, fileName, tcs);
            }
            catch (Exception exception)
            {
                webClient.UploadFileCompleted -= handler;
                tcs.TrySetException(exception);
            }
            return tcs.Task;
        }

        /// <summary>Uploads data in a string to the specified resource, asynchronously.</summary>
        /// <param name="webClient">The WebClient.</param>
        /// <param name="address">The URI to which the data should be uploaded.</param>
        /// <param name="method">The HTTP method that should be used to upload the data.</param>
        /// <param name="data">The data to upload.</param>
        /// <returns>A Task containing the data in the response from the upload.</returns>
        public static Task<string> UploadStringTask(this WebClient webClient, string address, string method, string data)
        {
            return webClient.UploadStringTask(new Uri(address), method, data);
        }

        /// <summary>Uploads data in a string to the specified resource, asynchronously.</summary>
        /// <param name="webClient">The WebClient.</param>
        /// <param name="address">The URI to which the data should be uploaded.</param>
        /// <param name="method">The HTTP method that should be used to upload the data.</param>
        /// <param name="data">The data to upload.</param>
        /// <returns>A Task containing the data in the response from the upload.</returns>
        public static Task<string> UploadStringTask(this WebClient webClient, Uri address, string method, string data)
        {
            TaskCompletionSource<string> tcs = new TaskCompletionSource<string>(address);
            UploadStringCompletedEventHandler handler = null;
            handler = delegate (object sender, UploadStringCompletedEventArgs e) {
                EAPCommon.HandleCompletion<string>(tcs, e, () => e.Result, delegate {
                    webClient.UploadStringCompleted -= handler;
                });
            };
            webClient.UploadStringCompleted += handler;
            try
            {
                webClient.UploadStringAsync(address, method, data, tcs);
            }
            catch (Exception exception)
            {
                webClient.UploadStringCompleted -= handler;
                tcs.TrySetException(exception);
            }
            return tcs.Task;
        }
    }
}

