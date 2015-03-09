using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Xml;
using System.Xml.Linq;
using System.Xml.XPath;
using LFNet.Common.IO;

namespace LFNet.Common.Helpers
{
    public class XmlHelper
    {
        public static string FormatXml(string xml)
        {
            XElement element = XElement.Parse(xml);
            XmlWriterSettings settings = new XmlWriterSettings {
                Indent = true,
                Encoding = Encoding.UTF8
            };
            StringBuilder sb = new StringBuilder();
            using (StringEncodedWriter writer = new StringEncodedWriter(Encoding.UTF8, sb))
            {
                XmlWriter writer2 = XmlWriter.Create(writer, settings);
                element.WriteTo(writer2);
               
            }
            return sb.ToString();
        }

        public static string GetFirstNamespace(string xml)
        {
            IDictionary<string, string> namespaces = GetNamespaces(xml);
            if (namespaces != null)
            {
                foreach (KeyValuePair<string, string> pair in namespaces)
                {
                    return pair.Value;
                }
            }
            return string.Empty;
        }

        public static IDictionary<string, string> GetNamespaces(string xml)
        {
            StringReader input = new StringReader(xml);
            using (XmlReader reader2 = XmlReader.Create(input))
            {
                XPathNavigator navigator = XElement.Load(reader2).CreateNavigator();
                navigator.MoveToFirstChild();
                return navigator.GetNamespacesInScope(XmlNamespaceScope.Local);
            }
        }
    }
}

