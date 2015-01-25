using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Reflection;
using System.Text;
using Autofac;
using Autofac.Configuration;

namespace LFNet.Services
{
    /// <summary>
    /// 配置目录
    /// </summary>
    public static class ServiceContainer
    {
        public static ContainerBuilder Builder = new ContainerBuilder();
        private static IContainer _container;

        public static IContainer Container
        {
            get
            {
                if (_container == null)
                {
                    _container = BuildContainer();
                }
                return _container;
            }
          
        }
        public static event EventHandler<ContainerBuildingEventArgs> ContainerBuilding;
        public static event EventHandler<ContainerBuildedEventArgs> ContainerBuilded;

        private static void OnContainerBuilded(IContainer container)
        {
            EventHandler<ContainerBuildedEventArgs> handler = ContainerBuilded;
            if (handler != null) handler(null, new ContainerBuildedEventArgs(container));
        }

        private static void OnContainerBuilding(ContainerBuilder containerBuilder)
        {
            EventHandler<ContainerBuildingEventArgs> handler = ContainerBuilding;
            if (handler != null) handler(null, new ContainerBuildingEventArgs(containerBuilder));
        }


        public static void RegisterTypeAssignableTo<T>(bool singleInstance = false)
        {
            Assembly[] ass;

            if (System.Web.HttpRuntime.AppDomainAppId != null)
            {
                ass = System.Web.Compilation.BuildManager.GetReferencedAssemblies().Cast<Assembly>().ToArray();
            }
            else
            {
                ass = System.AppDomain.CurrentDomain.GetAssemblies();
            }

            var register = Builder.RegisterAssemblyTypes(ass).AssignableTo<T>().AsImplementedInterfaces();
            if (singleInstance) register.SingleInstance();
        }
        public static void RegisterType<T, AsType>(bool singleInstance = false)
        {
            var register = Builder.RegisterType<T>().As<AsType>();
            if (singleInstance) register.SingleInstance();
        }

        public static void RegisterType<T>(bool singleInstance = false)
        {
            var register = Builder.RegisterType<T>();
            if (singleInstance) register.SingleInstance();
        }

        public static void RegisterType(Type implementationType, bool singleInstance = false)
        {
            var register = Builder.RegisterType(implementationType);
            if (singleInstance) register.SingleInstance();
        }

        public static void RegisterFromConfigurationSettings(string sectionName = "autofac")
        {
            Builder.RegisterModule(new ConfigurationSettingsReader("autofac"));
        }


        private static IContainer BuildContainer()
        {
            if (System.Web.HttpRuntime.AppDomainAppId != null || System.Web.HttpContext.Current != null)
            {
                // ASP.NET 網站應用程式
                Builder.RegisterAssemblyTypes(System.Web.Compilation.BuildManager.GetReferencedAssemblies().Cast<Assembly>().ToArray()).AssignableTo<IService>().AsImplementedInterfaces().SingleInstance();
            }
            else
            {
                // Windows 應用程式或 Windows 服務
                //    Builder.RegisterAssemblyTypes(System.AppDomain.CurrentDomain.GetAssemblies().Where(p => p.Location.StartsWith(System.AppDomain.CurrentDomain.BaseDirectory, StringComparison.OrdinalIgnoreCase)).ToArray()).AssignableTo<IService>().AsImplementedInterfaces().SingleInstance();
                Builder.RegisterAssemblyTypes(System.AppDomain.CurrentDomain.GetAssemblies()).AssignableTo<IService>().AsImplementedInterfaces().SingleInstance();

            }
            OnContainerBuilding(Builder);
            IContainer container = Builder.Build();
            OnContainerBuilded(container);
            return container;
        }

        public static object Resolve(Type serviceType)
        {
            return Container.Resolve(serviceType);
        }

        public static T Resolve<T>()
        {
                return Container.Resolve<T>();
        }

        public static object ResolveNamed(string serviceName, Type serviceType)
        {
            return Container.ResolveNamed(serviceName, serviceType);
        }

        //public static void Rebuild()
        //{
        //  ContainerBuilder   containerBuilder=new ContainerBuilder();
        //  BuildContainer(containerBuilder);
        //    Builder = containerBuilder;

        //}

        /// <summary>
        /// 更新
        /// </summary>
        public static void Update()
        {
            Builder.Update(_container);
        }
    }

    public class ContainerBuildingEventArgs : EventArgs
    {
        public ContainerBuilder ContainerBuilder { get; private set; }
        

        public ContainerBuildingEventArgs(ContainerBuilder containerBuilder)
        {
            ContainerBuilder = containerBuilder;
        }
    }
    public class ContainerBuildedEventArgs : EventArgs
    {
        public IContainer Container { get; private set; }


        public ContainerBuildedEventArgs(IContainer container)
        {
            Container = container;
        }
    }
}
