using System;

namespace Kamilla.Network.Logging
{
    /// <summary>
    /// Handles an event that is fired when a
    /// <see cref="Kamilla.Network.Packet"/> is added to a storage.
    /// </summary>
    /// <param name="sender">
    /// The storage that the <see cref="Kamilla.Network.Packet"/> was added to.
    /// </param>
    /// <param name="e">
    /// An instance of <see cref="Kamilla.Network.Logging.PacketAddedEventArgs"/>
    /// class that contains the event data.
    /// </param>
    public delegate void PacketAddedEventHandler(object sender, PacketAddedEventArgs e);

    /// <summary>
    /// Contains data related to an event that is fired when a
    /// <see cref="Kamilla.Network.Packet"/> is added to a storage.
    /// </summary>
    public sealed class PacketAddedEventArgs : EventArgs
    {
        Packet m_packet;

        /// <summary>
        /// Gets a <see cref="Kamilla.Network.Packet"/> that was added to storage.
        /// </summary>
        public Packet Packet { get { return m_packet; } }

        /// <summary>
        /// Initializes a new instance of <see cref="Kamilla.Network.Logging.PacketAddedEventArgs"/>
        /// class with the specified <see cref="Kamilla.Network.Packet"/>.
        /// </summary>
        /// <param name="packet">
        /// An instance of <see cref="Kamilla.Network.Packet"/> that was added to storage.
        /// </param>
        public PacketAddedEventArgs(Packet packet)
        {
            m_packet = packet;
        }
    }
}
