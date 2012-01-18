using System;
using System.Text;
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

        public static string ReadPascalString32(this StreamHandler sh)
        {
            return PascalStringReader(sh, sh.ReadInt32());
        }

        public static string ReadPascalString16(this StreamHandler sh)
        {
            return PascalStringReader(sh, sh.ReadUInt16());
        }

        public static string ReadPascalString8(this StreamHandler sh)
        {
            return PascalStringReader(sh, sh.ReadByte());
        }

        private static string PascalStringReader(StreamHandler sh, int length)
        {
            if (length > 0)
            {
                byte[] bytes = sh.ReadBytes(length + 1);

                int len = length + 1;
                if (bytes[bytes.Length - 1] == 0x00)
                    --len;

                return Encoding.UTF8.GetString(bytes, 0, len);
            }
            else
                return string.Empty;
        }
    }
}
