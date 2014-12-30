using System;
using System.IO;
using Microsoft.Win32;

namespace LFNet.Common.Win32
{
    public class ContentType
    {
        private const string MimeKey = @"MIME\Database\Content Type";

        private static string GetByCommonExtension(string contentType)
        {
            switch (contentType)
            {
                case "image/png":
                    return ".png";

                case "image/gif":
                    return ".gif";

                case "image/jpeg":
                    return ".jpeg";

                case "image/bmp":
                    return ".bmp";

                case "image/tiff":
                    return ".tiff";

                case "text/html":
                    return ".html";

                case "text/xml":
                    return ".xml";

                case "text/css":
                    return ".css";

                case "text/plain":
                    return ".txt";

                case "application/zip":
                    return ".zip";

                case "application/x-gzip":
                    return ".gz";
            }
            return string.Empty;
        }

        /// <summary>
        /// Gets the content type based on file extension .
        /// </summary>
        /// <param name="fileExtension">The file extension.</param>
        /// <returns>The content type.</returns>
        public static string GetByExtension(string fileExtension)
        {
            if (string.IsNullOrEmpty(fileExtension))
            {
                throw new ArgumentNullException("fileExtension");
            }
            string contentType = GetContentType(fileExtension);
            if (string.IsNullOrEmpty(contentType))
            {
                contentType = GetContentTypeFromRegistry(fileExtension);
            }
            return contentType;
        }

        private static string GetContentType(string fileExtension)
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

        private static string GetContentTypeFromRegistry(string fileExtension)
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

        public static string GetExtension(string contentType)
        {
            if (contentType == null)
            {
                throw new ArgumentNullException("contentType");
            }
            string byCommonExtension = GetByCommonExtension(contentType);
            if (string.IsNullOrEmpty(byCommonExtension))
            {
                byCommonExtension = GetExtensionFromRegistry(contentType);
            }
            return byCommonExtension;
        }

        private static string GetExtensionFromRegistry(string contentType)
        {
            using (RegistryKey key = Registry.ClassesRoot.OpenSubKey(Path.Combine(@"MIME\Database\Content Type", contentType), false))
            {
                if (key != null)
                {
                    return (string) key.GetValue("Extension", string.Empty);
                }
            }
            return string.Empty;
        }

        public static class Image
        {
            public const string Gif = "image/gif";
            public const string Jpeg = "image/jpeg";
            public const string Png = "image/png";
            public const string Tiff = "image/tiff";
        }

        public static class Multipart
        {
            public const string Alternative = "multipart/alternative";
            public const string FormData = "multipart/form-data";
            public const string Mixed = "multipart/mixed";
            public const string Related = "multipart/related";
        }

        /// <summary>
        /// Human-readable text and source code content types
        /// </summary>
        public static class Text
        {
            /// <summary>Cascading Style Sheets; text/css</summary>
            public const string Css = "text/css ";
            /// <summary>RichText; text/enriched</summary>
            public const string Enriched = "text/enriched";
            /// <summary>Html; text/html</summary>
            public const string Html = "text/html";
            /// <summary>JavaScript; text/javascript</summary>
            public const string JavaScript = "text/javascript ";
            /// <summary>Plain Text; text/plain</summary>
            public const string Plain = "text/plain";
            /// <summary>RichText; text/richtext</summary>
            public const string RichText = "text/richtext";
            /// <summary>Sgml; text/sgml</summary>
            public const string Sgml = "text/sgml";
            /// <summary>Xml; text/xml</summary>
            public const string Xml = "text/xml";
        }
    }
}

