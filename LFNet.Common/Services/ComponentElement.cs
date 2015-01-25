
using System.Collections.Generic;
using System.Xml.Serialization;
namespace LFNet.Services
{
    /// <summary>
    /// Element describing a component registration.
    /// </summary>
    public class ComponentElement
    {


        /// <summary>
        /// Gets the type of the component.
        /// </summary>
        /// <value>The type.</value>
        [XmlAttribute]
        public string Type { get; set; }

        /// <summary>
        /// Gets the service exposed by the component. For multiple-service components,
        /// use the services element instead.
        /// </summary>
        /// <value>The service.</value>
        [XmlAttribute]
        public string Service { get; set; }

        /// <summary>
        /// Allows the component to be added to another composite component.
        /// </summary>
        /// <value>The name of the composite component.</value>
        [XmlAttribute]
        public string MemberOf { get; set; }

        /// <summary>
        /// Allows the component to be added to another composite component.
        /// </summary>
        /// <value>The name of the composite component.</value>
        [XmlAttribute]
        public string Name { get; set; }

        /// <summary>
        /// Sets the scope of the component instances.
        /// </summary>
        /// <value>singleton (default,) factory or container.</value>
        [XmlAttribute]
        public string InstanceScope { get; set; }

        /// <summary>
        /// Sets the ownership over the component instances.
        /// </summary>
        /// <value>container (default) or external.</value>
        [XmlAttribute]
        public string Ownership { get; set; }

        /// <summary>
        /// Sets up property injection for the component instances. This uses the
        /// OnActivated event so that circular dependencies can be handled.
        /// </summary>
        /// <value>never (default,) all, unset.</value>
        [XmlAttribute]
        public string InjectProperties { get; set; }

        private List<ServiceElement> _services=new List<ServiceElement>();

        /// <summary>
        /// Gets the services exposed by the component.
        /// </summary>
        /// <value>The services.</value>
        public List<ServiceElement> Services
        {
            get { return _services; }
            set { _services = value; }
        }

        private List<ParameterElement> _parameters=new List<ParameterElement>();

        /// <summary>
        /// Gets the parameters used to construct the component.
        /// </summary>
        /// <value>The parameters.</value>
        public List<ParameterElement> Parameters
        {
            get { return _parameters; }
            set { _parameters = value; }
        }

        private List<PropertyElement> _properties=new List<PropertyElement>();

        /// <summary>
        /// Gets the properties to be explicitly set on the component.
        /// </summary>
        /// <value>The explicit properties.</value>
        public List<PropertyElement> Properties
        {
            get { return _properties; }
            set { _properties = value; }
        }

        private List<MetadataElement> _metadata=new List<MetadataElement>();

        /// <summary>
        /// Gets the extended properties associated with the registration.
        /// </summary>
        /// <value>The extended properties.</value>
        public List<MetadataElement> Metadata
        {
            get { return _metadata; }
            set { _metadata = value; }
        }
    }
}