using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Globalization;
using Kamilla.IO;
using System.Runtime.InteropServices;
using System.Runtime.CompilerServices;
using Kamilla.Network.Protocols.Wow;

namespace Kamilla.Network.Logging.Wow
{
    [NetworkLog(
        FileExtension = ".pkt",
        HeaderBytes = new byte[] { (byte)'P', (byte)'K', (byte)'T', 0x00, 0x03 }
        )]
    public class Pkt30NetworkLog : WowNetworkLog,
        IHasClientVersion, IHasCultureInfo, IHasSessionKey,
        IHasStartTicks, IHasStartTime, IHasPktSnifferId
    {
        public override string Name { get { return "PKT 3.0"; } }
        const int SessionKeyLength = 40;

        #region Nesteds
        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        unsafe struct MainHeader
        {
            public static readonly int Size = Marshal.SizeOf(typeof(MainHeader));
            public static readonly byte[] EmptyLangBytes = "xxXX".Select(x => (byte)x).ToArray();
            public static readonly byte[] SignatureChars = "PKT".Select(x => (byte)x).ToArray();

            public fixed byte Signature[3];
            public byte MinorVersion;
            public byte MajorVersion;
            public byte SnifferId;
            public uint ClientBuild;
            public fixed byte Lang[4];
            public fixed byte SessionKey[SessionKeyLength];
            public int OptionalHeaderLength;
        }

        [Flags]
        enum PktFileOptHeaderFlags : uint
        {
            None = 0,
            HasOCAD = 1 << 0,
            HasSnifferDescString = 1 << 1,
            HasFCAD = 1 << 2,
            HasSCAD = 1 << 3,
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        struct ChunkHeader
        {
            public static readonly int Size = Marshal.SizeOf(typeof(ChunkHeader));

            private const uint DirectionToClientByte = (uint)'S';
            private const uint DirectionToServerByte = (uint)'C';
            private const uint DirectionJunk = ((uint)'M' << 8) | ((uint)'S' << 16) | ((uint)'G' << 24);

            private const uint DirectionToClient = DirectionToClientByte | DirectionJunk;
            private const uint DirectionToServer = DirectionToServerByte | DirectionJunk;

            public TransferDirection Direction
            {
                get
                {
                    if (RawDirection == DirectionToClient)
                        return TransferDirection.ToClient;
                    else
                        return TransferDirection.ToServer;
                }
                set
                {
                    if (value == TransferDirection.ToClient)
                        RawDirection = DirectionToClient;
                    else
                        RawDirection = DirectionToServer;
                }
            }

            public uint RawDirection;
            public uint UnixTime;
            public uint TickCount;
            public int OptionalDataLength;
            public int DataLength;
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        struct OutChunkHeader
        {
            public static readonly int Size = Marshal.SizeOf(typeof(OutChunkHeader));

            public ChunkHeader m_header;
            public uint m_opcode;
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        struct OutChunkHeaderKamilla
        {
            public static readonly int Size = Marshal.SizeOf(typeof(OutChunkHeaderKamilla));

            public ChunkHeader m_header;
            public byte m_flags;
            public byte m_connId;
            public uint m_opcode;
        }
        #endregion

        public byte[] LangBytes
        {
            get
            {
                if (this.Culture != null)
                {
                    var bytes = Encoding.ASCII.GetBytes(this.Culture.Name.Replace("-", ""));
                    if (bytes.Length != 4)
                        throw new InvalidOperationException();
                    return bytes;
                }

                return MainHeader.EmptyLangBytes;
            }
        }

        // backing
        string m_snifferDesc;
        PktSnifferId m_snifferId;
        byte[] m_TCAD;
        byte[] m_sessionKey = new byte[40];

        public DateTime StartTime { get; set; }
        public uint StartTicks { get; set; }
        public CultureInfo Culture { get; set; }
        public byte[] SessionKey
        {
            get
            {
                return (byte[])m_sessionKey.Clone();
            }
            set
            {
                if (value == null)
                    Array.Clear(m_sessionKey, 0, 40);
                else if (value.Length != 40)
                    throw new ArgumentException("Length of Session Key must be 40.");
                else
                    Array.Copy(value, m_sessionKey, 40);
            }
        }
        public Version ClientVersion { get; set; }
        public PktSnifferId SnifferId
        {
            get { return m_snifferId; }
            set
            {
                if (m_mode == NetworkLogMode.Writing &&
                    m_stream != null && m_snifferId != value)
                    throw new InvalidOperationException(
                        "Cannot modify Sniffer Id while a file is opened."
                        );

                m_snifferId = value;
            }
        }
        public string SnifferDesc
        {
            get { return m_snifferDesc ?? string.Empty; }
            set
            {
                if (m_mode == NetworkLogMode.Writing &&
                    m_stream != null && m_snifferDesc.Length != value.Length)
                    throw new InvalidOperationException(
                        "Cannot modify Sniffer Description while a file is opened."
                        );

                m_snifferDesc = value;
            }
        }
        public byte[] TCAD
        {
            get { return m_TCAD; }
            set
            {
                if (value != null && value.Length != 32)
                    throw new ArgumentException();

                if (m_mode == NetworkLogMode.Writing &&
                    m_stream != null && !((m_TCAD != null) ^ (value != null)))
                    throw new InvalidOperationException(
                        "Cannot modify TCAD while a file is opened."
                        );

                m_TCAD = value;
            }
        }

        #region .ctor
        public Pkt30NetworkLog(NetworkLogMode mode)
            : base(mode)
        {
        }
        #endregion

        protected override unsafe void InternalOpenForReading(Stream stream, bool closeStream)
        {
            base.InternalOpenForReading(stream, closeStream);

            int optLen;

            var headerBytes = m_stream.ReadBytes(MainHeader.Size);
            fixed (byte* ptr = headerBytes)
            {
                var header = (MainHeader*)ptr;
                GetClientBuildInfo(header->ClientBuild);

                var langBytes = stackalloc byte[5];
                langBytes[0] = header->Lang[0];
                langBytes[1] = header->Lang[1];
                langBytes[2] = (byte)'-';
                langBytes[3] = header->Lang[2];
                langBytes[4] = header->Lang[3];
                var lang = new string((sbyte*)langBytes, 0, 5);
                try
                {
                    this.Culture = CultureInfo.GetCultureInfo(lang);
                }
                catch
                {
                }

                optLen = header->OptionalHeaderLength;
                Marshal.Copy(new IntPtr(header->SessionKey), m_sessionKey, 0, 40);
                this.SnifferId = (PktSnifferId)header->SnifferId;
            }

            if (this.SnifferId == PktSnifferId.Kamilla)
            {
                if (optLen > 0)
                {
                    using (var reader = new StreamHandler(m_stream.ReadBytes(optLen)))
                    {
                        this.StartTime = reader.ReadUInt32().AsUnixTime();
                        this.StartTicks = reader.ReadUInt32();

                        // ACAD, cannot maintain integrity so just skip
                        reader.Skip(16);

                        this.SnifferDesc = reader.ReadCString();

                        if (!reader.IsRead)
                            this.TCAD = reader.ReadBytes(32);
                    }
                }
            }
            else
                m_stream.Skip(optLen);

            this.InternalSetCapacity((int)((m_stream.Length - m_stream.Position) / 100));
        }

        protected override unsafe void InternalRead(Action<int> reportProgressDelegate)
        {
            int headerSize = ChunkHeader.Size;
            var headerBytes = new byte[headerSize];
            var startTicks = this.StartTicks;
            bool firstPacket = true;

            fixed (byte* ptr = headerBytes)
            {
                int progress = 0;
                var header = (ChunkHeader*)ptr;

                while (m_stream.CanRead(1))
                {
                    if (m_stream.Read(headerBytes, 0, headerSize) != headerSize)
                        throw new EndOfStreamException();

                    var flags = PacketFlags.None;
                    int connId = 0;

                    if (m_snifferId == PktSnifferId.Kamilla)
                    {
                        int nOptBytes = header->OptionalDataLength;
                        if (--nOptBytes >= 0)
                        {
                            flags = (PacketFlags)m_stream.ReadByte();

                            if (--nOptBytes >= 0)
                            {
                                connId = m_stream.ReadByte();

                                if (nOptBytes > 0)
                                    m_stream.Skip(nOptBytes);
                            }
                        }
                    }
                    else
                        m_stream.Skip(header->OptionalDataLength);

                    var opcode = m_stream.ReadUInt32();
                    var data = m_stream.ReadBytes(header->DataLength - 4);

                    var wowFlags = (WowPacketFlags)(flags & ~PacketFlags.All);
                    if ((wowFlags & WowPacketFlags.HelloPacket) != 0)
                        opcode = SpecialWowOpcodes.HelloOpcode;

                    flags &= PacketFlags.All;

                    var packet = new WowPacket(data, header->Direction,
                        flags, wowFlags, header->UnixTime.AsUnixTime(),
                        header->TickCount, opcode, connId);
                    this.InternalAddPacket(packet);
                    this.OnPacketAdded(packet);

                    if (firstPacket)
                    {
                        if (this.StartTicks == 0)
                        {
                            this.StartTime = packet.ArrivalTime;
                            this.StartTicks = packet.ArrivalTicks;
                        }
                    }
                    firstPacket = false;

                    if (reportProgressDelegate != null)
                    {
                        int newProgress = (int)(
                            (m_stream.Position - m_streamOriginalPosition) * 100
                            / (m_stream.Length - m_streamOriginalPosition));
                        if (newProgress != progress)
                        {
                            progress = newProgress;
                            reportProgressDelegate(progress);
                        }
                    }
                }
            }
        }

        protected override void InternalOpenForWriting(Stream stream)
        {
            base.InternalOpenForWriting(stream);
            this.InternalWriteMetaData();
        }

        protected override unsafe void InternalWriteMetaData()
        {
            m_stream.DoAt(m_streamOriginalPosition, () =>
            {
                int optLen = 0;
                byte[] descStr = null;

                if (this.SnifferId == PktSnifferId.Kamilla)
                {
                    optLen += 8;

                    // ACAD
                    optLen += 16;

                    if (!string.IsNullOrEmpty(this.SnifferDesc))
                    {
                        descStr = Encoding.ASCII.GetBytes(this.SnifferDesc);
                        optLen += descStr.Length + 1;
                    }

                    if (this.TCAD != null)
                        optLen += 32;
                }

                var bytes = new byte[MainHeader.Size + optLen];

                fixed (byte* bytesPtr = bytes)
                {
                    var header = (MainHeader*)bytesPtr;
                    var langBytes = this.LangBytes;

                    header->Signature[0] = (byte)'P';
                    header->Signature[1] = (byte)'K';
                    header->Signature[2] = (byte)'T';
                    header->MinorVersion = 0;
                    header->MajorVersion = 3;
                    header->SnifferId = (byte)this.SnifferId;
                    header->ClientBuild = (uint)this.ClientVersion.Revision;
                    header->Lang[0] = langBytes[0];
                    header->Lang[1] = langBytes[1];
                    header->Lang[2] = langBytes[2];
                    header->Lang[3] = langBytes[3];
                    Marshal.Copy(this.SessionKey, 0, new IntPtr(header->SessionKey), 40);
                    header->OptionalHeaderLength = optLen;

                    if (this.SnifferId == PktSnifferId.Kamilla)
                    {
                        int index = MainHeader.Size;
                        *(uint*)(bytesPtr + index) = this.StartTime.ToUnixTime();
                        index += 4;
                        *(uint*)(bytesPtr + index) = this.StartTicks;
                        index += 4;
                        // ACAD
                        index += 16;

                        if (!string.IsNullOrEmpty(this.SnifferDesc))
                        {
                            Buffer.BlockCopy(descStr, 0, bytes, index, descStr.Length);
                            index += descStr.Length;
                            bytes[index] = 0;
                            ++index;
                        }

                        if (this.TCAD != null)
                        {
                            Buffer.BlockCopy(this.TCAD, 0, bytes, index, 32);
                            index += 32;
                        }
                    }
                }

                m_stream.WriteBytes(bytes);
            });
        }

        protected override unsafe void InternalWritePacket2(WowPacket packet)
        {
            var data = packet.Data;
            byte[] bytes;

            if (this.SnifferId == PktSnifferId.Kamilla)
            {
                bytes = new byte[OutChunkHeaderKamilla.Size];
                fixed (byte* bytesPtr = bytes)
                {
                    var header = (OutChunkHeaderKamilla*)bytesPtr;
                    header->m_header.UnixTime = packet.ArrivalTime.ToUnixTime();
                    header->m_header.DataLength = data.Length + 4;
                    header->m_header.Direction = packet.Direction;
                    header->m_header.OptionalDataLength = 2;
                    header->m_header.TickCount = packet.ArrivalTicks;
                    header->m_flags = (byte)packet.Flags;
                    header->m_connId = (byte)packet.ConnectionId;
                    header->m_opcode = packet.Opcode;
                }
            }
            else
            {
                bytes = new byte[OutChunkHeader.Size];
                fixed (byte* bytesPtr = bytes)
                {
                    var header = (OutChunkHeader*)bytesPtr;
                    header->m_header.UnixTime = packet.ArrivalTime.ToUnixTime();
                    header->m_header.DataLength = data.Length + 4;
                    header->m_header.Direction = packet.Direction;
                    header->m_header.OptionalDataLength = 0;
                    header->m_header.TickCount = packet.ArrivalTicks;
                    header->m_opcode = packet.Opcode;
                }
            }

            m_stream.WriteBytes(bytes);
            m_stream.WriteBytes(data);
        }

        protected override void InternalSave(Stream stream)
        {
            if (m_stream != null)
                throw new InvalidOperationException();

            m_stream = new StreamHandler(stream);

            this.InternalWriteMetaData();
            foreach (var packet in this.Packets)
                this.InternalWritePacket(packet);

            m_stream = null;
        }
    }
}
