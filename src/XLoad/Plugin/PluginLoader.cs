namespace XLoad.Plugin
{
    using System;
    using System.IO;
    using System.Linq;
    using System.Reflection;

    public static class PluginLoader
    {
        public static Assembly LoadPlugin(string pluginPath)
        {
            PluginLoadContext loadContext = new PluginLoadContext(pluginPath);

            return loadContext.LoadFromAssemblyName(new AssemblyName(Path.GetFileNameWithoutExtension(pluginPath)));
        }

        public static IPlugin GetPluginFromAssembly(Assembly assembly)
        {
            var interfaceType = typeof(IPlugin);

            var types = assembly.GetTypes();

            return assembly.GetTypes()
                .Where(type => interfaceType.IsAssignableFrom(type))
                .Select(type => Activator.CreateInstance(type) as IPlugin)
                .Where(plugin => plugin != null)
                .FirstOrDefault();
        }
    }
}
