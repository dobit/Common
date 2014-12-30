using System;
using System.Net;

namespace System.Threading
{
    /// <summary>An asynchronous cache for downloaded HTML.</summary>
    public sealed class HtmlAsyncCache : AsyncCache<Uri, string>
    {
        /// <summary>Initializes the HtmlCache.</summary>
        public HtmlAsyncCache() : base(uri => new WebClient().DownloadStringTask(uri))
        {
        }
    }
}

