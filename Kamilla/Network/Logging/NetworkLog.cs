using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Kamilla.Network.Viewing;
using Kamilla.IO;
using System.IO;
using Kamilla.Network.Protocols;

namespace Kamilla.Network.Logging
{
    /// <summary>
    /// Represents a storage of network packets.
    /// </summary>
    public abstract class NetworkLog : IDisposable
    {
        #region Static Members
        protected const string s_cannotAddToWriting = "Cannot add packets to a NetworkLog in Writing mode.";
        protected const string s_cannotAddToReading = "Cannot add packets to a NetworkLog in Reading mode.";
        protected const string s_useInternalAddPacket = "Use InternalAddPacket to add packet while reading file in Reading mode.";
        protected const string s_modeInvalid = "Mode of the current NetworkLog is invalid.";
        #endregion

        #region Data
        List<Packet> m_packets;
        protected readonly NetworkLogMode m_mode;
        /// <summary>
        /// File stream handler in Writing/Reading modes.
        /// </summary>
        protected StreamHandler m_stream;
        protected long m_streamOriginalPosition;
        /// <summary>
        /// Indicates whether the dump loaded from stream or file.
        /// </summary>
        protected bool m_isLoaded;
        #endregion

        #region .ctor
        /// <summary>
        /// Initializes a new instance of <see cref="Kamilla.Network.Logging.NetworkLog"/> class
        /// in the specified <see cref="Kamilla.Network.Logging.NetworkLogMode"/>.
        /// </summary>
        /// <param name="mode">
        /// The <see cref="Kamilla.Network.Logging.NetworkLogMode"/> in which
        /// to open the <see cref="Kamilla.Network.Logging.NetworkLog"/>.
        /// </param>
        /// <exception cref="System.ArgumentException">
        /// mode is invalid.
        /// </exception>
        public NetworkLog(NetworkLogMode mode)
        {
            switch (mode)
            {
                case NetworkLogMode.Abstract:
                case NetworkLogMode.Reading:
                    m_packets = new List<Packet>();
                    break;
                case NetworkLogMode.Writing:
                    if (this is IHasStartTicks)
                        ((IHasStartTicks)this).StartTicks = (uint)Environment.TickCount;
                    if (this is IHasStartTime)
                        ((IHasStartTime)this).StartTime = DateTime.Now;
                    break;
                default:
                    throw new ArgumentException("mode");
            }
        }
        #endregion

        #region Properties
        /// <summary>
        /// Gets the localized name of the current <see cref="Kamilla.Network.Logging.NetworkLog"/>.
        /// </summary>
        public abstract string Name { get; }

        /// <summary>
        /// Gets or sets the number of packets the internal
        /// data structure can contain without resizing.
        /// </summary>
        /// <exception cref="System.InvalidOperationException">
        /// The current <see cref="Kamilla.Network.Logging.NetworkLog"/>
        /// was opened in <see href="Kamilla.Network.Logging.NetworkLogMode.Writing"/> mode.
        /// -or-
        /// The accessor is set and the current <see cref="Kamilla.Network.Logging.NetworkLog"/>
        /// was opened in <see href="Kamilla.Network.Logging.NetworkLogMode.Reading"/> mode.
        /// </exception>
        /// <exception cref="System.ArgumentOutOfRangeException">
        /// The accessor is set and the value is less than the number of packets
        /// contained in the current <see cref="Kamilla.Network.Logging.NetworkLog"/>.
        /// </exception>
        /// <exception cref="System.ObjectDisposedException">
        /// The current instance of <see cref="Kamilla.Network.Logging.NetworkLog"/> is disposed.
        /// </exception>
        public int Capacity
        {
            get
            {
                if (m_mode == NetworkLogMode.Writing)
                    throw new InvalidOperationException();

                if (m_packets == null)
                    throw new ObjectDisposedException("NetworkLog");

                return m_packets.Capacity;
            }
            set
            {
                if (m_mode == NetworkLogMode.Reading)
                    throw new InvalidOperationException();

                this.InternalSetCapacity(value);
            }
        }

