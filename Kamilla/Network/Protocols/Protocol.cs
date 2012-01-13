using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

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
        /// Gets the name of the current <see cref="Kamilla.Network.Protocols.Protocol"/>.
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
    }
}
