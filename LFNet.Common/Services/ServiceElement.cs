using System.Xml.Serialization;

namespace LFNet.Services
{

    /// <summary>
    /// Element describing a service exposed by a component.
    /// </summary>
    public class ServiceElement
    {

        /// <summary>
        /// Gets the service type.
        /// </summary>
        /// <value>The type.</value>
        [XmlAttribute]
        public string Type { get; set; }

        /// <summary>
        /// Gets the service name.
        /// </summary>
        /// <value>The name.</value>
        [XmlAttribute]
        public string Name { get; set; }
    }

}