        protected void InternalSetCapacity(int capacity)
        {
            if (m_mode == NetworkLogMode.Writing)
                throw new InvalidOperationException();

            if (m_packets == null)
                throw new ObjectDisposedException("NetworkLog");

            m_packets.Capacity = capacity;
        }

        /// <summary>
        /// Gets the number of packets currently stored inside
        /// the current <see cref="Kamilla.Network.Logging.NetworkLog"/>.
        /// </summary>
        /// <exception cref="System.InvalidOperationException">
        /// The current <see cref="Kamilla.Network.Logging.NetworkLog"/>
        /// was opened in <see href="Kamilla.Network.Logging.NetworkLogMode.Writing"/> mode.
        /// </exception>
        /// <exception cref="System.ObjectDisposedException">
        /// The current instance of <see cref="Kamilla.Network.Logging.NetworkLog"/> is disposed.
        /// </exception>
        public int Count
        {
            get
            {
                if (m_mode == NetworkLogMode.Writing)
                    throw new InvalidOperationException();

                if (m_packets == null)
                    throw new ObjectDisposedException("NetworkLog");

                return m_packets.Count;
            }
        }

        /// <summary>
        /// Gets the <see cref="Kamilla.Network.Logging.NetworkLogMode"/> in which
        /// the current <see cref="Kamilla.Network.Logging.NetworkLog"/> was opened.
        /// </summary>
        public NetworkLogMode Mode { get { return m_mode; } }

        /// <summary>
        /// Gets the packets that are stored inside the current <see cref="Kamilla.Network.Logging.NetworkLog"/>.
        /// </summary>
        /// <exception cref="System.InvalidOperationException">
        /// The current <see cref="Kamilla.Network.Logging.NetworkLog"/>
        /// was opened in <see href="Kamilla.Network.Logging.NetworkLogMode.Writing"/> mode.
        /// </exception>
        /// <exception cref="System.ObjectDisposedException">
        /// The current instance of <see cref="Kamilla.Network.Logging.NetworkLog"/> is disposed.
        /// </exception>
        public IEnumerable<Packet> Packets
        {
            get
            {
                if (m_mode == NetworkLogMode.Writing)
                    throw new InvalidOperationException();

                if (m_packets == null)
                    throw new ObjectDisposedException("NetworkLog");

                return m_packets;
            }
        }

        /// <summary>
        /// Gets the <see cref="Kamilla.Network.Protocols.ProtocolWrapper"/> of a protocol
        /// that is most likely the real network protocol of the stored packets in the log.
        /// </summary>
        public virtual ProtocolWrapper SuggestedProtocol
        {
            get { return null; }
        }
        #endregion

        #region Adding Packets
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
        /// <exception cref="System.ArgumentNullException">
        /// packet is null.
        /// </exception>
        /// <exception cref="System.InvalidOperationException">
        /// The current <see cref="Kamilla.Network.Logging.NetworkLog"/>
        /// was opened in <see href="Kamilla.Network.Logging.NetworkLogMode.Reading"/> mode.
        /// </exception>
        /// <exception cref="System.IO.IOException">
        /// The mode is <see href="Kamilla.Network.Logging.NetworkLogMode.Writing"/>
        /// and an I/O exception occured while writing packet.
        /// </exception>
        /// <exception cref="System.ObjectDisposedException">
        /// The mode is <see href="Kamilla.Network.Logging.NetworkLogMode.Writing"/> and
        /// the underlying <see cref="System.IO.Stream"/> is closed.
        /// </exception>
        public void AddPacket(Packet packet)
        {
            if (packet == null)
                throw new ArgumentNullException("packet");

            switch (m_mode)
            {
                case NetworkLogMode.Abstract:
                {
                    this.InternalAddPacket(packet);
                    break;
                }
                case NetworkLogMode.Writing:
                {
                    if (m_stream == null)
                        throw new ObjectDisposedException("Stream");

                    if (!m_stream.BaseStream.CanWrite)
                        throw new EndOfStreamException();

                    this.InternalWritePacket(packet);
                    break;
                }
                case NetworkLogMode.Reading:
                    throw new InvalidOperationException(s_cannotAddToReading + " " + s_useInternalAddPacket);
                default:
                    throw new InvalidOperationException(s_modeInvalid);
            }

            this.OnPacketAdded(packet);
        }

