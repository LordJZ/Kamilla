using System;
using System.ComponentModel;
using Kamilla.Network.Logging;
using Kamilla.Network.Parsing;

namespace Kamilla.Network.Viewing
{
    /// <summary>
    /// Represents a union of objects used to store, display and interpret data
    /// of a <see cref="Kamilla.Network.Packet"/> in a
    /// <see cref="Kamilla.Network.Viewing.NetworkLogViewerBase"/>.
    /// </summary>
    public class ViewerItem : INotifyPropertyChanged
    {
        NetworkLogViewerBase m_viewer;
        NetworkLog m_log;
        Packet m_packet;
        PacketParser m_parser;
        object m_visualData;
        int m_index;
        bool m_visualDataQueried;

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
        /// Gets or sets an instance of <see cref="Kamilla.Network.Parsing.PacketParser"/> class that is
        /// assigned to interpret data of the current <see cref="Kamilla.Network.Viewing.ViewerItem"/>.
        /// 
        /// This value can be null.
        /// </summary>
        public PacketParser Parser
        {
            get { return m_parser; }
            set
            {
                var old = m_parser;
                if (old == value)
                    return;

                m_parser = value;

                m_viewer.OnItemParserChanged(this, old, value);
            }
        }

        /// <summary>
        /// Gets the <see cref="Kamilla.Network.Logging.NetworkLog"/> that
        /// stores the underlying <see cref="Kamilla.Network.Packet"/>.
        /// </summary>
        public NetworkLog Log { get { return m_log; } }

        /// <summary>
        /// Gets or sets the visual data associated with the current instance
        /// of <see cref="Kamilla.Network.Viewing.ViewerItem"/>.
        /// 
        /// This value can be null.
        /// </summary>
        public object VisualData
        {
            get
            {
                if (!m_visualDataQueried)
                {
                    m_visualDataQueried = true;
                    m_viewer.OnItemVisualDataQueried(this);
                    m_visualDataQueried = false;
                }

                return m_visualData;
            }
            set
            {
                var old = m_visualData;
                if (old == value)
                    return;

                m_visualData = value;

                m_viewer.OnItemVisualDataChanged(this, old, value);
            }
        }

        /// <summary>
        /// Occurs when a property is changed.
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        static PropertyChangedEventArgs s_args;

        /// <summary>
        /// Raises the <see cref="Kamilla.Network.Viewing.ViewerItem.PropertyChanged"/> event
        /// for the <see cref="Kamilla.Network.Viewing.ViewerItem.VisualData"/> property.
        /// </summary>
        public void NotifyDataChanged()
        {
            if (this.PropertyChanged != null)
                this.PropertyChanged(this, s_args ?? (s_args = new PropertyChangedEventArgs("VisualData")));
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
