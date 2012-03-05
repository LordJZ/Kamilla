using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using Kamilla.Network.Protocols.Wow;

namespace Kamilla.Network.Logging.Wow
{
    [NetworkLog(
        FileExtension = ".log",
        HeaderString = "20",
        Flags = NetworkLogFlags.ReadOnly
        )]
    public sealed class MangosNetworkLog : WowNetworkLog, IHasClientVersion
    {
        public override string Name
        {
            get { return "MaNGOS-based Server World Log"; }
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

        public MangosNetworkLog(NetworkLogMode mode)
            : base(mode)
        {
            if (mode == NetworkLogMode.Writing)
                throw new NotSupportedException();
        }

        Version m_version;
        public Version ClientVersion
        {
            get { return m_version; }
            set { throw new NotSupportedException(); }
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

            int nLine = 0;
            string line;
            var direction = TransferDirection.ToClient;
            uint opcode = SpecialWowOpcodes.UnknownOpcode;
            var time = DateTime.MinValue;
            byte conn_id = 0;
            byte[] data = null;
            bool waitingDate = true;
            bool readingData = false;
            string dataString = null;
            var flags = PacketFlags.None;
            var flags2 = WowPacketFlags.None;
            int progress = 0;
            try
            {
                while ((line = m_streamReader.ReadLine()) != null)
                {
                    ++nLine;

                    if (nLine == 1)
                    {
                        if (line.StartsWith("EXPECTED CLIENT BUILD ", StringComparison.InvariantCultureIgnoreCase))
                        {
                            m_version = GetClientBuildInfo(uint.Parse(line.Substring("EXPECTED CLIENT BUILD ".Length)));
                        }

                        continue;
                    }

                    if (line.Trim() == string.Empty)
                    {
                        readingData = false;

                        if (data != null)
                        {
                            string[] datas = dataString.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                            if (datas.Length != data.Length)
                                throw new FormatException(
                                    string.Format("Error in data chunk at line {0} ({1} vs {2} len)",
                                    nLine, datas.Length, data.Length)
                                    );
                            for (int i = 0; i < datas.Length; ++i)
                                data[i] = byte.Parse(datas[i], NumberStyles.AllowHexSpecifier);

                            var pkt = new WowPacket(data, direction, flags, flags2, time, 0, opcode, conn_id);
                            this.InternalAddPacket(pkt);
                            this.OnPacketAdded(pkt);

                            if (reportProgressDelegate != null)
                            {
                                int newProgress = (int)(m_streamReader.BaseStream.Position * 100 / m_streamReader.BaseStream.Length);
                                if (newProgress != progress)
                                {
                                    progress = newProgress;
                                    reportProgressDelegate(progress);
                                }
                            }

                            dataString = null;
                            data = null;
                        }

                        waitingDate = true;
                    }
                    else if (waitingDate)
                    {
                        time = DateTime.Parse(line.Trim().Substring(0, 19), CultureInfo.InvariantCulture);
                        waitingDate = false;

                        if (line.IndexOf("SERVER") != -1)
                            direction = TransferDirection.ToClient;
                        else if (line.IndexOf("CLIENT") != -1)
                            direction = TransferDirection.ToServer;

                        flags = PacketFlags.None;
                        flags2 = WowPacketFlags.None;
                    }
                    else if (readingData)
                    {
                        dataString += line;
                    }
                    else if (line.StartsWith("CLIENT"))
                    {
                        direction = TransferDirection.ToServer;
                    }
                    else if (line.StartsWith("SERVER"))
                    {
                        direction = TransferDirection.ToClient;
                    }
                    else if (line.StartsWith("SOCKET"))
                    {
                        int socket = int.Parse(line.Substring(line.IndexOf(':') + 1).Trim());
                        if (ConnectionIds.ContainsKey(socket))
                            conn_id = ConnectionIds[socket];
                        else
                        {
                            conn_id = (byte)(ConnectionIds.Count == 0 ? 1 : ConnectionIds.Last().Value + 1);
                            ConnectionIds.Add(socket, conn_id);
                        }
                    }
                    else if (line.StartsWith("LENGTH"))
                    {
                        int len = int.Parse(line.Substring(line.IndexOf(':') + 1).Trim());
                        data = new byte[len];
                    }
                    else if (line.StartsWith("OPCODE"))
                    {
                        string substr = line.Substring(line.IndexOf('(') + 1 + 2).TrimEnd(')');
                        opcode = ushort.Parse(substr, NumberStyles.AllowHexSpecifier);
                        if (opcode == 0x4F57)
                        {
                            flags2 |= WowPacketFlags.HelloPacket;
                            opcode = SpecialWowOpcodes.HelloOpcode;
                        }
                    }
                    else if (line.StartsWith("DATA"))
                    {
                        readingData = true;
                        dataString = string.Empty;
                    }
                    else if (line.Trim().Equals("NOT SEND", StringComparison.InvariantCultureIgnoreCase))
                    {
                        flags |= PacketFlags.Freezed;
                    }
                    else
                        throw new IOException();
                }
            }
            catch (Exception ex)
            {
                throw new FormatException("Error reading data on line " + nLine + ".", ex);
            }
            finally
            {
                this.CloseStream();
            }
        }
    }
}
