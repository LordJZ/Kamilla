using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using Kamilla.Network.Protocols.Wow;

namespace Kamilla.Network.Logging.Wow
{
    [NetworkLog(
        FileExtension = ".bin",
        HeaderString = "0",
        Flags = NetworkLogFlags.ReadOnly
        )]
    public sealed class TrinityNetworkLog : WowNetworkLog
    {
        public override string Name
        {
            get { return "Trinity-based Server World Log"; }
        }

        protected override void InternalWritePacket2(WowPacket packet)
        {
            throw new NotSupportedException();
        }

        protected override void InternalWriteMetaData()
        {
            throw new NotSupportedException();
        }

        protected override void InternalSave(Stream stream)
        {
            throw new NotSupportedException();
        }

        public TrinityNetworkLog(NetworkLogMode mode)
            : base(mode)
        {
            if (mode == NetworkLogMode.Writing)
                throw new NotSupportedException();
        }

        StreamReader m_streamReader;
        bool m_closeStream;
        public override void CloseStream()
        {
            base.CloseStream();

            if (m_closeStream)
                m_streamReader.Close();

            m_streamReader = null;
        }

        public override bool StreamOpened
        {
            get
            {
                return base.StreamOpened || m_streamReader != null;
            }
        }

        protected override void InternalOpenForReading(Stream stream, bool closeStream)
        {
            m_isLoaded = true;

            if (stream.CanSeek)
            {
                var data = new byte[stream.Length - stream.Position];
                stream.Read(data, 0, data.Length);

                m_streamReader = new StreamReader(new MemoryStream(data));
                m_streamOriginalPosition = 0;

                if (closeStream)
                    stream.Close();
            }
            else
            {
                m_streamReader = new StreamReader(stream);
                m_streamOriginalPosition = stream.Position;
                m_closeStream = closeStream;
            }
        }

        protected override void InternalRead(Action<int> reportProgressDelegate)
        {
            var ConnectionIds = new Dictionary<int, byte>();
            var direction = TransferDirection.ToClient;
            uint opcode = SpecialWowOpcodes.UnknownOpcode;
            var time = DateTime.MinValue;
            uint ticks = 0;
            byte conn_id = 0;
            byte[] data = null;
            var flags = PacketFlags.None;
            var flags2 = WowPacketFlags.None;
            int progress = 0;

            BinaryReader br = new BinaryReader(m_streamReader.BaseStream);

            try
            {
                while (br.BaseStream.Position < br.BaseStream.Length)
                {
                    opcode = (uint)br.ReadInt32();
                    int data_size = br.ReadInt32();
                    time = br.ReadUInt32().AsUnixTime();
                    direction = (TransferDirection)br.ReadByte();
                    data = br.ReadBytes(data_size);

                    var pkt = new WowPacket(data, direction, flags, flags2, time, ticks, opcode, conn_id);
                    this.InternalAddPacket(pkt);
                    this.OnPacketAdded(pkt);

                    if (reportProgressDelegate != null)
                    {
                        int newProgress = (int)(br.BaseStream.Position * 100 / br.BaseStream.Length);
                        if (newProgress != progress)
                        {
                            progress = newProgress;
                            reportProgressDelegate(progress);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw new FormatException("Error reading data on position " + br.BaseStream.Position + " (" + br.BaseStream.Length + ").", ex);
            }
            finally
            {
                this.CloseStream();
            }
        }
    }
}
