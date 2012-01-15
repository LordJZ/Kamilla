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
        public ViewerItem Item { get; internal set; }
    }
}
