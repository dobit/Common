using System.Xml.Serialization;

namespace LFNet.Services
{

    /// <summary>
    /// Element describing a component property.
    /// </summary>
    public class MetadataElement 
    {
        /// <summary>
        /// Gets the name of the property.
        /// </summary>
        /// <value>The name.</value>
       [XmlAttribute]
        public string Name { get; set; }

        /// <summary>
        /// Gets the value to be set (will be converted.)
        /// </summary>
        /// <value>The value.</value>
        [XmlAttribute]
        public string Value { get; set; }

        /// <summary>
        /// Gets the type of the value.
        /// </summary>
        /// <value>The type of the value. Defaults to string if not specified.</value>
        [XmlAttribute]
        public string Type { get; set; }
    }

}