        /// <summary>
        /// Adds a <see cref="Kamilla.Network.Packet"/> to the internal packet
        /// storage of the current <see cref="Kamilla.Network.Logging.NetworkLog"/>.
        /// </summary>
        /// <param name="packet">
        /// Instance of <see cref="Kamilla.Network.Packet"/> that should be added to the
        /// current <see cref="Kamilla.Network.Logging.NetworkLog"/>.
        /// </param>
        /// <exception cref="System.InvalidOperationException">
        /// The current <see cref="Kamilla.Network.Logging.NetworkLog"/>
        /// was opened in <see href="Kamilla.Network.Logging.NetworkLogMode.Writing"/> mode.
        /// </exception>
        /// <exception cref="System.ArgumentNullException">
        /// packet is null.
        /// </exception>
        /// <exception cref="System.ObjectDisposedException">
        /// The current instance of <see cref="Kamilla.Network.Logging.NetworkLog"/> is disposed.
        /// </exception>
        protected virtual void InternalAddPacket(Packet packet)
        {
            if (m_mode == NetworkLogMode.Writing)
                throw new InvalidOperationException(s_cannotAddToWriting);

            if (packet == null)
                throw new ArgumentNullException("packet");

            if (m_packets == null)
                throw new ObjectDisposedException("NetworkLog");

            m_packets.Add(packet);
        }

        /// <summary>
        /// Fires the <see href="Kamilla.Network.Logging.NetworkLog.PacketAdded"/> event.
        /// </summary>
        /// <param name="packet">
        /// The <see cref="Kamilla.Network.Packet"/> that was added to
        /// the current <see cref="Kamilla.Network.Logging.NetworkLog"/>.
        /// </param>
        /// <exception cref="System.ArgumentNullException">
        /// packet is null.
        /// </exception>
        protected void OnPacketAdded(Packet packet)
        {
            if (packet == null)
                throw new ArgumentNullException("packet");

            if (this.PacketAdded != null)
                this.PacketAdded(this, new PacketAddedEventArgs(packet));
        }
        #endregion

        #region Writing
        #region Step-by-step Saving
        /// <summary>
        /// Opens the specified file for writing.
        /// </summary>
        /// <param name="filename">
        /// Name of the file to open.
        /// </param>
        /// <exception cref="System.ArgumentNullException">
        /// filename is null.
        /// </exception>
        /// <exception cref="System.ArgumentException">
        /// Failed to open file with the specified name.
        /// </exception>
        /// <exception cref="System.IO.IOException">
        /// An I/O exception occured.
        /// </exception>
        /// <exception cref="System.InvalidOperationException">
        /// The current <see cref="Kamilla.Network.Logging.NetworkLog"/>
        /// was not opened in <see href="Kamilla.Network.Logging.NetworkLogMode.Writing"/> mode.
        /// -or-
        /// A stream is already opened.
        /// </exception>
        public void OpenForWriting(string filename)
        {
            if (m_mode != NetworkLogMode.Writing)
                throw new InvalidOperationException();

            if (m_stream != null)
                throw new InvalidOperationException("A stream is already opened.");

            if (filename == null)
                throw new ArgumentNullException("filename");

            Stream stream;
            try
            {
                stream = new FileStream(filename, FileMode.Create);
            }
            catch (Exception e)
            {
                throw new ArgumentException("Failed to open file.", e);
            }

            this.InternalOpenForWriting(stream);
        }

