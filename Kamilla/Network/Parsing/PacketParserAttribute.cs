using System;
using Kamilla.Network.Protocols;

namespace Kamilla.Network.Parsing
{
    /// <summary>
    /// Indicates <see cref="Kamilla.Network.Protocols.Protocol"/> and opcode
    /// that the current <see cref="Kamilla.Network.Parsing.PacketParser"/> can handle.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = true)]
    public class PacketParserAttribute : Attribute
    {
        Type m_protocol;
        uint m_opcode;

        /// <summary>
        /// Initializes a new instance of <see cref="Kamilla.Network.Parsing.PacketParserAttribute"/>
        /// for the specified <see cref="System.Type"/> of <see cref="Kamilla.Network.Protocols.Protocol"/>
        /// and opcode.
        /// </summary>
        /// <param name="protocol">
        /// <see cref="System.Type"/> of <see cref="Kamilla.Network.Protocols.Protocol"/>
        /// that the underlying <see cref="Kamilla.Network.Parsing.PacketParser"/> can handle.
        /// </param>
        /// <param name="opcode">
        /// Opcode that the current <see cref="Kamilla.Network.Parsing.PacketParser"/> can handle.
        /// </param>
        /// <exception cref="System.ArgumentNullException">
        /// protocol is null.
        /// </exception>
        /// <exception cref="System.ArgumentException">
        /// protocol is not subclass of <see cref="Kamilla.Network.Protocols.Protocol"/>.
        /// </exception>
        public PacketParserAttribute(Type protocol, uint opcode)
        {
            if (protocol == null)
                throw new ArgumentNullException();

            if (!protocol.IsSubclassOf(Protocol.Type))
                throw new ArgumentException("protocol");

            m_protocol = protocol;
            m_opcode = opcode;
        }

        /// <summary>
        /// Gets the type of <see cref="Kamilla.Network.Protocols.Protocol"/>
        /// that the current <see cref="Kamilla.Network.Parsing.PacketParser"/> can handle.
        /// </summary>
        public Type ProtocolType { get { return m_protocol; } }

        /// <summary>
        /// Gets the opcode that the current
        /// <see cref="Kamilla.Network.Parsing.PacketParser"/> can handle.
        /// </summary>
        public uint Opcode { get { return m_opcode; } }
    }
}
