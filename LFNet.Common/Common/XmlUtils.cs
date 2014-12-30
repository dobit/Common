using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Xml;
using System.Xml.Serialization;
using System.Xml.XPath;
using System.Xml.Xsl;

namespace LFNet.Common
{
    /// <summary>
    /// 
    /// </summary>
    /// <remarks></remarks>
    public class XmlUtils
    {
        /// <summary>
        /// Appends the element.
        /// </summary>
        /// <param name="objDoc">The obj doc.</param>
        /// <param name="objNode">The obj node.</param>
        /// <param name="attName">Name of the att.</param>
        /// <param name="attValue">The att value.</param>
        /// <param name="includeIfEmpty">if set to <c>true</c> [include if empty].</param>
        /// <remarks></remarks>
        public static void AppendElement(ref XmlDocument objDoc, XmlNode objNode, string attName, string attValue,
                                         bool includeIfEmpty)
        {
            AppendElement(ref objDoc, objNode, attName, attValue, includeIfEmpty, false);
        }

        /// <summary>
        /// Appends the element.
        /// </summary>
        /// <param name="objDoc">The obj doc.</param>
        /// <param name="objNode">The obj node.</param>
        /// <param name="attName">Name of the att.</param>
        /// <param name="attValue">The att value.</param>
        /// <param name="includeIfEmpty">if set to <c>true</c> [include if empty].</param>
        /// <param name="CDATA">if set to <c>true</c> [CDATA].</param>
        /// <remarks></remarks>
        public static void AppendElement(ref XmlDocument objDoc, XmlNode objNode, string attName, string attValue,
                                         bool includeIfEmpty, bool CDATA)
        {
            if (String.IsNullOrEmpty(attValue) && !includeIfEmpty)
            {
                return;
            }
            if (CDATA)
            {
                objNode.AppendChild(CreateCDataElement(objDoc, attName, attValue));
            }
            else
            {
                objNode.AppendChild(CreateElement(objDoc, attName, attValue));
            }
        }

        /// <summary>
        /// Creates the attribute.
        /// </summary>
        /// <param name="objDoc">The obj doc.</param>
        /// <param name="attName">Name of the att.</param>
        /// <param name="attValue">The att value.</param>
        /// <returns></returns>
        /// <remarks></remarks>
        public static XmlAttribute CreateAttribute(XmlDocument objDoc, string attName, string attValue)
        {
            XmlAttribute attribute = objDoc.CreateAttribute(attName);
            attribute.Value = attValue;
            return attribute;
        }

        /// <summary>
        /// Creates the attribute.
        /// </summary>
        /// <param name="objDoc">The obj doc.</param>
        /// <param name="objNode">The obj node.</param>
        /// <param name="attName">Name of the att.</param>
        /// <param name="attValue">The att value.</param>
        /// <remarks></remarks>
        public static void CreateAttribute(XmlDocument objDoc, XmlNode objNode, string attName, string attValue)
        {
            XmlAttribute attribute = objDoc.CreateAttribute(attName);
            attribute.Value = attValue;
            objNode.Attributes.Append(attribute);
        }


        /// <summary>
        /// Creates the element.
        /// </summary>
        /// <param name="objDoc">The obj doc.</param>
        /// <param name="nodeName">Name of the node.</param>
        /// <param name="nodeValue">The node value.</param>
        /// <returns></returns>
        /// <remarks></remarks>
        public static XmlElement CreateElement(XmlDocument objDoc, string nodeName, string nodeValue)
        {
            XmlElement element = objDoc.CreateElement(nodeName);
            element.InnerText = nodeValue;
            return element;
        }

        /// <summary>
        /// Creates the C data element.
        /// </summary>
        /// <param name="objDoc">The obj doc.</param>
        /// <param name="nodeName">Name of the node.</param>
        /// <param name="nodeValue">The node value.</param>
        /// <returns></returns>
        /// <remarks></remarks>
        public static XmlElement CreateCDataElement(XmlDocument objDoc, string nodeName, string nodeValue)
        {
            XmlElement element = objDoc.CreateElement(nodeName);
            element.AppendChild(objDoc.CreateCDataSection(nodeValue));
            return element;
        }


