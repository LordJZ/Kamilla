using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Kamilla.Network
{
    /// <summary>
    /// Represents a single network packet.
    /// </summary>
    public class Packet
    {
        byte[] m_data;
        uint m_arrivalTicks;
        DateTime m_arrivalDateTime;
        PacketFlags m_flags;
        TransferDirection m_direction;

        /// <summary>
        /// Gets the <see cref="Kamilla.Network.TransferDirection"/>
        /// of the current <see cref="Kamilla.Network.Packet"/>.
        /// </summary>
        public TransferDirection Direction { get { return m_direction; } }

        /// <summary>
        /// Gets the <see cref="Kamilla.Network.PacketFlags"/>
        /// of the current <see cref="Kamilla.Network.Packet"/>.
        /// </summary>
        public PacketFlags Flags { get { return m_flags; } }

        /// <summary>
        /// Gets the <see cref="System.DateTime"/> representing the moment when
        /// the <see cref="Kamilla.Network.Packet"/> arrived.
        /// </summary>
        public DateTime ArrivalTime { get { return m_arrivalDateTime; } }

        /// <summary>
        /// Gets the number of milliseconds passed from the
        /// Operation System start to the <see cref="Kamilla.Network.Packet"/> arrival event.
        /// </summary>
        public uint ArrivalTicks { get { return m_arrivalTicks; } }

        /// <summary>
        /// Initializes a new instance of <see cref="Kamilla.Network.Packet"/> class
        /// with the specified data and <see cref="Kamilla.Network.TransferDirection"/>.
        /// </summary>
        /// <param name="data">
        /// Data of the new <see cref="Kamilla.Network.Packet"/>.
        /// </param>
        /// <param name="direction">
        /// <see cref="Kamilla.Network.TransferDirection"/> of the new <see cref="Kamilla.Network.Packet"/>.
        /// </param>
        public Packet(byte[] data, TransferDirection direction)
            : this(data, direction, PacketFlags.None)
        {
        }

        /// <summary>
        /// Initializes a new instance of <see cref="Kamilla.Network.Packet"/> class
        /// with the specified data, <see cref="Kamilla.Network.TransferDirection"/>,
        /// and <see cref="Kamilla.Network.PacketFlags"/>.
        /// </summary>
        /// <param name="data">
        /// Data of the new <see cref="Kamilla.Network.Packet"/>.
        /// </param>
        /// <param name="direction">
        /// <see cref="Kamilla.Network.TransferDirection"/> of the new <see cref="Kamilla.Network.Packet"/>.
        /// </param>
        /// <param name="flags">
        /// <see cref="Kamilla.Network.PacketFlags"/> of the new <see cref="Kamilla.Network.Packet"/>.
        /// </param>
        public Packet(byte[] data, TransferDirection direction, PacketFlags flags)
            : this(data, direction, flags, DateTime.Now, (uint)Environment.TickCount)
        {
        }

        /// <summary>
        /// Initializes a new instance of <see cref="Kamilla.Network.Packet"/> class
        /// with the specified data, <see cref="Kamilla.Network.TransferDirection"/>,
        /// <see cref="Kamilla.Network.PacketFlags"/>, and arrival time and ticks.
        /// </summary>
        /// <param name="data">
        /// Data of the new <see cref="Kamilla.Network.Packet"/>.
        /// </param>
        /// <param name="direction">
        /// <see cref="Kamilla.Network.TransferDirection"/> of the new <see cref="Kamilla.Network.Packet"/>.
        /// </param>
        /// <param name="flags">
        /// <see cref="Kamilla.Network.PacketFlags"/> of the new <see cref="Kamilla.Network.Packet"/>.
        /// </param>
        /// <param name="arrivalTicks">
        /// Number of milliseconds passed from the Operation System start to the packet arrival event.
        /// </param>
        /// <param name="arrivalTime">
        /// An instance of <see cref="System.DateTime"/> representing the moment when
        /// the packet <see cref="Kamilla.Network.Packet"/> arrived.
        /// </param>
        public Packet(byte[] data, TransferDirection direction, PacketFlags flags,
            DateTime arrivalTime, uint arrivalTicks)
        {
            m_arrivalDateTime = arrivalTime;
            m_arrivalTicks = arrivalTicks;
            m_data = data;
            m_direction = direction;
            m_flags = flags;
        }
    }
}
