using System.Collections.Generic;
using System.Xml.Serialization;
using Autofac.Core;

namespace LFNet.Services
{
    /// <summary>
    /// Element describing a module registration.
    /// </summary>
    public class ModuleElement
    {
        /// <summary>
        /// Gets the type of the module. Must expose <see cref="IModule"/>.
        /// </summary>
        /// <value>The type.</value>
        [XmlAttribute]
        public string Type { get; set; }

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
    }
}