        /// <summary>
        /// Deserializes the specified obj stream.
        /// </summary>
        /// <param name="objStream">The obj stream.</param>
        /// <param name="type">The type.</param>
        /// <returns></returns>
        /// <remarks></remarks>
        public static object Deserialize(Stream objStream, Type type)
        {
            //object obj = Activator.CreateInstance(type);
            //Dictionary<int, TabInfo> tabDic = obj as Dictionary<int, TabInfo>;
            //if (tabDic != null)
            //{
            //    obj = DeSerializeDictionary<TabInfo>(objStream, "dictionary");
            //    return obj;
            //}
            //Dictionary<int, ModuleInfo> moduleDic = obj as Dictionary<int, ModuleInfo>;
            //if (moduleDic != null)
            //{
            //    obj = DeSerializeDictionary<ModuleInfo>(objStream, "dictionary");
            //    return obj;
            //}
            //Dictionary<int, TabPermissionCollection> tabPermDic = obj as Dictionary<int, TabPermissionCollection>;
            //if (tabPermDic != null)
            //{
            //    obj = DeSerializeDictionary<TabPermissionCollection>(objStream, "dictionary");
            //    return obj;
            //}
            //Dictionary<int, ModulePermissionCollection> modPermDic = obj as Dictionary<int, ModulePermissionCollection>;
            //if (modPermDic != null)
            //{
            //    obj = DeSerializeDictionary<ModulePermissionCollection>(objStream, "dictionary");
            //    return obj;
            //}
            var serializer = new XmlSerializer(type);
            TextReader tr = new StreamReader(objStream);
            object obj = serializer.Deserialize(tr);
            tr.Close();
            return obj;
        }

        /// <summary>
        /// Des the serialize dictionary.
        /// </summary>
        /// <typeparam name="TValue">The type of the value.</typeparam>
        /// <param name="objStream">The obj stream.</param>
        /// <param name="rootname">The rootname.</param>
        /// <returns></returns>
        /// <remarks></remarks>
        public static Dictionary<int, TValue> DeSerializeDictionary<TValue>(Stream objStream, string rootname)
        {
            var xmlDoc = new XmlDocument();
            xmlDoc.Load(objStream);
            var objDictionary = new Dictionary<int, TValue>();
            foreach (XmlElement xmlItem in xmlDoc.SelectNodes(rootname + "/item"))
            {
                int key = Convert.ToInt32(xmlItem.GetAttribute("key"));
                string typeName = xmlItem.GetAttribute("type");
                var objValue = Activator.CreateInstance<TValue>();
                var xser = new XmlSerializer(objValue.GetType());
                var reader = new XmlTextReader(new StringReader(xmlItem.InnerXml));
                objDictionary.Add(key, (TValue) xser.Deserialize(reader));
            }
            return objDictionary;
        }

        /// <summary>
        /// Des the serialize hashtable.
        /// </summary>
        /// <param name="xmlSource">The XML source.</param>
        /// <param name="rootname">The rootname.</param>
        /// <returns></returns>
        /// <remarks></remarks>
        public static Hashtable DeSerializeHashtable(string xmlSource, string rootname)
        {
            Hashtable objHashTable;
            if (!String.IsNullOrEmpty(xmlSource))
            {
                objHashTable = new Hashtable();
                var xmlDoc = new XmlDocument();
                xmlDoc.LoadXml(xmlSource);
                foreach (XmlElement xmlItem in xmlDoc.SelectNodes(rootname + "/item"))
                {
                    string key = xmlItem.GetAttribute("key");
                    string typeName = xmlItem.GetAttribute("type");
                    var xser = new XmlSerializer(Type.GetType(typeName));
                    var reader = new XmlTextReader(new StringReader(xmlItem.InnerXml));
                    objHashTable.Add(key, xser.Deserialize(reader));
                }
            }
            else
            {
                objHashTable = new Hashtable();
            }
            return objHashTable;
        }

