using System;
using System.IO;
using Microsoft.Win32;

namespace LFNet.Common.Helpers
{
    /// <summary>
    /// A class with file helper methods
    /// </summary>
    public static class FileHelper
    {
        private const string MimeKey = @"MIME\Database\Content Type";

        private static string GetCommon(string fileExtension)
        {
            switch (fileExtension)
            {
                case ".png":
                    return "image/png";

                case ".gif":
                    return "image/gif";

                case ".jpeg":
                case ".jpg":
                    return "image/jpeg";

                case ".bmp":
                    return "image/bmp";

                case ".tif":
                case ".tiff":
                    return "image/tiff";

                case ".html":
                case ".htm":
                    return "text/html";

                case ".xml":
                    return "text/xml";

                case ".css":
                    return "text/css";

                case ".js":
                    return "text/plain";

                case ".zip":
                    return "application/zip";

                case ".gz":
                    return "application/x-gzip";

                case ".txt":
                    return "text/plain";

                case ".xsd":
                case ".xslt":
                    return "application/xml";
            }
            return string.Empty;
        }

        /// <summary>
        /// Gets the content type based on file extension .
        /// </summary>
        /// <param name="fileExtension">The file extension.</param>
        /// <returns>The content type.</returns>
        public static string GetContentType(string fileExtension)
        {
            if (string.IsNullOrEmpty(fileExtension))
            {
                throw new ArgumentNullException("fileExtension");
            }
            string common = GetCommon(fileExtension);
            if (string.IsNullOrEmpty(common))
            {
                common = GetFromRegistry(fileExtension);
            }
            return common;
        }

        private static string GetFromRegistry(string fileExtension)
        {
            string str = string.Empty;
            RegistryKey classesRoot = Registry.ClassesRoot;
            fileExtension = fileExtension.ToLower();
            using (RegistryKey key2 = classesRoot.OpenSubKey(fileExtension, false))
            {
                if (key2 != null)
                {
                    str = key2.GetValue("Content Type", string.Empty) as string;
                }
                if (!string.IsNullOrEmpty(str))
                {
                    return str;
                }
            }
            using (RegistryKey key3 = classesRoot.OpenSubKey(@"MIME\Database\Content Type", false))
            {
                foreach (string str2 in key3.GetSubKeyNames())
                {
                    string name = Path.Combine(@"MIME\Database\Content Type", str2);
                    using (RegistryKey key4 = classesRoot.OpenSubKey(name, false))
                    {
                        string str4 = key4.GetValue("Extension", string.Empty) as string;
                        if (fileExtension.Equals(str4, StringComparison.OrdinalIgnoreCase))
                        {
                            return str2;
                        }
                    }
                }
            }
            return str;
        }
    }
}