        /// <summary>
        /// Opens the specified <see cref="System.IO.Stream"/> for writing.
        /// </summary>
        /// <param name="stream">
        /// The <see cref="System.IO.Stream"/> to open for writing.
        /// </param>
        /// <exception cref="System.ArgumentNullException">
        /// stream is null.
        /// </exception>
        /// <exception cref="System.IO.IOException">
        /// An I/O exception occured.
        /// </exception>
        /// <exception cref="System.InvalidOperationException">
        /// The current <see cref="Kamilla.Network.Logging.NetworkLog"/>
        /// was not opened in <see href="Kamilla.Network.Logging.NetworkLogMode.Writing"/> mode.
        /// -or-
        /// A stream is already opened.
        /// </exception>
        public void OpenForWriting(Stream stream)
        {
            if (m_mode != NetworkLogMode.Writing)
                throw new InvalidOperationException();

            if (m_stream != null)
                throw new InvalidOperationException("A stream is already opened.");

            if (stream == null)
                throw new ArgumentNullException("stream");

            if (!stream.CanWrite)
                throw new EndOfStreamException();

            this.InternalOpenForWriting(stream);
        }

        protected virtual void InternalOpenForWriting(Stream stream)
        {
            m_stream = new StreamHandler(stream);
            m_streamOriginalPosition = stream.Position;
        }

        /// <summary>
        /// When implemented in a derived class, writes meta data of the current
        /// <see cref="Kamilla.Network.Logging.NetworkLog"/> to the underlying stream.
        /// </summary>
        /// <exception cref="System.InvalidOperationException">
        /// The current <see cref="Kamilla.Network.Logging.NetworkLog"/>
        /// was not opened in <see href="Kamilla.Network.Logging.NetworkLogMode.Writing"/> mode.
        /// </exception>
        /// <exception cref="System.IO.IOException">
        /// An I/O exception occured.
        /// </exception>
        /// <exception cref="System.ObjectDisposedException">
        /// The mode is <see href="Kamilla.Network.Logging.NetworkLogMode.Writing"/> and
        /// the underlying <see cref="System.IO.Stream"/> is closed.
        /// </exception>
        public void WriteMetaData()
        {
            if (m_mode != NetworkLogMode.Writing)
                throw new InvalidOperationException();

            if (m_stream == null)
                throw new ObjectDisposedException("Stream");

            if (!m_stream.BaseStream.CanWrite)
                throw new EndOfStreamException();

            this.InternalWriteMetaData();
        }

        /// <summary>
        /// When implemented in a derived class, writes meta data of the current
        /// <see cref="Kamilla.Network.Logging.NetworkLog"/> to the underlying stream.
        /// </summary>
        protected abstract void InternalWriteMetaData();

        /// <summary>
        /// When implemented in a derived class, writes a
        /// <see cref="Kamilla.Network.Packet"/> to the underlying stream.
        /// </summary>
        /// <param name="packet">
        /// Instance of <see cref="Kamilla.Network.Packet"/> that
        /// should be written to the underlying stream.
        /// </param>
        protected abstract void InternalWritePacket(Packet packet);
        #endregion

        #region One-pass Saving
        /// <summary>
        /// Saves the current <see cref="Kamilla.Network.Logging.NetworkLog"/> to a file.
        /// </summary>
        /// <param name="filename">
        /// Name of the file to save into.
        /// </param>
        /// <exception cref="System.ArgumentNullException">
        /// filename is null.
        /// </exception>
        /// <exception cref="System.ArgumentException">
        /// Failed to open file with the specified name.
        /// </exception>
        /// <exception cref="System.IO.IOException">
        /// An I/O exception occured.
        /// </exception>
        /// <exception cref="System.InvalidOperationException">
        /// The current <see cref="Kamilla.Network.Logging.NetworkLog"/>
        /// was not opened in <see href="Kamilla.Network.Logging.NetworkLogMode.Abstract"/> mode.
        /// </exception>
        public void Save(string filename)
        {
            if (m_mode != NetworkLogMode.Abstract)
                throw new InvalidOperationException();

            if (filename == null)
                throw new ArgumentNullException("filename");

            Stream stream;
            try
            {
                stream = new FileStream(filename, FileMode.Create);
            }
            catch (Exception e)
            {
                throw new ArgumentException("Failed to open file.", e);
            }

            this.InternalSave(stream);
        }

