using System.Xml.Serialization;

namespace LFNet.Services
{

    /// <summary>
    /// Element describing a component constructor parameter.
    /// </summary>
    public class ParameterElement
    {
        /// <summary>
        /// Gets the name of the parameter.
        /// </summary>
        /// <value>The name.</value>
        [XmlAttribute]
        public string Name { get; set; }

        /// <summary>
        /// Gets the value used to set the parameter (type will be converted.)
        /// </summary>
        /// <value>The value.</value>
        [XmlAttribute]
        public string Value { get; set; }
    }

}
