using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Kamilla.Network.Parsing;
using Kamilla.Network.Viewing;

namespace Kamilla.Network.Protocols
{
    /// <summary>
    /// Represents a network protocol.
    /// </summary>
    public abstract class Protocol
    {
        /// <summary>
        /// Gets the <see cref="System.Type"/> of the <see cref="Kamilla.Network.Protocols.Protocol"/>.
        /// </summary>
        public static readonly Type Type = typeof(Protocol);

        /// <summary>
        /// Gets the localized name of the current <see cref="Kamilla.Network.Protocols.Protocol"/>.
        /// </summary>
        public abstract string Name { get; }

        /// <summary>
        /// Gets the number of columns when packets of the current
        /// <see cref="Kamilla.Network.Protocols.Protocol"/> are displayed in a ListView.
        /// </summary>
        public abstract int ListViewColumns { get; }

        /// <summary>
        /// Gets headers of columns when packets of the current
        /// <see cref="Kamilla.Network.Protocols.Protocol"/> are displayed in a ListView.
        /// </summary>
        public abstract string[] ListViewColumnHeaders { get; }

        /// <summary>
        /// Gets widths of columns when packets of the current
        /// <see cref="Kamilla.Network.Protocols.Protocol"/> are displayed in a ListView.
        /// </summary>
        public abstract int[] ListViewColumnWidths { get; }

        protected abstract PacketParser InternalCreateParser(ViewerItem item);

        public void CreateParser(ViewerItem item)
        {
            if (item == null)
                throw new ArgumentNullException("item");

            var parser = this.InternalCreateParser(item);
            if (parser == null)
                parser = new UndefinedPacketParser();

            parser.Item = item;
            item.Parser = parser;
        }

        /// <summary>
        /// When implemented in a derived class,
        /// loads the current instance of <see cref="Kamilla.Network.Protocols.Protocol"/>
        /// and attachs to the provided <see cref="Kamilla.Network.Viewing.INetworkLogViewer"/>.
        /// </summary>
        /// <param name="viewer">
        /// The instance of <see cref="Kamilla.Network.Viewing.INetworkLogViewer"/> to attach to.
        /// </param>
        /// <exception cref="System.InvalidOperationException">
        /// The current instance of <see cref="Kamilla.Network.Protocols.Protocol"/> is already
        /// attached to a <see cref="Kamilla.Network.Viewing.INetworkLogViewer"/>.
        /// </exception>
        public abstract void Load(INetworkLogViewer viewer);

        /// <summary>
        /// When implemented in a derived class,
        /// unloads and releases all resources used by
        /// the current instance of <see cref="Kamilla.Network.Protocols.Protocol"/>.
        /// </summary>
        /// <exception cref="System.InvalidOperationException">
        /// The current instance of <see cref="Kamilla.Network.Protocols.Protocol"/> is not
        /// attached to a <see cref="Kamilla.Network.Viewing.INetworkLogViewer"/>.
        /// </exception>
        public abstract void Unload();
    }
}