        /// <summary>
        /// Saves the current <see cref="Kamilla.Network.Logging.NetworkLog"/>
        /// to a <see cref="System.IO.Stream"/>.
        /// </summary>
        /// <param name="stream">
        /// The <see cref="System.IO.Stream"/> to save the
        /// current <see cref="Kamilla.Network.Logging.NetworkLog"/> to.
        /// </param>
        /// <exception cref="System.ArgumentNullException">
        /// stream is null.
        /// </exception>
        /// <exception cref="System.IO.IOException">
        /// An I/O exception occured.
        /// </exception>
        /// <exception cref="System.InvalidOperationException">
        /// The current <see cref="Kamilla.Network.Logging.NetworkLog"/>
        /// was not opened in <see href="Kamilla.Network.Logging.NetworkLogMode.Abstract"/> mode.
        /// </exception>
        public void Save(Stream stream)
        {
            if (m_mode != NetworkLogMode.Abstract)
                throw new InvalidOperationException();

            if (stream == null)
                throw new ArgumentNullException("stream");

            if (!stream.CanWrite)
                throw new EndOfStreamException();

            this.InternalSave(stream);
        }

        /// <summary>
        /// When implemented in a derived class, saves the whole
        /// <see cref="Kamilla.Network.Logging.NetworkLog"/> to a <see cref="System.IO.Stream"/>.
        /// </summary>
        /// <param name="stream">
        /// The <see cref="System.IO.Stream"/> to save the
        /// current <see cref="Kamilla.Network.Logging.NetworkLog"/> to.
        /// </param>
        protected abstract void InternalSave(Stream stream);
        #endregion
        #endregion

        #region Reading
        /// <summary>
        /// Opens a file for reading.
        /// </summary>
        /// <param name="filename">
        /// Name of the file to open for reading.
        /// </param>
        /// <exception cref="System.ArgumentNullException">
        /// filename is null.
        /// </exception>
        /// <exception cref="System.ArgumentException">
        /// Failed to open file with the specified name.
        /// </exception>
        /// <exception cref="System.IO.IOException">
        /// An I/O exception occured.
        /// </exception>
        /// <exception cref="System.InvalidOperationException">
        /// The current <see cref="Kamilla.Network.Logging.NetworkLog"/>
        /// was not opened in <see href="Kamilla.Network.Logging.NetworkLogMode.Reading"/> mode.
        /// -or-
        /// A stream is already opened.
        /// -or-
        /// The current <see cref="Kamilla.Network.Logging.NetworkLog"/> is already loaded.
        /// </exception>
        public void OpenForReading(string filename)
        {
            if (m_mode != NetworkLogMode.Reading)
                throw new InvalidOperationException();

            if (m_isLoaded)
                throw new InvalidOperationException();

            if (m_stream != null)
                throw new InvalidOperationException("A stream is already opened.");

            if (filename == null)
                throw new ArgumentNullException("filename");

            Stream stream;
            try
            {
                stream = new FileStream(filename, FileMode.Open);
            }
            catch (Exception e)
            {
                throw new ArgumentException("Failed to open file.", e);
            }

            this.InternalOpenForReading(stream, true);
        }

        /// <summary>
        /// Opens the specified <see cref="System.IO.Stream"/> for reading.
        /// </summary>
        /// <param name="stream">
        /// The <see cref="System.IO.Stream"/> to open for reading.
        /// </param>
        /// <param name="closeStream">
        /// Indicates whether the <see cref="System.IO.Stream"/> should be
        /// closed when the reading is complete.
        /// </param>
        /// <exception cref="System.ArgumentNullException">
        /// stream is null.
        /// </exception>
        /// <exception cref="System.IO.IOException">
        /// An I/O exception occured.
        /// </exception>
        /// <exception cref="System.InvalidOperationException">
        /// The current <see cref="Kamilla.Network.Logging.NetworkLog"/>
        /// was not opened in <see href="Kamilla.Network.Logging.NetworkLogMode.Reading"/> mode.
        /// -or-
        /// A stream is already opened.
        /// -or-
        /// The current <see cref="Kamilla.Network.Logging.NetworkLog"/> is already loaded.
        /// </exception>
        public void OpenForReading(Stream stream, bool closeStream)
        {
            if (m_mode != NetworkLogMode.Reading)
                throw new InvalidOperationException();

            if (m_isLoaded)
                throw new InvalidOperationException();

            if (m_stream != null)
                throw new InvalidOperationException("A stream is already opened.");

            if (stream == null)
                throw new ArgumentNullException("stream");

            if (!stream.CanRead)
                throw new EndOfStreamException();

            this.InternalOpenForReading(stream, closeStream);
        }