        /// <summary>
        /// Gets the attribute value.
        /// </summary>
        /// <param name="nav">The nav.</param>
        /// <param name="attributeName">Name of the attribute.</param>
        /// <returns></returns>
        /// <remarks></remarks>
        public static string GetAttributeValue(XPathNavigator nav, string attributeName)
        {
            return nav.GetAttribute(attributeName, "");
        }

        /// <summary>
        /// Gets the attribute value as integer.
        /// </summary>
        /// <param name="nav">The nav.</param>
        /// <param name="attributeName">Name of the attribute.</param>
        /// <param name="defaultValue">The default value.</param>
        /// <returns></returns>
        /// <remarks></remarks>
        public static int GetAttributeValueAsInteger(XPathNavigator nav, string attributeName, int defaultValue)
        {
            int intValue = defaultValue;
            string strValue = GetAttributeValue(nav, attributeName);
            if (!string.IsNullOrEmpty(strValue))
            {
                intValue = Convert.ToInt32(strValue);
            }
            return intValue;
        }

        /// <summary>
        /// Gets the node value.
        /// </summary>
        /// <param name="nav">The nav.</param>
        /// <param name="path">The path.</param>
        /// <returns></returns>
        /// <remarks></remarks>
        public static string GetNodeValue(XPathNavigator nav, string path)
        {
            string strValue = Null.NullString;
            XPathNavigator elementNav = nav.SelectSingleNode(path);
            if (elementNav != null)
            {
                strValue = elementNav.Value;
            }
            return strValue;
        }

        /// <summary>
        /// Gets the node value.
        /// </summary>
        /// <param name="objNode">The obj node.</param>
        /// <param name="nodeName">Name of the node.</param>
        /// <param name="defaultValue">The default value.</param>
        /// <returns></returns>
        /// <remarks></remarks>
        public static string GetNodeValue(XmlNode objNode, string nodeName, string defaultValue)
        {
            string strValue = defaultValue;
            if ((objNode[nodeName] != null))
            {
                strValue = objNode[nodeName].InnerText;
                if (String.IsNullOrEmpty(strValue) && !String.IsNullOrEmpty(defaultValue))
                {
                    strValue = defaultValue;
                }
            }
            return strValue;
        }

        /// <summary>
        /// Gets the node value boolean.
        /// </summary>
        /// <param name="objNode">The obj node.</param>
        /// <param name="nodeName">Name of the node.</param>
        /// <returns></returns>
        /// <remarks></remarks>
        public static bool GetNodeValueBoolean(XmlNode objNode, string nodeName)
        {
            return GetNodeValueBoolean(objNode, nodeName, false);
        }

        /// <summary>
        /// Gets the node value boolean.
        /// </summary>
        /// <param name="objNode">The obj node.</param>
        /// <param name="nodeName">Name of the node.</param>
        /// <param name="defaultValue">if set to <c>true</c> [default value].</param>
        /// <returns></returns>
        /// <remarks></remarks>
        public static bool GetNodeValueBoolean(XmlNode objNode, string nodeName, bool defaultValue)
        {
            bool bValue = defaultValue;
            if ((objNode[nodeName] != null))
            {
                string strValue = objNode[nodeName].InnerText;
                if (!string.IsNullOrEmpty(strValue))
                {
                    bValue = Convert.ToBoolean(strValue);
                }
            }
            return bValue;
        }

        /// <summary>
        /// Gets the node value date.
        /// </summary>
        /// <param name="objNode">The obj node.</param>
        /// <param name="nodeName">Name of the node.</param>
        /// <param name="defaultValue">The default value.</param>
        /// <returns></returns>
        /// <remarks></remarks>
        public static DateTime GetNodeValueDate(XmlNode objNode, string nodeName, DateTime defaultValue)
        {
            DateTime dateValue = defaultValue;
            if ((objNode[nodeName] != null))
            {
                string strValue = objNode[nodeName].InnerText;
                if (!string.IsNullOrEmpty(strValue))
                {
                    dateValue = Convert.ToDateTime(strValue);
                    if (dateValue.Date.Equals(Null.NullDate.Date))
                    {
                        dateValue = Null.NullDate;
                    }
                }
            }
            return dateValue;
        }

