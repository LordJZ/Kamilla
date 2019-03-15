﻿using System;
using System.IO;
using Kamilla.Network;
using Kamilla.Network.Parsing;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Serialization;

namespace Kamilla.PoE.Viewer
{
    public class PoELoginParser : PacketParser
    {
        public static LoginStreamOpcodes? GetOpcode(Packet pkt)
        {
            if (pkt.Data.Length < 2)
                return null;

            return (LoginStreamOpcodes)((pkt.Data[0] << 8) | pkt.Data[1]);
        }

        static readonly JsonSerializer _json = new JsonSerializer
        {
            Formatting = Formatting.Indented,
            Converters = { new StringEnumConverter(new DefaultNamingStrategy()) }
        };

        protected override void InternalParse()
        {
            if (!(GetOpcode(this.Packet) is LoginStreamOpcodes op))
                return;

            if (!LoginStream.Serializers.TryGetValue(op, out PacketSerializer ser))
                return;

            PoEPacket packet = ser.Deserialize(this.Packet.Data.AsSpan(2), out int consumed);
            this.Reader.Skip(consumed + 2);

            _json.Serialize(new StringWriter(Output), packet);
        }
    }
}
