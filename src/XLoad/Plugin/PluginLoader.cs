namespace XLoad.Plugin
{
    using Plugin;
    using System;
    using System.IO;
    using System.Linq;
    using System.Reflection;

    public static class PluginLoader
    {
        public static Assembly LoadPlugin(string pluginPath)
        {
            var assembly = Assembly.LoadFile(pluginPath);

            return assembly;
        }

        public static IPlugin GetPluginFromAssembly(Assembly assembly)
        {
            var interfaceType = typeof(IPlugin);
            
            return assembly.GetTypes()
                .Where(type => interfaceType.IsAssignableFrom(type))
                .Select(type => Activator.CreateInstance(type) as IPlugin)
                .Where(plugin => plugin != null)
                .FirstOrDefault();
        }
    }
}