        /// <summary>
        /// Gets the node value int.
        /// </summary>
        /// <param name="objNode">The obj node.</param>
        /// <param name="nodeName">Name of the node.</param>
        /// <returns></returns>
        /// <remarks></remarks>
        public static int GetNodeValueInt(XmlNode objNode, string nodeName)
        {
            return GetNodeValueInt(objNode, nodeName, 0);
        }

        /// <summary>
        /// Gets the node value int.
        /// </summary>
        /// <param name="objNode">The obj node.</param>
        /// <param name="nodeName">Name of the node.</param>
        /// <param name="defaultValue">The default value.</param>
        /// <returns></returns>
        /// <remarks></remarks>
        public static int GetNodeValueInt(XmlNode objNode, string nodeName, int defaultValue)
        {
            int intValue = defaultValue;
            if ((objNode[nodeName] != null))
            {
                string strValue = objNode[nodeName].InnerText;
                if (!string.IsNullOrEmpty(strValue))
                {
                    intValue = Convert.ToInt32(strValue);
                }
            }
            return intValue;
        }

        /// <summary>
        /// Gets the node value single.
        /// </summary>
        /// <param name="objNode">The obj node.</param>
        /// <param name="nodeName">Name of the node.</param>
        /// <returns></returns>
        /// <remarks></remarks>
        public static float GetNodeValueSingle(XmlNode objNode, string nodeName)
        {
            return GetNodeValueSingle(objNode, nodeName, 0);
        }

        /// <summary>
        /// Gets the node value single.
        /// </summary>
        /// <param name="objNode">The obj node.</param>
        /// <param name="nodeName">Name of the node.</param>
        /// <param name="defaultValue">The default value.</param>
        /// <returns></returns>
        /// <remarks></remarks>
        public static float GetNodeValueSingle(XmlNode objNode, string nodeName, float defaultValue)
        {
            float sValue = defaultValue;
            if ((objNode[nodeName] != null))
            {
                string strValue = objNode[nodeName].InnerText;
                if (!string.IsNullOrEmpty(strValue))
                {
                    sValue = Convert.ToSingle(strValue, CultureInfo.InvariantCulture);
                }
            }
            return sValue;
        }

        /// <summary>
        /// Gets the XML writer settings.
        /// </summary>
        /// <param name="conformance">The conformance.</param>
        /// <returns></returns>
        /// <remarks></remarks>
        public static XmlWriterSettings GetXmlWriterSettings(ConformanceLevel conformance)
        {
            var settings = new XmlWriterSettings();
            settings.ConformanceLevel = conformance;
            settings.OmitXmlDeclaration = true;
            settings.Indent = true;
            return settings;
        }

        /// <summary>
        /// Serializes the dictionary.
        /// </summary>
        /// <param name="source">The source.</param>
        /// <param name="rootName">Name of the root.</param>
        /// <returns></returns>
        /// <remarks></remarks>
        public static string SerializeDictionary(IDictionary source, string rootName)
        {
            string strString;
            if (source.Count != 0)
            {
                XmlSerializer xser;
                StringWriter sw;
                var xmlDoc = new XmlDocument();
                XmlElement xmlRoot = xmlDoc.CreateElement(rootName);
                xmlDoc.AppendChild(xmlRoot);
                foreach (object key in source.Keys)
                {
                    XmlElement xmlItem = xmlDoc.CreateElement("item");
                    xmlItem.SetAttribute("key", Convert.ToString(key));
                    xmlItem.SetAttribute("type", source[key].GetType().AssemblyQualifiedName);
                    var xmlObject = new XmlDocument();
                    xser = new XmlSerializer(source[key].GetType());
                    sw = new StringWriter();
                    xser.Serialize(sw, source[key]);
                    xmlObject.LoadXml(sw.ToString());
                    xmlItem.AppendChild(xmlDoc.ImportNode(xmlObject.DocumentElement, true));
                    xmlRoot.AppendChild(xmlItem);
                }
                strString = xmlDoc.OuterXml;
            }
            else
            {
                strString = "";
            }
            return strString;
        }

