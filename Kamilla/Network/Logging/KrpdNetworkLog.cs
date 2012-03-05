using System;
using System.Collections.Generic;
using System.Linq;
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
    public sealed class KrpdNetworkLog : NetworkLog, IHasStartTime, IHasStartTicks
    {
        public const string SignatureString = "KRPD  v1";
        public static readonly byte[] SignatureBytes = Encoding.ASCII.GetBytes(SignatureString);

        public override string Name
        {
            get { return NetworkStrings.NetworkLog_KRPDv1; }
        }

        #region Nesteds
        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        struct MainHeader
        {
            public static readonly int Size = Marshal.SizeOf(typeof(MainHeader));

            public unsafe fixed byte Signature[8];
            public uint StartedOnUnix;
            public uint StartedOnTicks;
            public int PacketCount;
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
                get {
                    return (m_dataLength & 0x80000000) != 0 ?
                        TransferDirection.ToClient : TransferDirection.ToServer; }
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

        public KrpdNetworkLog(NetworkLogMode mode)
            : base(mode)
        {
        }

        protected override unsafe void InternalOpenForReading(Stream stream, bool closeStream)
        {
            m_isLoaded = true;

            var mainHeaderBytes = new byte[MainHeader.Size];

            stream.Read(mainHeaderBytes, 0, MainHeader.Size);

            fixed (byte* mainHeaderBytesPtr = mainHeaderBytes)
            {
                var mainHeader = (MainHeader*)mainHeaderBytesPtr;

                m_nPackets = mainHeader->PacketCount;
                this.StartTime = mainHeader->StartedOnUnix.AsUnixTime();
                this.StartTicks = mainHeader->StartedOnTicks;

                this.InternalSetCapacity(m_nPackets);
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

                var startTime = this.StartTime;
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

                fixed (byte* bytesPtr = bytes)
                {
                    var header = (MainHeader*)bytesPtr;
                    header->PacketCount = this.Count;
                    Marshal.Copy(SignatureBytes, 0, new IntPtr(header->Signature), SignatureBytes.Length);
                    header->StartedOnTicks = this.StartTicks;
                    header->StartedOnUnix = this.StartTime.ToUnixTime();
                }

                m_stream.WriteBytes(bytes);
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
