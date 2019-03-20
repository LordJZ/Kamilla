using System;
using System.IO;
using Kamilla.Network;
using Kamilla.Network.Parsing;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Serialization;

namespace Kamilla.PoE.Viewer
{
    public class PoEGameParser : PacketParser
    {
        public static GameStreamOpcodes? GetOpcode(Packet pkt)
        {
            if (pkt.Data.Length < 2)
                return null;

            return (GameStreamOpcodes)((pkt.Data[0] << 8) | pkt.Data[1]);
        }

        protected override void InternalParse()
        {
            while (this.Reader.RemainingLength >= 2)
            {
                ushort opcode = Reader.ReadUInt16();
                opcode = (ushort)((ushort)(opcode >> 8) | (ushort)(opcode << 8));

                if (Output.Length > 0)
                {
                    Output.AppendLine();
                    Output.AppendLine();
                    Output.AppendLine($"Trailing opcode 0x{(int)opcode:X} {opcode}");
                }

                if (!GameStream.Serializers.TryGetValue((GameStreamOpcodes)opcode, out PacketSerializer ser))
                {
                    Output.AppendLine("No serializer");
                    return;
                }

                PoEPacket packet = ser.Deserialize(this.Packet.Data.AsSpan((int)Reader.Position), out int consumed);
                if (packet == null)
                {
                    Output.AppendLine("Failed to deserialize");
                    return;
                }

                JsonPresenter.Serializer.Serialize(new StringWriter(Output), packet);

                this.Reader.Skip(consumed);
            }
       }
    }
}
