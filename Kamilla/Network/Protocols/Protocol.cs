using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Kamilla.Network.Parsing;
using Kamilla.Network.Viewing;
using System.Windows.Controls;

namespace Kamilla.Network.Protocols
{
    /// <summary>
    /// Represents a network protocol.
    /// </summary>
    public abstract class Protocol
    {
        internal ProtocolWrapper m_wrapper;

        /// <summary>
        /// Gets the <see cref="System.Type"/> of the <see cref="Kamilla.Network.Protocols.Protocol"/>.
        /// </summary>
        public static readonly Type Type = typeof(Protocol);

        /// <summary>
        /// Gets the <see cref="Kamilla.Network.Protocols.ProtocolWrapper"/> of
        /// the current <see cref="Kamilla.Network.Protocols.Protocol"/>.
        /// </summary>
        public ProtocolWrapper Wrapper { get { return m_wrapper; } }

        /// <summary>
        /// Gets the localized name of the current <see cref="Kamilla.Network.Protocols.Protocol"/>.
        /// </summary>
        public abstract string Name { get; }

        public abstract ViewBase View { get; }

        protected virtual PacketParser InternalCreateParser(ViewerItem item)
        {
            var packet = item.Packet as IPacketWithOpcode;
            if (packet == null)
                return null;

            Type parserType;
            if (m_wrapper.m_parsers.TryGetValue(packet.Opcode, out parserType))
                return (PacketParser)Activator.CreateInstance(parserType);

            return null;
        }

        public void CreateParser(ViewerItem item)
        {
            if (item == null)
                throw new ArgumentNullException("item");

            var parser = this.InternalCreateParser(item);
            if (parser == null)
                parser = new UndefinedPacketParser();

            parser.m_item = item;
            item.Parser = parser;
        }

        /// <summary>
        /// When implemented in a derived class,
        /// loads the current instance of <see cref="Kamilla.Network.Protocols.Protocol"/>
        /// and attachs to the provided <see cref="Kamilla.Network.Viewing.NetworkLogViewerBase"/>.
        /// 
        /// This method MUST be called from the UI thread if one exists.
        /// </summary>
        /// <param name="viewer">
        /// The instance of <see cref="Kamilla.Network.Viewing.NetworkLogViewerBase"/> to attach to.
        /// </param>
        /// <returns>
        /// The <see cref="System.Windows.Controls.ViewBase"/> that defines the appearance of packet list.
        /// </returns>
        /// <exception cref="System.InvalidOperationException">
        /// The current instance of <see cref="Kamilla.Network.Protocols.Protocol"/> is already
        /// attached to a <see cref="Kamilla.Network.Viewing.NetworkLogViewerBase"/>.
        /// </exception>
        public abstract void Load(NetworkLogViewerBase viewer);

        /// <summary>
        /// When implemented in a derived class,
        /// unloads and releases all resources used by
        /// the current instance of <see cref="Kamilla.Network.Protocols.Protocol"/>.
        /// 
        /// This method MUST be called from the UI thread if one exists.
        /// </summary>
        /// <exception cref="System.InvalidOperationException">
        /// The current instance of <see cref="Kamilla.Network.Protocols.Protocol"/> is not
        /// attached to a <see cref="Kamilla.Network.Viewing.NetworkLogViewerBase"/>.
        /// </exception>
        public abstract void Unload();
    }
}
