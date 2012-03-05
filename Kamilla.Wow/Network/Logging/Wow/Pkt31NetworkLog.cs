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
        HeaderBytes = new byte[] { (byte)'P', (byte)'K', (byte)'T', 0x01, 0x03 }
        )]
    public class Pkt31NetworkLog : WowNetworkLog,
        IHasClientVersion, IHasCultureInfo, IHasSessionKey,
        IHasStartTicks, IHasStartTime, IHasPktSnifferId
    {
        public override string Name { get { return "PKT 3.1"; } }
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
            public uint StartedOnUnix;
            public uint StartedOnTicks;
            public int OptionalHeaderLength;
        }

        [Flags]
        enum OptHeaderFlags : uint
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
            public int ConnectionId;
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
        byte[] m_FCAD;
        byte[] m_SCAD;
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
        public byte[] FCAD
        {
            get { return m_FCAD; }
            set
            {
                if (value != null && value.Length != 16)
                    throw new ArgumentException();

                if (m_mode == NetworkLogMode.Writing &&
                    m_stream != null && !((m_FCAD != null) ^ (value != null)))
                    throw new InvalidOperationException(
                        "Cannot modify FCAD while a file is opened."
                        );

                m_FCAD = value;
            }
        }
        public byte[] SCAD
        {
            get { return m_SCAD; }
            set
            {
                if (value != null && value.Length != 16)
                    throw new ArgumentException();

                if (m_mode == NetworkLogMode.Writing &&
                    m_stream != null && !((m_SCAD != null) ^ (value != null)))
                    throw new InvalidOperationException(
                        "Cannot modify SCAD while a file is opened."
                        );

                m_SCAD = value;
            }
        }

        #region .ctor
        public Pkt31NetworkLog(NetworkLogMode mode)
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
                this.StartTicks = header->StartedOnTicks;
                this.StartTime = header->StartedOnUnix.AsUnixTime();
            }

            if (this.SnifferId == PktSnifferId.Kamilla)
            {
                using (var reader = new StreamHandler(m_stream.ReadBytes(optLen)))
                {
                    var flags = (OptHeaderFlags)reader.ReadUInt32();

                    if ((flags & OptHeaderFlags.HasOCAD) != 0)
                        reader.Skip(4);

                    if ((flags & OptHeaderFlags.HasSnifferDescString) != 0)
                        this.SnifferDesc = reader.ReadCString();

                    if ((flags & OptHeaderFlags.HasFCAD) != 0)
                        this.FCAD = reader.ReadBytes(16);

                    if ((flags & OptHeaderFlags.HasSCAD) != 0)
                        this.SCAD = reader.ReadBytes(16);
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

            fixed (byte* ptr = headerBytes)
            {
                int progress = 0;
                var header = (ChunkHeader*)ptr;

                while (!m_stream.IsRead)
                {
                    if (m_stream.Read(headerBytes, 0, headerSize) != headerSize)
                        throw new EndOfStreamException();

                    var flags = PacketFlags.None;

                    if (m_snifferId == PktSnifferId.Kamilla && header->OptionalDataLength > 0)
                    {
                        flags = (PacketFlags)m_stream.ReadByte();
                        m_stream.Skip(header->OptionalDataLength - 1);
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
                        flags, wowFlags, this.StartTime.AddMilliseconds(header->TickCount - startTicks),
                        header->TickCount, opcode, header->ConnectionId);
                    this.InternalAddPacket(packet);
                    this.OnPacketAdded(packet);

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
                var flags = OptHeaderFlags.None;
                int optLen = 0;
                byte[] descStr = null;

                if (this.SnifferId == PktSnifferId.Kamilla)
                {
                    optLen += 4;

                    if (!string.IsNullOrEmpty(this.SnifferDesc))
                    {
                        flags |= OptHeaderFlags.HasSnifferDescString;
                        descStr = Encoding.ASCII.GetBytes(this.SnifferDesc);
                        optLen += descStr.Length + 1;
                    }

                    if (this.FCAD != null)
                    {
                        flags |= OptHeaderFlags.HasFCAD;
                        optLen += 16;
                    }

                    if (this.SCAD != null)
                    {
                        flags |= OptHeaderFlags.HasSCAD;
                        optLen += 16;
                    }
                }

                var bytes = new byte[MainHeader.Size + optLen];

                fixed (byte* bytesPtr = bytes)
                {
                    var header = (MainHeader*)bytesPtr;
                    var langBytes = this.LangBytes;

                    header->Signature[0] = (byte)'P';
                    header->Signature[1] = (byte)'K';
                    header->Signature[2] = (byte)'T';
                    header->MinorVersion = 1;
                    header->MajorVersion = 3;
                    header->SnifferId = (byte)this.SnifferId;
                    header->ClientBuild = (uint)this.ClientVersion.Revision;
                    header->Lang[0] = langBytes[0];
                    header->Lang[1] = langBytes[1];
                    header->Lang[2] = langBytes[2];
                    header->Lang[3] = langBytes[3];
                    Marshal.Copy(this.SessionKey, 0, new IntPtr(header->SessionKey), 40);
                    header->StartedOnUnix = this.StartTime.ToUnixTime();
                    header->StartedOnTicks = this.StartTicks;
                    header->OptionalHeaderLength = optLen;

                    if (this.SnifferId == PktSnifferId.Kamilla)
                    {
                        int index = MainHeader.Size;
                        *(uint*)(bytesPtr + index) = (uint)flags;
                        index += 4;

                        if ((flags & OptHeaderFlags.HasSnifferDescString) != 0)
                        {
                            Buffer.BlockCopy(descStr, 0, bytes, index, descStr.Length);
                            index += descStr.Length;
                            bytes[index] = 0;
                            ++index;
                        }

                        if ((flags & OptHeaderFlags.HasFCAD) != 0)
                        {
                            Buffer.BlockCopy(this.FCAD, 0, bytes, index, 16);
                            index += 16;
                        }

                        if ((flags & OptHeaderFlags.HasSCAD) != 0)
                        {
                            Buffer.BlockCopy(this.SCAD, 0, bytes, index, 16);
                            index += 16;
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
                    header->m_header.ConnectionId = packet.ConnectionId;
                    header->m_header.DataLength = data.Length + 4;
                    header->m_header.Direction = packet.Direction;
                    header->m_header.OptionalDataLength = 1;
                    header->m_header.TickCount = packet.ArrivalTicks;
                    header->m_flags = (byte)packet.Flags;
                    header->m_opcode = packet.Opcode;
                }
            }
            else
            {
                bytes = new byte[OutChunkHeader.Size];
                fixed (byte* bytesPtr = bytes)
                {
                    var header = (OutChunkHeader*)bytesPtr;
                    header->m_header.ConnectionId = packet.ConnectionId;
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
