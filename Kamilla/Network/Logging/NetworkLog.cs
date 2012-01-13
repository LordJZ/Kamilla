using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Kamilla.Network.Viewing;

namespace Kamilla.Network.Logging
{
    /// <summary>
    /// Represents a storage of network packets.
    /// </summary>
    public abstract class NetworkLog
    {
        readonly List<LoggedPacket> m_packets = new List<LoggedPacket>();

        /// <summary>
        /// Gets the internal <see cref="Kamilla.Network.Logging.LoggedPacket"/> storage.
        /// </summary>
        public IEnumerable<LoggedPacket> Packets
        {
            get
            {
                // TODO: Write-Only

                return m_packets;
            }
        }

        /// <summary>
        /// Occurs when a packet is added to the current <see cref="Kamilla.Network.Logging.NetworkLog"/>.
        /// </summary>
        public event PacketAddedEventHandler PacketAdded;

        /// <summary>
        /// Adds a <see cref="Kamilla.Network.Packet"/> to the
        /// current <see cref="Kamilla.Network.Logging.NetworkLog"/>.
        /// </summary>
        /// <param name="packet">
        /// Instance of <see cref="Kamilla.Network.Packet"/> that should be added to the
        /// current <see cref="Kamilla.Network.Logging.NetworkLog"/>.
        /// </param>
        public void AddPacket(Packet packet)
        {
            // TODO: Write-Only
            {
                var lpacket = new LoggedPacket(this, packet);
                m_packets.Add(lpacket);
            }

            if (this.PacketAdded != null)
                this.PacketAdded(this, new PacketAddedEventArgs(packet));
        }
    }
}
