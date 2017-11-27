using System.Reflection;
using Autofac;

namespace UBeat.Crm.CoreApi.Core
{
    public static class CoreApiRegistEngine
    {
        public static void RegisterImplemented(ContainerBuilder builder, string assemblyName,string endWithName)
        {
            var assembly = Assembly.Load(new AssemblyName(assemblyName));
            // Registers both modules
            builder.RegisterAssemblyModules(assembly);

            // Registers both interfaces
            builder.RegisterAssemblyTypes(assembly)
            .Where(t => t.Name.EndsWith(endWithName)).AsImplementedInterfaces().SingleInstance().PropertiesAutowired();
        }

        public static void RegisterServices(ContainerBuilder builder, string assemblyName, string endWithName)
        {
            var assembly = Assembly.Load(new AssemblyName(assemblyName));
            // Registers both modules
            builder.RegisterAssemblyModules(assembly);
            
            // Registers both interfaces
            builder.RegisterAssemblyTypes(assembly)
            .Where(t => t.Name.EndsWith(endWithName)).SingleInstance().PropertiesAutowired();
        }
    }
}
