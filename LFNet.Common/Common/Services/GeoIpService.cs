using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Threading.Tasks;
using System.Web.Script.Serialization;
using System.Net;

namespace LFNet.Common.Services
{
    public class GeoIpService
    {
        public static GeoLocationInfo GetLocationInfo(string ipaddress)
        {
            HttpWebRequest webRequest = (HttpWebRequest) WebRequest.Create(string.Format("http://freegeoip.net/json/{0}", ipaddress));
            webRequest.Method = "GET";
            webRequest.Accept = "application/json";
            Task<WebResponse> responseAsync = webRequest.GetResponseAsync();
            if (!responseAsync.Wait(TimeSpan.FromSeconds(5.0)))
            {
                return null;
            }
            if (!responseAsync.IsCompleted || (responseAsync.Result == null))
            {
                return null;
            }
            using (StreamReader reader = new StreamReader(responseAsync.Result.GetResponseStream()))
            {
                string input = reader.ReadToEnd();
                Dictionary<string, string> dictionary = new JavaScriptSerializer().Deserialize<Dictionary<string, string>>(input);
                if (dictionary == null)
                {
                    return null;
                }
                return new GeoLocationInfo { City = dictionary["city"], RegionName = dictionary["region_name"], RegionCode = dictionary["region_code"], CountryCode = dictionary["country_code"], CountryName = dictionary["country_name"] };
            }
        }
    }
}

