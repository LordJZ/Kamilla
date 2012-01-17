using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Kamilla.Network.Logging;
using Kamilla.Network.Parsing;
using Kamilla.Network.Viewing;

namespace Kamilla.Network.Protocols
{
    /// <summary>
    /// Provides an interface to manage loaded instance
    /// of <see cref="Kamilla.Network.Protocols.Protocol"/> class.
    /// </summary>
    public static class ProtocolManager
    {
        static bool s_initialized;
        static ProtocolWrapper[] s_wrappers;

        /// <summary>
        /// Gets the loaded instances of <see cref="Kamilla.Network.Protocols.ProtocolWrapper"/>.
        /// </summary>
        public static IEnumerable<ProtocolWrapper> ProtocolWrappers
        {
            get
            {
                if (!s_initialized)
                    Initialize();

                return s_wrappers;
            }
        }

        #region Finders
        /// <summary>
        /// Finds a <see cref="Kamilla.Network.Protocols.Protocol"/> with the specified typename.
        /// </summary>
        /// <param name="typename">
        /// Typename of the <see cref="Kamilla.Network.Protocols.ProtocolWrapper"/>.
        /// </param>
        /// <returns>
        /// <see cref="Kamilla.Network.Protocols.ProtocolWrapper"/> of the
        /// specified <see cref="System.Type"/> if found; otherwise, null.
        /// </returns>
        /// <exception cref="System.ArgumentNullException">
        /// typename is null.
        /// </exception>
        public static ProtocolWrapper FindWrapper(string typename)
        {
            if (typename == null)
                throw new ArgumentNullException();

            return FindWrapper(wrapper => wrapper.TypeName == typename);
        }

        /// <summary>
        /// Finds a <see cref="Kamilla.Network.Protocols.Protocol"/> of the specified type.
        /// </summary>
        /// <param name="type">
        /// <see cref="System.Type"/> of the <see cref="Kamilla.Network.Protocols.ProtocolWrapper"/>.
        /// </param>
        /// <returns>
        /// <see cref="Kamilla.Network.Protocols.ProtocolWrapper"/> of the
        /// specified <see cref="System.Type"/> if found; otherwise, null.
        /// </returns>
        /// <exception cref="System.ArgumentNullException">
        /// type is null.
        /// </exception>
        /// <exception cref="System.ArgumentException">
        /// type is not subtype of <see cref="Kamilla.Network.Protocols.Protocol"/>
        /// </exception>
        public static ProtocolWrapper FindWrapper(Type type)
        {
            if (type == null)
                throw new ArgumentNullException();

            if (!type.IsSubclassOf(Protocol.Type))
                throw new ArgumentException();

            return FindWrapper(wrapper => wrapper.Type == type);
        }

        /// <summary>
        /// Finds a <see cref="Kamilla.Network.Protocols.ProtocolWrapper"/> matching the specified predicate.
        /// </summary>
        /// <param name="predicate">
        /// Predicate that should be matched.
        /// </param>
        /// <returns>
        /// An instance of <see cref="Kamilla.Network.Protocols.ProtocolWrapper"/> that
        /// matches the specified predicate, if found; otherwise, null.
        /// </returns>
        /// <exception cref="System.ArgumentNullException">
        /// predicate is null.
        /// </exception>
        public static ProtocolWrapper FindWrapper(Func<ProtocolWrapper, bool> predicate)
        {
            if (predicate == null)
                throw new ArgumentNullException();

            var wrapper = s_wrappers.FirstOrDefault(predicate);
            if (wrapper == null)
                return null;

            return wrapper;
        }
        #endregion

        /// <summary>
        /// Initializes the <see cref="Kamilla.Network.Protocols.ProtocolManager"/>.
        /// </summary>
        public static void Initialize()
        {
            if (s_initialized)
                return;

            s_initialized = true;

            var protocolTypes = new List<Type>(10);
            var parserTypes = new List<Type>(1000);
            var pluginTypes = new List<Type>(30);

            var basePacketParser = typeof(PacketParser);
            var baseProtocol = typeof(Protocol);
            var baseViewerPlugin = typeof(NetworkLogViewerBasePlugin);

            foreach (var type in TypeManager.Types)
            {
                if (type.IsSubclassOf(baseProtocol))
                    protocolTypes.Add(type);
                else if (type.IsSubclassOf(basePacketParser))
                    parserTypes.Add(type);
                else if (type.IsSubclassOf(baseViewerPlugin))
                    pluginTypes.Add(type);
            }

            // Load Protocols
            var nProtocols = protocolTypes.Count;
            s_wrappers = new ProtocolWrapper[nProtocols];
            for (int i = 0; i < nProtocols; ++i)
            {
                try
                {
                    s_wrappers[i] = new ProtocolWrapper(i, protocolTypes[i]);
                }
                catch
                {
                    Console.WriteLine("Error: Failed to initialize protocol {0}", protocolTypes[i]);
                }
            }

            // Load Parsers
            var parserAttrType = typeof(PacketParserAttribute);
            foreach (var type in parserTypes)
            {
                var attrs = (PacketParserAttribute[])type.GetCustomAttributes(parserAttrType, true);

                foreach (var attr in attrs)
                {
                    if (attr.Opcode == SpecialOpcodes.UnknownOpcode)
                        continue;

                    var protocol = FindWrapper(attr.ProtocolType);
                    if (protocol == null)
                    {
                        Console.WriteLine("Error: Cannot find protocol '{0}' for parser type '{1}'",
                            protocol, type);
                        continue;
                    }

                    // TODO: add to protocol
                }
            }

            // Load Plugins
            var pluginAttrType = typeof(NetworkLogViewerPluginAttribute);
            foreach (var type in pluginTypes)
            {
                var attrs = (NetworkLogViewerPluginAttribute[])type.GetCustomAttributes(pluginAttrType, true);

                foreach (var attr in attrs)
                {
                    var protocol = FindWrapper(attr.ProtocolType);
                    if (protocol == null)
                    {
                        Console.WriteLine("Error: Cannot find protocol '{0}' for plugin type '{1}'",
                            protocol, type);
                        continue;
                    }

                    // TODO: add to protocol
                }
            }

            // TODO: shrink protocol storages
        }
    }
}
