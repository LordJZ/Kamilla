using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Kamilla.Network.Viewing;

namespace Kamilla.Network.Parsing
{
    /// <summary>
    /// Provides an interface to convert instances of
    /// <see cref="Kamilla.Network.Packet"/> to
    /// human-understandable representation.
    /// </summary>
    public abstract class PacketParser
    {
        /// <summary>
        /// Gets the <see cref="Kamilla.Network.Viewing.ViewerItem"/> to which
        /// the current <see cref="Kamilla.Network.Parsing.PacketParser"/> is attached.
        /// </summary>
        public ViewerItem Item { get; internal set; }
    }
}
