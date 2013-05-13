using System;
using System.Collections.Generic;
using System.IO.Compression;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Runtime.InteropServices;
using Kamilla.Network.Protocols;
using Kamilla.Network;
using Kamilla.IO;
using Kamilla;
using System.IO;

namespace Kamilla.Network.Logging
{
    [NetworkLog(
        FileExtension = ".krpd",
        HeaderString = SignatureString,
        Flags = NetworkLogFlags.None
        )]
    public sealed class Krpd2NetworkLog : NetworkLog, IHasStartTime, IHasStartTicks, IHasAddressInfo, IHasSnifferDesc
    {
        public const string SignatureString = "KRPD  v2";
        public static readonly byte[] SignatureBytes = Encoding.ASCII.GetBytes(SignatureString);

        public override string Name
        {
            get { return NetworkStrings.NetworkLog_KRPDv2; }
        }

        #region Nesteds

        [Flags]
        public enum ExtraDataFlags : uint
        {
            None = 0,
            HasClientIPv4 = 1 << 0,
            HasServerIPv4 = 1 << 1,
            HasClientIPv6 = 1 << 2,
            HasServerIPv6 = 1 << 3,
            HasClientPort = 1 << 4,
            HasServerPort = 1 << 5,
            HasSnifferIdString = 1 << 6,
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        struct MainHeader
        {
            public static readonly int Size = Marshal.SizeOf(typeof(MainHeader));

            public unsafe fixed byte Signature[8];
            public uint StartedOnUnix;
            public uint StartedOnTicks;
            public int PacketCount;
            public ExtraDataFlags ExtraInfoFlags;
            public int ExtraDataLength;
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        struct ChunkHeader
        {
            public static readonly int Size = Marshal.SizeOf(typeof(ChunkHeader));

            public uint TickCount;
            private uint m_dataLength;

            public int DataLength
            {
                get { return (int)(m_dataLength & 0x7FFFFFFF); }
                set
                {
                    m_dataLength &= 0x80000000;
                    m_dataLength |= (uint)(value & 0x7FFFFFFF);
                }
            }

            public TransferDirection Direction
            {
                get
                {
                    return (m_dataLength & 0x80000000) != 0 ?
                        TransferDirection.ToClient : TransferDirection.ToServer;
                }
                set
                {
                    m_dataLength &= 0x7FFFFFFF;
                    if (value == TransferDirection.ToClient)
                        m_dataLength |= 0x80000000;
                }
            }
        }
        #endregion

        int m_nPackets;
        public DateTime StartTime { get; set; }
        public uint StartTicks { get; set; }

        IPAddress m_clientIP;
        IPAddress m_serverIP;
        int m_clientPort;
        int m_serverPort;

        string m_snifferDesc;

        public ExtraDataFlags ExtraInfoFlags { get; set; }

        void SetExtraDataFlag(ExtraDataFlags flag, bool value)
        {
            if (value)
                this.ExtraInfoFlags |= flag;
            else
                this.ExtraInfoFlags &= ~flag;
        }

        public IPAddress ClientAddress
        {
            get { return m_clientIP; }
            set
            {
                m_clientIP = value;

                this.SetExtraDataFlag(ExtraDataFlags.HasClientIPv4, false);
                this.SetExtraDataFlag(ExtraDataFlags.HasClientIPv6, false);

                if (value != null)
                {
                    this.SetExtraDataFlag(
                        value.AddressFamily == AddressFamily.InterNetworkV6
                        ? ExtraDataFlags.HasClientIPv6
                        : ExtraDataFlags.HasClientIPv4,
                        true);
                }
            }
        }

        public int ClientPort
        {
            get { return m_clientPort; }
            set
            {
                if (m_clientPort >= ushort.MaxValue)
                    throw new ArgumentOutOfRangeException("value");

                m_clientPort = value;
                this.SetExtraDataFlag(ExtraDataFlags.HasClientPort, value != 0);
            }
        }

        public IPAddress ServerAddress
        {
            get { return m_serverIP; }
            set
            {
                m_serverIP = value;

                this.SetExtraDataFlag(ExtraDataFlags.HasServerIPv4, false);
                this.SetExtraDataFlag(ExtraDataFlags.HasServerIPv6, false);

                if (value != null)
                {
                    this.SetExtraDataFlag(
                        value.AddressFamily == AddressFamily.InterNetworkV6
                        ? ExtraDataFlags.HasServerIPv6
                        : ExtraDataFlags.HasServerIPv4,
                        true);
                }
            }
        }

        public int ServerPort
        {
            get { return m_serverPort; }
            set
            {
                if (m_serverPort >= ushort.MaxValue)
                    throw new ArgumentOutOfRangeException("value");

                m_serverPort = value;
                this.SetExtraDataFlag(ExtraDataFlags.HasServerPort, value != 0);
            }
        }

        public string SnifferDesc
        {
            get { return m_snifferDesc; }
            set
            {
                m_snifferDesc = value;
                this.SetExtraDataFlag(ExtraDataFlags.HasSnifferIdString, !string.IsNullOrEmpty(value));
            }
        }

        public Krpd2NetworkLog(NetworkLogMode mode)
            : base(mode)
        {
        }

        protected override unsafe void InternalOpenForReading(Stream stream, bool closeStream)
        {
            m_isLoaded = true;

            var mainHeaderBytes = new byte[MainHeader.Size];

            stream.Read(mainHeaderBytes, 0, MainHeader.Size);

            MainHeader* mainHeader;
            fixed (byte* mainHeaderBytesPtr = mainHeaderBytes)
            {
                mainHeader = (MainHeader*)mainHeaderBytesPtr;

                m_nPackets = mainHeader->PacketCount;
                this.StartTime = mainHeader->StartedOnUnix.AsUnixTime();
                this.StartTicks = mainHeader->StartedOnTicks;

                this.InternalSetCapacity(m_nPackets);
            }

            int extraLen = mainHeader->ExtraDataLength;
            if (extraLen > 0)
            {
                byte[] extraData = new byte[extraLen];
                stream.Read(extraData, 0, extraLen);
                using (var reader = new StreamHandler(extraData))
                {
                    ExtraDataFlags flags = mainHeader->ExtraInfoFlags;
                    if (flags.HasFlag(ExtraDataFlags.HasClientIPv4))
                        this.ClientAddress = new IPAddress(reader.ReadBytes(4));
                    if (flags.HasFlag(ExtraDataFlags.HasServerIPv4))
                        this.ServerAddress = new IPAddress(reader.ReadBytes(4));
                    if (flags.HasFlag(ExtraDataFlags.HasClientIPv6))
                        this.ClientAddress = new IPAddress(reader.ReadBytes(16));
                    if (flags.HasFlag(ExtraDataFlags.HasServerIPv6))
                        this.ServerAddress = new IPAddress(reader.ReadBytes(16));
                    if (flags.HasFlag(ExtraDataFlags.HasClientPort))
                        this.ClientPort = reader.ReadInt32();
                    if (flags.HasFlag(ExtraDataFlags.HasServerPort))
                        this.ServerPort = reader.ReadInt32();
                    if (flags.HasFlag(ExtraDataFlags.HasSnifferIdString))
                    {
                        int origLen = reader.ReadInt32();
                        int cmprLen = reader.ReadInt32();
                        var arr = new byte[origLen];

                        using (var iStream = new MemoryStream(reader.ReadBytes(cmprLen)))
                        using (var ds = new DeflateStream(iStream, CompressionMode.Decompress, true))
                            ds.Read(arr, 0, arr.Length);

                        this.SnifferDesc = Encoding.UTF8.GetString(arr);
                    }
                }
            }

            m_stream = new StreamHandler(stream, closeStream);
        }

        protected override void InternalOpenForWriting(Stream stream)
        {
            base.InternalOpenForWriting(stream);
            this.InternalWriteMetaData();
        }

        protected override unsafe void InternalRead(Action<int> reportProgressDelegate)
        {
            int headerSize = ChunkHeader.Size;
            var headerBytes = new byte[headerSize];

            fixed (byte* headerBytesPtr = headerBytes)
            {
                var header = (ChunkHeader*)headerBytesPtr;
                int progress = 0;

                //var startTime = this.StartTime;
                var startTicks = this.StartTicks;

                for (int i = 0; i < m_nPackets; ++i)
                {
                    if (m_stream.Read(headerBytes, 0, headerSize) != headerSize)
                        throw new EndOfStreamException();

                    var packet = new Packet(m_stream.ReadBytes(header->DataLength), header->Direction,
                        PacketFlags.None, this.StartTime.AddMilliseconds(header->TickCount - startTicks),
                        header->TickCount);
                    this.InternalAddPacket(packet);
                    this.OnPacketAdded(packet);

                    if (reportProgressDelegate != null)
                    {
                        int newProgress = i * 100 / m_nPackets;
                        if (newProgress != progress)
                        {
                            progress = newProgress;
                            reportProgressDelegate(progress);
                        }
                    }
                }
            }
        }

        protected override unsafe void InternalWriteMetaData()
        {
            m_stream.DoAt(m_streamOriginalPosition, () =>
            {
                var bytes = new byte[MainHeader.Size];

                byte[] extraData;
                using (var writer = new StreamHandler())
                {
                    var flags = this.ExtraInfoFlags;
                    if (flags.HasFlag(ExtraDataFlags.HasClientIPv4) ||
                        flags.HasFlag(ExtraDataFlags.HasClientIPv6))
                        writer.WriteBytes(this.ClientAddress.GetAddressBytes());
                    if (flags.HasFlag(ExtraDataFlags.HasServerIPv4) ||
                        flags.HasFlag(ExtraDataFlags.HasServerIPv6))
                        writer.WriteBytes(this.ServerAddress.GetAddressBytes());
                    if (flags.HasFlag(ExtraDataFlags.HasClientPort))
                        writer.WriteInt32(this.ClientPort);
                    if (flags.HasFlag(ExtraDataFlags.HasServerPort))
                        writer.WriteInt32(this.ServerPort);

                    if (flags.HasFlag(ExtraDataFlags.HasSnifferIdString))
                    {
                        byte[] arr = Encoding.UTF8.GetBytes(this.SnifferDesc ?? string.Empty);
                        writer.WriteInt32(arr.Length);

                        using (MemoryStream oStream = new MemoryStream())
                        {
                            // BUG http://blogs.msdn.com/b/bclteam/archive/2006/05/10/592551.aspx
                            using (var ds = new DeflateStream(oStream, CompressionMode.Compress, true))
                                ds.Write(arr, 0, arr.Length);

                            arr = oStream.ToArray();
                        }

                        writer.WriteInt32(arr.Length);
                        writer.WriteBytes(arr);
                    }

                    extraData = writer.ToByteArray();
                }

                fixed (byte* bytesPtr = bytes)
                {
                    var header = (MainHeader*)bytesPtr;
                    header->PacketCount = this.Count;
                    Marshal.Copy(SignatureBytes, 0, new IntPtr(header->Signature), SignatureBytes.Length);
                    header->StartedOnTicks = this.StartTicks;
                    header->StartedOnUnix = this.StartTime.ToUnixTime();
                    header->ExtraDataLength = extraData.Length;
                    header->ExtraInfoFlags = this.ExtraInfoFlags;
                }

                m_stream.WriteBytes(bytes);
                m_stream.WriteBytes(extraData);
            });
        }

        protected override unsafe void InternalWritePacket(Packet packet)
        {
            var bytes = new byte[ChunkHeader.Size];

            fixed (byte* bytesPtr = bytes)
            {
                var header = (ChunkHeader*)bytesPtr;
                header->TickCount = packet.ArrivalTicks;
                header->Direction = packet.Direction;
                header->DataLength = packet.Data.Length;
            }

            m_stream.WriteBytes(bytes);
            m_stream.WriteBytes(packet.Data);
        }

        protected override void InternalSave(Stream stream)
        {
            if (this.StreamOpened)
                throw new InvalidOperationException();

            m_stream = new StreamHandler(stream);

            this.InternalWriteMetaData();
            foreach (var packet in this.Packets)
                this.InternalWritePacket(packet);

            m_stream = null;
        }
    }
}
