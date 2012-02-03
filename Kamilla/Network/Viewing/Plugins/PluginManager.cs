using System;
using System.Collections.Generic;

namespace Kamilla.Network.Viewing.Plugins
{
    public static class PluginManager
    {
        static bool s_initialized;
        static Type[] s_plugins;

        public static INetworkLogViewerPlugin[] CreatePluginSet()
        {
            if (!s_initialized)
                Initialize();

            var count = s_plugins.Length;
            var result = new INetworkLogViewerPlugin[count];

            for (int i = 0; i < count; i++)
                result[i] = (INetworkLogViewerPlugin)Activator.CreateInstance(s_plugins[i]);

            return result;
        }

        public static void Initialize()
        {
            if (s_initialized)
                return;

            s_initialized = true;

            Console.WriteLine("Debug: Loading plugins...");

            var types = new List<Type>();

            var baseType = typeof(INetworkLogViewerPlugin);
            foreach (var type in TypeManager.Types)
            {
                if (type != baseType && baseType.IsAssignableFrom(type))
                {
                    var ctor = type.GetConstructor(Type.EmptyTypes);
                    if (ctor != null)
                        types.Add(type);
                    else
                        Console.WriteLine("Error: Parameterless constructor not found for plugin {0}",
                            type.FullName);
                }
            }

            s_plugins = types.ToArray();
            Console.WriteLine("Loaded {0} plugins", s_plugins.Length);
        }
    }
}
