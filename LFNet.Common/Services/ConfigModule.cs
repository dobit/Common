using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Globalization;
using System.Linq;
using System.Reflection;
using Autofac;
using Autofac.Builder;
using Autofac.Core;
using Autofac.Core.Activators.Reflection;
using Container = Autofac.Core.Container;
using Module = Autofac.Module;

namespace LFNet.Services
{
    internal class ConfigModule: Module
    {
        private ServiceConfig _serviceConfig;

        /// <summary>
        /// Initializes a new instance of the <see cref="ConfigModule"/> class.
        /// 
        /// </summary>
        public ConfigModule()
            : this(ServiceConfig.Instance)
        {

        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ConfigModule"/> class.
        /// </summary>
        /// <param name="serviceConfig">A instance of the service configuration.</param>
        public ConfigModule(ServiceConfig serviceConfig)
        {
            // TODO: Complete member initialization
            this._serviceConfig = serviceConfig;
        }
        /// <summary>
        /// Override to add registrations to the container.
        /// </summary>
        /// <param name="builder">The builder.</param>
        protected override void Load(ContainerBuilder builder)
        {
            Enforce.ArgumentNotNull(builder, "builder");

            Assembly defaultAssembly = null;
            if (!string.IsNullOrEmpty(_serviceConfig.DefaultAssembly))
            {
                defaultAssembly = Assembly.Load(_serviceConfig.DefaultAssembly);
            }

            foreach (ModuleElement moduleElement in _serviceConfig.Modules)
            {
                var moduleType = LoadType(moduleElement.Type, defaultAssembly);
                var moduleActivator = new ReflectionActivator(
                    moduleType,
                    new  BindingFlagsConstructorFinder(BindingFlags.Public),
                    new MostParametersConstructorSelector(),
                    moduleElement.Parameters.ToParameters(),
                    moduleElement.Properties.ToParameters());
                var module = (IModule)moduleActivator.ActivateInstance(Container.Empty, Enumerable.Empty<Parameter>());
                builder.RegisterModule(module);
            }

            foreach (ComponentElement component in _serviceConfig.Components)
            {
                var registrar = builder.RegisterType(LoadType(component.Type, defaultAssembly));

                IList<Service> services = new List<Service>();
                if (!string.IsNullOrEmpty(component.Service))
                {
                    var serviceType = LoadType(component.Service, defaultAssembly);
                    if (!string.IsNullOrEmpty(component.Name))
                        services.Add(new KeyedService(component.Name, serviceType));
                    else
                        services.Add(new TypedService(serviceType));
                }
                else
                {
                    if (!string.IsNullOrEmpty(component.Name))
                        throw new ConfigurationErrorsException(string.Format(
                            "Service Type Must Be Specified:{0}", component.Name));
                }

                foreach (ServiceElement service in component.Services)
                {
                    var serviceType = LoadType(service.Type, defaultAssembly);
                    if (!string.IsNullOrEmpty(service.Name))
                        services.Add(new KeyedService(service.Name, serviceType));
                    else
                        services.Add(new TypedService(serviceType));
                }

                foreach (var service in services)
                    registrar.As(service);

                foreach (var param in component.Parameters.ToParameters())
                    registrar.WithParameter(param);

                foreach (var prop in component.Properties.ToParameters())
                    registrar.WithProperty(prop);

                foreach (var ep in component.Metadata)
                    registrar.WithMetadata(
                        ep.Name, TypeManipulation.ChangeToCompatibleType(ep.Value, Type.GetType(ep.Type)));

                if (!string.IsNullOrEmpty(component.MemberOf))
                    registrar.MemberOf(component.MemberOf);

                SetScope(component, registrar);
                SetOwnership(component, registrar);
                SetInjectProperties(component, registrar);
            }

            //foreach (FileElement file in _serviceConfig.Files)
            //{
            //    var section = DefaultSectionName;
            //    if (!string.IsNullOrEmpty(file.Section))
            //        section = file.Section;

            //    var reader = new ConfigurationSettingsReader(section, file.Name);
            //    builder.RegisterModule(reader);
            //}
        }

        /// <summary>
        /// Sets the property injection mode for the component.
        /// </summary>
        /// <param name="component">The component.</param>
        /// <param name="registrar">The registrar.</param>
        protected virtual void SetInjectProperties<TReflectionActivatorData, TSingleRegistrationStyle>(ComponentElement component, IRegistrationBuilder<object, TReflectionActivatorData, TSingleRegistrationStyle> registrar)
            where TReflectionActivatorData : ReflectionActivatorData
            where TSingleRegistrationStyle : SingleRegistrationStyle
        {
            //Enforce.ArgumentNotNull(component, "component");
            //Enforce.ArgumentNotNull(registrar, "registrar");

            if (!string.IsNullOrEmpty(component.InjectProperties))
            {
                switch (component.InjectProperties.ToLower())
                {
                    case "no":
                        break;
                    case "yes":
                        registrar.PropertiesAutowired( (PropertyWiringOptions) 1);
                        break;
                    default:
                        throw new ConfigurationErrorsException(string.Format(CultureInfo.CurrentCulture,
                            "UnrecognisedInject Properties:{0}", component.InjectProperties));
                }
            }
        }

        /// <summary>
        /// Sets the ownership model of the component.
        /// </summary>
        /// <param name="component">The component.</param>
        /// <param name="registrar">The registrar.</param>
        protected virtual void SetOwnership<TReflectionActivatorData, TSingleRegistrationStyle>(ComponentElement component, IRegistrationBuilder<object, TReflectionActivatorData, TSingleRegistrationStyle> registrar)
            where TReflectionActivatorData : ReflectionActivatorData
            where TSingleRegistrationStyle : SingleRegistrationStyle
        {
            //Enforce.ArgumentNotNull(component, "component");
            //Enforce.ArgumentNotNull(registrar, "registrar");

            if (!string.IsNullOrEmpty(component.Ownership))
            {
                switch (component.Ownership.ToLower())
                {
                    case "lifetime-scope":
                        registrar.OwnedByLifetimeScope();
                        break;
                    case "external":
                        registrar.ExternallyOwned();
                        break;
                    default:
                        throw new ConfigurationErrorsException(string.Format(CultureInfo.CurrentCulture,
                            "Unrecognised Ownership:{0}", component.Ownership));
                }
            }
        }

        /// <summary>
        /// Sets the scope model for the component.
        /// </summary>
        /// <param name="component">The component.</param>
        /// <param name="registrar">The registrar.</param>
        protected virtual void SetScope<TReflectionActivatorData, TSingleRegistrationStyle>(ComponentElement component, IRegistrationBuilder<object, TReflectionActivatorData, TSingleRegistrationStyle> registrar)
            where TReflectionActivatorData : ReflectionActivatorData
            where TSingleRegistrationStyle : SingleRegistrationStyle
        {
            //Enforce.ArgumentNotNull(component, "component");
            //Enforce.ArgumentNotNull(registrar, "registrar");

            if (!string.IsNullOrEmpty(component.InstanceScope))
            {
                switch (component.InstanceScope.ToLower())
                {
                    case "single-instance":
                        registrar.SingleInstance();
                        break;
                    case "per-lifetime-scope":
                        registrar.InstancePerLifetimeScope();
                        break;
                    case "per-dependency":
                        registrar.InstancePerDependency();
                        break;
                    default:
                        throw new ConfigurationErrorsException(string.Format(CultureInfo.CurrentCulture,
                            "Unrecognised Scope:{0}", component.InstanceScope));
                }
            }
        }
        /// <summary>
        /// Loads the type.
        /// </summary>
        /// <param name="typeName">Name of the type.</param>
        /// <param name="defaultAssembly">The default assembly.</param>
        /// <returns></returns>
        protected virtual Type LoadType(string typeName, Assembly defaultAssembly)
        {
            if (typeName == string.Empty)
                throw new ArgumentOutOfRangeException("typeName");

            Type type = Type.GetType(typeName);

            if (type == null && defaultAssembly != null)
                type = defaultAssembly.GetType(typeName, false); // Don't throw on error.

            if (type == null)
                throw new ConfigurationErrorsException(string.Format(CultureInfo.CurrentCulture,
                    "Type Not Found:{0}", typeName));

            return type;
        }
    }

    
    
}