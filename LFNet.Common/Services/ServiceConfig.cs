using System.Collections.Generic;
using LFNet.Configuration;
using System.Xml.Serialization;
namespace LFNet.Services
{

    public class ServiceConfig:BaseConfig<ServiceConfig>
    {
        private List<ModuleElement> _modules=new List<ModuleElement>();

        /// <summary>
        /// Gets the modules to be registered.
        /// </summary>
        /// <value>The modules.</value>
        public List<ModuleElement> Modules
        {
            get { return _modules; }
            set { _modules = value; }
        }

        private List<ComponentElement> _components = new List<ComponentElement>();// { new ComponentElement() { Type = "LFNet.DataProviders.SqlDataProvider,LFNet.EduDoc.Data.SqlServer", InstanceScope = "single-instance", Service = "LFNet.DataProviders.DataProvider,LFNet.EduDoc" } };

        /// <summary>
        /// Gets the components to be registered.
        /// </summary>
        /// <value>The components.</value>
        public List<ComponentElement> Components
        {
            get { return _components; }
            set { _components = value; }
        }

        private List<string> _files=new List<string>();

        /// <summary>
        /// Gets additional configuration files.
        /// </summary>
        /// <value>The files.</value>
        public List<string> Files
        {
            get { return _files; }
            set { _files = value; }
        }

        /// <summary>
        /// Gets the default assembly to search for types in when not explicitly
        /// provided with the type name.
        /// </summary>
        /// <value>The default assembly.</value>
        [XmlAttribute]
        public string DefaultAssembly { get; set; }
    }

}