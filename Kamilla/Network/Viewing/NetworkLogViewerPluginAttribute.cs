using System;
using Kamilla.Network.Protocols;

namespace Kamilla.Network.Viewing
{
    /// <summary>
    /// Indicates <see cref="Kamilla.Network.Protocols.Protocol"/>
    /// that the current <see cref="Kamilla.Network.Viewing.NetworkLogViewerBasePlugin"/> can handle.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = true)]
    public sealed class NetworkLogViewerPluginAttribute : Attribute
    {
        Type m_protocol;

        /// <summary>
        /// Initializes a new instance of <see cref="Kamilla.Network.Viewing.NetworkLogViewerPluginAttribute"/>
        /// for the specified <see cref="System.Type"/> of <see cref="Kamilla.Network.Protocols.Protocol"/>.
        /// </summary>
        /// <param name="protocol">
        /// <see cref="System.Type"/> of <see cref="Kamilla.Network.Protocols.Protocol"/>
        /// that the underlying <see cref="Kamilla.Network.Viewing.NetworkLogViewerBasePlugin"/> can handle.
        /// </param>
        /// <exception cref="System.ArgumentNullException">
        /// protocol is null.
        /// </exception>
        /// <exception cref="System.ArgumentException">
        /// protocol is not subclass of <see cref="Kamilla.Network.Protocols.Protocol"/>.
        /// </exception>
        public NetworkLogViewerPluginAttribute(Type protocol)
        {
            if (protocol == null)
                throw new ArgumentNullException();

            if (!protocol.IsSubclassOf(Protocol.Type))
                throw new ArgumentException("protocol");

            m_protocol = protocol;
        }

        /// <summary>
        /// Gets the type of <see cref="Kamilla.Network.Protocols.Protocol"/>
        /// that the current <see cref="Kamilla.Network.Viewing.NetworkLogViewerBasePlugin"/> can handle.
        /// </summary>
        public Type ProtocolType { get { return m_protocol; } }
    }
}
