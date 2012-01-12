using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Kamilla.Network
{
    /// <summary>
    /// Represents flags of a <see cref="Kamilla.Network.Packet"/>.
    /// </summary>
    [Flags]
    public enum PacketFlags : byte
    {
        /// <summary>
        /// No flags defined.
        /// </summary>
        None = 0,

        /// <summary>
        /// The packet wasn't emitted by the marked sender.
        /// </summary>
        Custom = 0x01,

        /// <summary>
        /// The packet hasn't reached it's destination.
        /// </summary>
        Freezed = 0x02,

        /// <summary>
        /// The packet is extracted from a TCP packet.
        /// </summary>
        Trailing = 0x04,

        /// <summary>
        /// The packet is extracted from multiple TCP packets.
        /// </summary>
        Fragmented = 0x08,

        /// <summary>
        /// Specifies all available options of the <see cref="Kamilla.Network.PacketFlags"/> enum.
        /// </summary>
        All = Custom | Freezed | Trailing | Fragmented
    }
}
