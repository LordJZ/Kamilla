using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Kamilla.Network.Protocols.Wow
{
    /// <summary>
    /// Represents a single World of Warcraft network packet.
    /// </summary>
    public sealed class WowPacket : Packet, IPacketWithOpcode
    {
        int m_connectionId;
        uint m_opcode;
        WowPacketFlags m_wowFlags;

        /// <summary>
        /// Gets the connection id of the current <see cref="Kamilla.Network.Protocols.Wow.WowPacket"/>.
        /// </summary>
        public int ConnectionId { get { return m_connectionId; } }

        /// <summary>
        /// Gets the opcode of the current <see cref="Kamilla.Network.Protocols.Wow.WowPacket"/>.
        /// </summary>
        public uint Opcode { get { return m_opcode; } }

        public WowPacketFlags WowFlags { get { return m_wowFlags; } }

        /// <summary>
        /// Initializes a new instance of <see cref="Kamilla.Network.Protocols.Wow.WowPacket"/> class.
        /// </summary>
        /// <param name="data">
        /// Data of the new <see cref="Kamilla.Network.Protocols.Wow.WowPacket"/>.
        /// </param>
        /// <param name="direction">
        /// <see cref="Kamilla.Network.TransferDirection"/> of
        /// the new <see cref="Kamilla.Network.Protocols.Wow.WowPacket"/>.
        /// </param>
        /// <param name="flags">
        /// The <see cref="Kamilla.Network.PacketFlags"/> of
        /// the new <see cref="Kamilla.Network.Protocols.Wow.WowPacket"/>.
        /// </param>
        /// <param name="wowFlags">
        /// The <see cref="Kamilla.Network.Protocols.Wow.WowPacketFlags"/> of
        /// the new <see cref="Kamilla.Network.Protocols.Wow.WowPacket"/>.
        /// </param>
        /// <param name="arrivalTicks">
        /// Number of milliseconds passed from the Operation System start to the packet arrival event.
        /// </param>
        /// <param name="arrivalTime">
        /// An instance of <see cref="System.DateTime"/> representing the moment when
        /// the packet <see cref="Kamilla.Network.Protocols.Wow.WowPacket"/> arrived.
        /// </param>
        /// <param name="opcode">
        /// The opcode of the new <see cref="Kamilla.Network.Protocols.Wow.WowPacket"/>.
        /// </param>
        /// <param name="connectionId">
        /// The connection id of the new <see cref="Kamilla.Network.Protocols.Wow.WowPacket"/>.
        /// </param>
        public WowPacket(byte[] data, TransferDirection direction, PacketFlags flags, WowPacketFlags wowFlags,
            DateTime arrivalTime, uint arrivalTicks, uint opcode, int connectionId)
            : base(data, direction, flags, arrivalTime, arrivalTicks)
        {
            m_opcode = opcode;
            m_connectionId = connectionId;
            m_wowFlags = wowFlags;
        }
    }
}
