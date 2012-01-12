using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Kamilla.Network.Logging
{
    /// <summary>
    /// Represents a <see cref="Kamilla.Network.Packet"/> that is
    /// assigned to a <see cref="Kamilla.Network.Logging.NetworkLog"/>.
    /// </summary>
    public class LoggedPacket : Packet
    {
        NetworkLog m_log;

        /// <summary>
        /// Gets the <see cref="Kamilla.Network.Logging.NetworkLog"/> to which
        /// the current <see cref="Kamilla.Network.Logging.LoggedPacket"/> is assigned to.
        /// </summary>
        public NetworkLog Log { get { return m_log; } }

        /// <summary>
        /// Initializes a new instance of <see cref="Kamilla.Network.Logging.LoggedPacket"/> class
        /// using data of a <see cref="Kamilla.Network.Packet"/> with the specified
        /// <see cref="Kamilla.Network.Logging.NetworkLog"/>.
        /// </summary>
        /// <param name="log">
        /// The <see cref="Kamilla.Network.Logging.NetworkLog"/> to which
        /// the current <see cref="Kamilla.Network.Logging.LoggedPacket"/> is assigned to.
        /// </param>
        /// <param name="packet">
        /// The instance of <see cref="Kamilla.Network.Packet"/> to copy data from.
        /// </param>
        public LoggedPacket(NetworkLog log, Packet packet)
            : this(log, packet.Data, packet.Direction, packet.Flags, packet.ArrivalTime, packet.ArrivalTicks)
        {
        }

        /// <summary>
        /// Initializes a new instance of <see cref="Kamilla.Network.Logging.LoggedPacket"/> class
        /// with the specified data and <see cref="Kamilla.Network.TransferDirection"/>.
        /// </summary>
        /// <param name="log">
        /// The <see cref="Kamilla.Network.Logging.NetworkLog"/> to which
        /// the current <see cref="Kamilla.Network.Logging.LoggedPacket"/> is assigned to.
        /// </param>
        /// <param name="data">
        /// Data of the new <see cref="Kamilla.Network.Logging.LoggedPacket"/>.
        /// </param>
        /// <param name="direction">
        /// <see cref="Kamilla.Network.TransferDirection"/> of
        /// the new <see cref="Kamilla.Network.Logging.LoggedPacket"/>.
        /// </param>
        public LoggedPacket(NetworkLog log, byte[] data, TransferDirection direction)
            : this(log, data, direction, PacketFlags.None)
        {
        }

        /// <summary>
        /// Initializes a new instance of <see cref="Kamilla.Network.Logging.LoggedPacket"/> class
        /// with the specified data, <see cref="Kamilla.Network.TransferDirection"/>,
        /// and <see cref="Kamilla.Network.PacketFlags"/>.
        /// </summary>
        /// <param name="log">
        /// The <see cref="Kamilla.Network.Logging.NetworkLog"/> to which
        /// the current <see cref="Kamilla.Network.Logging.LoggedPacket"/> is assigned to.
        /// </param>
        /// <param name="data">
        /// Data of the new <see cref="Kamilla.Network.Logging.LoggedPacket"/>.
        /// </param>
        /// <param name="direction">
        /// <see cref="Kamilla.Network.TransferDirection"/> of
        /// the new <see cref="Kamilla.Network.Logging.LoggedPacket"/>.
        /// </param>
        /// <param name="flags">
        /// <see cref="Kamilla.Network.PacketFlags"/> of
        /// the new <see cref="Kamilla.Network.Logging.LoggedPacket"/>.
        /// </param>
        public LoggedPacket(NetworkLog log, byte[] data, TransferDirection direction, PacketFlags flags)
            : this(log, data, direction, flags, DateTime.Now, (uint)Environment.TickCount)
        {
        }

        /// <summary>
        /// Initializes a new instance of <see cref="Kamilla.Network.Logging.LoggedPacket"/> class
        /// with the specified data, <see cref="Kamilla.Network.TransferDirection"/>,
        /// <see cref="Kamilla.Network.PacketFlags"/>, and arrival time and ticks.
        /// </summary>
        /// <param name="log">
        /// The <see cref="Kamilla.Network.Logging.NetworkLog"/> to which
        /// the current <see cref="Kamilla.Network.Logging.LoggedPacket"/> is assigned to.
        /// </param>
        /// <param name="data">
        /// Data of the new <see cref="Kamilla.Network.Logging.LoggedPacket"/>.
        /// </param>
        /// <param name="direction">
        /// <see cref="Kamilla.Network.TransferDirection"/> of
        /// the new <see cref="Kamilla.Network.Logging.LoggedPacket"/>.
        /// </param>
        /// <param name="flags">
        /// <see cref="Kamilla.Network.PacketFlags"/> of
        /// the new <see cref="Kamilla.Network.Logging.LoggedPacket"/>.
        /// </param>
        /// <param name="arrivalTicks">
        /// Number of milliseconds passed from the Operation System start to the packet arrival event.
        /// </param>
        /// <param name="arrivalTime">
        /// An instance of <see cref="System.DateTime"/> representing the moment when
        /// the packet <see cref="Kamilla.Network.Logging.LoggedPacket"/> arrived.
        /// </param>
        public LoggedPacket(NetworkLog log, byte[] data, TransferDirection direction, PacketFlags flags,
            DateTime arrivalTime, uint arrivalTicks)
            : base(data, direction, flags, arrivalTime, arrivalTicks)
        {
            m_log = log;
        }
    }
}