        /// <summary>
        /// Serializes the hashtable.
        /// </summary>
        /// <param name="hashtable">The hashtable.</param>
        /// <param name="xmlDocument">The XML document.</param>
        /// <param name="rootNode">The root node.</param>
        /// <param name="elementName">Name of the element.</param>
        /// <param name="keyField">The key field.</param>
        /// <param name="valueField">The value field.</param>
        /// <remarks></remarks>
        public static void SerializeHashtable(Hashtable hashtable, XmlDocument xmlDocument, XmlNode rootNode,
                                              string elementName, string keyField, string valueField)
        {
            string sKey;
            XmlNode nodeSetting;
            XmlNode nodeSettingName;
            XmlNode nodeSettingValue;

            string sOuterElementName = elementName + "s";
            string sInnerElementName = elementName;

            XmlNode nodeSettings = rootNode.AppendChild(xmlDocument.CreateElement(sOuterElementName));
            foreach (string sKeyLoopVariable in hashtable.Keys)
            {
                sKey = sKeyLoopVariable;
                nodeSetting = nodeSettings.AppendChild(xmlDocument.CreateElement(sInnerElementName));
                nodeSettingName = nodeSetting.AppendChild(xmlDocument.CreateElement(keyField));
                nodeSettingName.InnerText = sKey;
                nodeSettingValue = nodeSetting.AppendChild(xmlDocument.CreateElement(valueField));
                nodeSettingValue.InnerText = hashtable[sKey].ToString();
            }
        }


        /// <summary>
        /// Updates the attribute.
        /// </summary>
        /// <param name="node">The node.</param>
        /// <param name="attName">Name of the att.</param>
        /// <param name="attValue">The att value.</param>
        /// <remarks></remarks>
        public static void UpdateAttribute(XmlNode node, string attName, string attValue)
        {
            if ((node != null))
            {
                XmlAttribute attrib = node.Attributes[attName];
                attrib.InnerText = attValue;
            }
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Xml Encodes HTML
        /// </summary>
        /// <param name="html">The HTML to encode</param>
        /// <returns></returns>
        public static string XmlEncode(string html)
        {
            return "<![CDATA[" + html + "]]>";
        }


        /// <summary>
        /// XSLs the transform.
        /// </summary>
        /// <param name="doc">The doc.</param>
        /// <param name="writer">The writer.</param>
        /// <param name="xsltUrl">The XSLT URL.</param>
        /// <remarks></remarks>
        public static void XSLTransform(XmlDocument doc, ref StreamWriter writer, string xsltUrl)
        {
            var xslt = new XslCompiledTransform();
            xslt.Load(xsltUrl);
            //Transform the file.
            xslt.Transform(doc, null, writer);
        }

        /// <summary>
        /// Serializes the specified obj.
        /// </summary>
        /// <param name="obj">The obj.</param>
        /// <returns></returns>
        /// <remarks></remarks>
        public static string Serialize(object obj)
        {
            string xmlObject;
            var dic = obj as IDictionary;
            if ((dic != null))
            {
                xmlObject = SerializeDictionary(dic, "dictionary");
            }
            else
            {
                var xmlDoc = new XmlDocument();
                var xser = new XmlSerializer(obj.GetType());
                var sw = new StringWriter();

                xser.Serialize(sw, obj);

                xmlDoc.LoadXml(sw.GetStringBuilder().ToString());
                XmlNode xmlDocEl = xmlDoc.DocumentElement;
                xmlDocEl.Attributes.Remove(xmlDocEl.Attributes["xmlns:xsd"]);
                xmlDocEl.Attributes.Remove(xmlDocEl.Attributes["xmlns:xsi"]);

                xmlObject = xmlDocEl.OuterXml;
            }
            return xmlObject;
        }
    }
}