        protected virtual void InternalOpenForReading(Stream stream, bool closeStream)
        {
            m_isLoaded = true;

            if (stream.CanSeek)
            {
                var data = new byte[stream.Length - stream.Position];
                stream.Read(data, 0, data.Length);
                m_stream = new StreamHandler(data);
                m_streamOriginalPosition = 0;

                if (closeStream)
                    stream.Close();
            }
            else
            {
                m_stream = new StreamHandler(stream, closeStream);
                m_streamOriginalPosition = stream.Position;
            }
        }

        /// <summary>
        /// Reads the contents of the underlying stream.
        /// </summary>
        /// <exception cref="System.IO.IOException">
        /// An I/O exception occured.
        /// </exception>
        /// <exception cref="System.InvalidOperationException">
        /// The current <see cref="Kamilla.Network.Logging.NetworkLog"/>
        /// was not opened in <see href="Kamilla.Network.Logging.NetworkLogMode.Reading"/> mode.
        /// </exception>
        /// <exception cref="System.ObjectDisposedException">
        /// The underlying <see cref="System.IO.Stream"/> is closed.
        /// </exception>
        public void Read()
        {
            if (m_mode != NetworkLogMode.Reading)
                throw new InvalidOperationException();

            if (m_stream == null)
                throw new ObjectDisposedException("Stream");

            if (!m_stream.BaseStream.CanRead)
                throw new EndOfStreamException();

            this.InternalRead(null);
        }

        /// <summary>
        /// Reads the contents of the underlying stream and reports progress if can.
        /// </summary>
        /// <param name="reportProgressDelegate">
        /// Delegate to report progress with. The argument is progress in percents (0-100).
        /// </param>
        /// <exception cref="System.IO.IOException">
        /// An I/O exception occured.
        /// </exception>
        /// <exception cref="System.InvalidOperationException">
        /// The current <see cref="Kamilla.Network.Logging.NetworkLog"/>
        /// was not opened in <see href="Kamilla.Network.Logging.NetworkLogMode.Reading"/> mode.
        /// </exception>
        /// <exception cref="System.ObjectDisposedException">
        /// The underlying <see cref="System.IO.Stream"/> is closed.
        /// </exception>
        /// <exception cref="System.ArgumentNullException">
        /// reportProgressDelegate is null.
        /// </exception>
        public void Read(Action<int> reportProgressDelegate)
        {
            if (m_mode != NetworkLogMode.Reading)
                throw new InvalidOperationException();

            if (reportProgressDelegate == null)
                throw new ArgumentNullException("reportProgressDelegate");

            if (m_stream == null)
                throw new ObjectDisposedException("Stream");

            if (!m_stream.BaseStream.CanRead)
                throw new EndOfStreamException();

            this.InternalRead(reportProgressDelegate);
        }

        protected abstract void InternalRead(Action<int> reportProgressDelegate);
        #endregion

        #region Misc
        /// <summary>
        /// Closes the underlying <see cref="System.IO.Stream"/> of the current
        /// instance of <see cref="Kamilla.Network.Logging.NetworkLog"/>.
        /// </summary>
        public void CloseStream()
        {
            if (m_stream != null)
            {
                m_stream.Close();
                m_stream = null;
            }
        }

        void IDisposable.Dispose()
        {
            if (m_stream != null)
            {
                m_stream.Close();
                m_stream = null;
            }

            m_packets = null;
        }
        #endregion
    }
}
