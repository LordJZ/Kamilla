﻿using System;
using Kamilla.Network.Parsing;
using Kamilla.Network.Logging;

namespace Kamilla.Network.Viewing
{
    /// <summary>
    /// Represents a union of objects used to store, display and interpret data
    /// of a <see cref="Kamilla.Network.Packet"/> in a
    /// <see cref="Kamilla.Network.Viewing.NetworkLogViewerBase"/>.
    /// </summary>
    public class ViewerItem
    {
        NetworkLogViewerBase m_viewer;
        NetworkLog m_log;
        Packet m_packet;
        internal PacketParser m_parser;
        object[] m_data;
        int m_index;

        /// <summary>
        /// Gets the <see cref="Kamilla.Network.Viewing.NetworkLogViewerBase"/> to which
        /// the current instance of <see cref="Kamilla.Network.Viewing.ViewerItem"/> belongs to.
        /// </summary>
        public NetworkLogViewerBase Viewer { get { return m_viewer; } }

        /// <summary>
        /// Gets the <see cref="Kamilla.Network.Packet"/> for which the current
        /// instance of <see cref="Kamilla.Network.Viewing.ViewerItem"/> was created.
        /// </summary>
        public Packet Packet { get { return m_packet; } }

        /// <summary>
        /// Gets an instance of <see cref="Kamilla.Network.Parsing.PacketParser"/> class that is
        /// assigned to interpret data of the current <see cref="Kamilla.Network.Viewing.ViewerItem"/>.
        /// 
        /// This value can be null.
        /// </summary>
        public PacketParser Parser
        {
            get { return m_parser; }
        }

        /// <summary>
        /// Gets the <see cref="Kamilla.Network.Logging.NetworkLog"/> that
        /// stores the underlying <see cref="Kamilla.Network.Packet"/>.
        /// </summary>
        public NetworkLog Log { get { return m_log; } }

        /// <summary>
        /// Gets or sets the data associated with the current instance
        /// of <see cref="Kamilla.Network.Viewing.ViewerItem"/>.
        /// 
        /// This value can be null.
        /// </summary>
        public object[] Data
        {
            get { return m_data; }
            set { m_data = value; }
        }

        /// <summary>
        /// Gets the counter of the current instance
        /// of <see cref="Kamilla.Network.Viewing.ViewerItem"/>.
        /// </summary>
        public int Index { get { return m_index; } }

        /// <summary>
        /// Initializes a new instance of <see cref="Kamilla.Network.Viewing.ViewerItem"/> that i
        /// </summary>
        /// <param name="viewer">
        /// The <see cref="Kamilla.Network.Viewing.NetworkLogViewerBase"/> to which
        /// the current instance of <see cref="Kamilla.Network.Viewing.ViewerItem"/> belongs to.
        /// </param>
        /// <param name="log">
        /// Gets the <see cref="Kamilla.Network.Logging.NetworkLog"/> that
        /// stores the underlying <see cref="Kamilla.Network.Packet"/>.
        /// </param>
        /// <param name="index">
        /// Gets the counter of the current instance
        /// of <see cref="Kamilla.Network.Viewing.ViewerItem"/>.
        /// </param>
        /// <param name="packet">
        /// Gets the <see cref="Kamilla.Network.Packet"/> for which the current
        /// instance of <see cref="Kamilla.Network.Viewing.ViewerItem"/> was created.
        /// </param>
        /// <exception cref="System.ArgumentNullException">
        /// viewer is null.
        /// -or-
        /// log is null.
        /// -or-
        /// packet is null.
        /// </exception>
        /// <exception cref="System.ArgumentOutOfRangeException">
        /// index is negative.
        /// </exception>
        public ViewerItem(NetworkLogViewerBase viewer, NetworkLog log, Packet packet, int index)
        {
            if (viewer == null)
                throw new ArgumentNullException("viewer");

            if (log == null)
                throw new ArgumentNullException("log");

            if (packet == null)
                throw new ArgumentNullException("packet");

            if (index < 0)
                throw new ArgumentOutOfRangeException("index");

            m_viewer = viewer;
            m_log = log;
            m_packet = packet;
            m_index = index;
        }
    }
}