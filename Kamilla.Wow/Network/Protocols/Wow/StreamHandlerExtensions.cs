using System;
using Kamilla.IO;

namespace Kamilla.Network.Protocols.Wow
{
    public static class StreamHandlerExtensions
    {
        public static StreamHandler ReadXorByte(this StreamHandler reader, ref byte value)
        {
            if (value != 0)
            {
                if (value != 1)
                    throw new InvalidOperationException();

                value ^= reader.ReadByte();
            }

            return reader;
        }

        public static StreamHandler WriteXorByte(this StreamHandler writer, byte value)
        {
            if (value != 0)
                writer.WriteByte((byte)(value ^ 1));

            return writer;
        }
    }
}
