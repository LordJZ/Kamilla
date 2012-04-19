using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;

namespace Kamilla.IO
{
    /// <summary>
    /// A class to manipulate data inside a stream.
    /// </summary>
    public class StreamHandler : IDisposable
    {
        private static byte[] s_newLineBytes = Environment.NewLine.Select(c => (byte)c).ToArray();

        #region Properties
        private Encoding m_encoding;
        private Encoder m_encoder;
        private Decoder m_decoder;
        private bool m_2BytesPerChar;
        private bool m_closeStream;
        /// <summary>
        /// When reading, this value indicates how many bits of m_buffer[0] is used.
        /// When writing, this value indicates how many bits are free in m_buffer[0].
        /// </summary>
        private int m_unalignedBits = 8;

        const int s_bufferSize = 16;
        private byte[] m_buffer = new byte[s_bufferSize];
        private byte[] m_charBytes;
        private char[] m_singleChar;

        /// <summary>
        /// Holds the underlying stream.
        /// </summary>
        protected Stream m_stream;

        /// <summary>
        /// Gets the underlying stream of the <see cref="Kamilla.IO.StreamHandler"/>.
        /// </summary>
        public Stream BaseStream
        {
            get
            {
                this.Flush();
                return m_stream;
            }
        }

        /// <summary>
        /// Gets the number of bytes from the current position of the stream to the end of the stream.
        /// </summary>
        /// <exception cref="System.ObjectDisposedException">
        /// Methods were called after the stream was closed.
        /// </exception>
        /// <exception cref="System.NotSupportedException">
        /// The stream does not support seeking.
        /// </exception>
        /// <exception cref="System.IO.IOException">
        /// An I/O error occurs.
        /// </exception>
        public long RemainingLength
        {
            get
            {
                this.Flush();
                return m_stream.Length - m_stream.Position;
            }
        }

        /// <summary>
        /// Gets the length of the stream in bytes.
        /// </summary>
        /// <returns>
        /// The length of the stream in bytes.</returns>
        /// <exception cref="T:System.ObjectDisposedException">
        /// The stream is closed.
        /// </exception>
        public long Length
        {
            get
            {
                this.Flush();
                return m_stream.Length;
            }
        }

        /// <summary>
        /// Gets or sets the current position within the stream.
        /// </summary>
        /// <returns>
        /// The current position within the stream.
        /// </returns>
        /// <exception cref="T:System.ArgumentOutOfRangeException">
        /// The position is set to a negative value or a value greater than <see cref="F:System.Int32.MaxValue" />.
        /// </exception>
        /// <exception cref="T:System.ObjectDisposedException">
        /// The stream is closed.
        /// </exception>
        public long Position
        {
            get { return m_stream.Position; }
            set { m_stream.Position = value; }
        }

        /// <summary>
        /// Gets the value indicating whether the current stream was fully read and no bytes can be read currently.
        /// </summary>
        /// <exception cref="System.ObjectDisposedException">
        /// Methods were called after the stream was closed.
        /// </exception>
        /// <exception cref="System.NotSupportedException">
        /// The stream does not support seeking.
        /// </exception>
        /// <exception cref="System.IO.IOException">
        /// An I/O error occurs.
        /// </exception>
        public bool IsRead
        {
            get
            {
                return this.RemainingLength == 0;
            }
        }
        #endregion

        #region Constructors
        /// <summary>
        /// Initializes a new instance of the <see cref="Kamilla.IO.StreamHandler"/> class
        /// based on the supplied stream and a specific character encoding.
        /// </summary>
        /// <param name="output">The output stream.</param>
        /// <param name="encoding">The character encoding.</param>
        /// <exception cref="System.ArgumentException">
        /// The stream does not support writing, or the stream is already closed.
        /// </exception>
        /// <exception cref="System.ArgumentNullException">
        /// output or encoding is null.
        /// </exception>
        public StreamHandler(Stream output, Encoding encoding)
            : this(output, encoding, true)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Kamilla.IO.StreamHandler"/> class
        /// based on the supplied stream and a specific character encoding.
        /// </summary>
        /// <param name="output">The output stream.</param>
        /// <param name="encoding">The character encoding.</param>
        /// <param name="closeStream">
        /// true, if the output stream should be closed when
        /// the <see cref="Kamilla.IO.StreamHandler"/> is disposed; otherwise, false.
        /// </param>
        /// <exception cref="System.ArgumentException">
        /// The stream does not support writing, or the stream is already closed.
        /// </exception>
        /// <exception cref="System.ArgumentNullException">
        /// output or encoding is null.
        /// </exception>
        public StreamHandler(Stream output, Encoding encoding, bool closeStream)
        {
            if (output == null)
                throw new ArgumentNullException("output");

            if (encoding == null)
                throw new ArgumentNullException("encoding");

            m_stream = output;
            m_closeStream = closeStream;
            m_encoding = encoding;
            m_encoder = m_encoding.GetEncoder();
            m_decoder = m_encoding.GetDecoder();
            m_2BytesPerChar = encoding is UnicodeEncoding;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Kamilla.IO.StreamHandler"/> class
        /// based on the supplied stream and using UTF-8 as the encoding for strings.
        /// </summary>
        /// <param name="output">The output stream.</param>
        /// <exception cref="System.ArgumentException">
        /// The stream does not support writing, or the stream is already closed.
        /// </exception>
        /// <exception cref="System.ArgumentNullException">
        /// output is null.
        /// </exception>
        public StreamHandler(Stream output)
            : this(output, new UTF8Encoding(false, true))
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Kamilla.IO.StreamHandler"/> class
        /// based on the supplied stream and using UTF-8 as the encoding for strings.
        /// </summary>
        /// <param name="output">The output stream.</param>
        /// <param name="closeStream">
        /// true, if the output stream should be closed when
        /// the <see cref="Kamilla.IO.StreamHandler"/> is disposed; otherwise, false.
        /// </param>
        /// <exception cref="System.ArgumentException">
        /// The stream does not support writing, or the stream is already closed.
        /// </exception>
        /// <exception cref="System.ArgumentNullException">
        /// output is null.
        /// </exception>
        public StreamHandler(Stream output, bool closeStream)
            : this(output, new UTF8Encoding(false, true), closeStream)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Kamilla.IO.StreamHandler"/> class
        /// using a new resizeable <see cref="System.IO.MemoryStream"/> and using UTF-8 as the encoding for strings.
        /// </summary>
        public StreamHandler()
            : this(new MemoryStream(), new UTF8Encoding(false, true))
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Kamilla.IO.StreamHandler"/> class
        /// using a new <see cref="System.IO.FileStream"/> in reading or writing mode
        /// and using UTF-8 as the encoding for strings.
        /// </summary>
        /// <param name="path">
        /// Name of the file to open.
        /// </param>
        public StreamHandler(string path)
            : this(new FileStream(path, FileMode.OpenOrCreate), new UTF8Encoding(false, true))
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Kamilla.IO.StreamHandler"/> class
        /// using a new <see cref="System.IO.FileStream"/> in the specified mode
        /// and using UTF-8 as the encoding for strings.
        /// </summary>
        /// <param name="path">
        /// A relative or absolute path for the file that the current
        /// <see cref="Kamilla.IO.StreamHandler"/> object will encapsulate.
        /// </param>
        /// <param name="mode">
        /// A <see cref="System.IO.FileMode"/> constant that determines how to open or create the file.
        /// </param>
        public StreamHandler(string path, FileMode mode)
            : this(new FileStream(path, mode), new UTF8Encoding(false, true))
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Kamilla.IO.StreamHandler"/> class
        /// using a new <see cref="System.IO.FileStream"/> in the specified mode
        /// and using the specified encoding for strings.
        /// </summary>
        /// <param name="path">
        /// A relative or absolute path for the file that the current
        /// <see cref="Kamilla.IO.StreamHandler"/> object will encapsulate.
        /// </param>
        /// <param name="mode">
        /// A <see cref="System.IO.FileMode"/> constant that determines how to open or create the file.
        /// </param>
        /// <param name="encoding">
        /// The character encoding.
        /// </param>
        public StreamHandler(string path, FileMode mode, Encoding encoding)
            : this(new FileStream(path, mode), encoding)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Kamilla.IO.StreamHandler"/> class
        /// based on the supplied byte array.
        /// </summary>
        /// <param name="buffer">The byte array containing data to be read or to be written into.</param>
        /// <exception cref="System.ArgumentNullException">
        /// buffer is null.
        /// </exception>
        public StreamHandler(byte[] buffer)
        {
            m_stream = new MemoryStream(buffer);
            m_closeStream = true;
            m_encoding = new UTF8Encoding(false, true);
            m_encoder = m_encoding.GetEncoder();
            m_decoder = m_encoding.GetDecoder();
            m_2BytesPerChar = false;
        }
        #endregion

        #region Misc Methods
        /// <summary>
        /// Closes the current <see cref="Kamilla.IO.StreamHandler"/> and the underlying stream.
        /// </summary>
        public virtual void Close()
        {
            this.Dispose(true);
        }

        /// <summary>
        /// Releases the unmanaged resources used by the <see cref="Kamilla.IO.StreamHandler"/>
        /// and optionally releases the managed resources.
        /// </summary>
        /// <param name="disposing">
        /// true to release both managed and unmanaged resources;
        /// false to release only unmanaged resources.
        /// </param>
        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (m_closeStream)
                    m_stream.Close();
            }
        }

        /// <summary>
        /// Releases all resources used by the current instance of the <see cref="Kamilla.IO.StreamHandler"/> class.
        /// </summary>
        public void Dispose()
        {
            this.Dispose(true);
        }

        /// <summary>
        /// Clears all buffers for the current writer and causes any buffered
        /// data to be written to the underlying device.
        /// </summary>
        public void Flush()
        {
            m_stream.Flush();
        }

        /// <summary>
        /// Gets the value indicating whether you can read the provided number of bytes.
        /// </summary>
        /// <param name="bytes">Number of bytes to read.</param>
        /// <returns>true if the provided number of bytes can be read; otherwise, false.</returns>
        /// <exception cref="System.ObjectDisposedException">
        /// Methods were called after the stream was closed.
        /// </exception>
        /// <exception cref="System.NotSupportedException">
        /// The stream does not support seeking.
        /// </exception>
        /// <exception cref="System.IO.IOException">
        /// An I/O error occurs.
        /// </exception>
        /// <exception cref="System.ArgumentException">
        /// bytes is negative.
        /// </exception>
        public bool CanRead(long bytes)
        {
            if (bytes < 0)
                throw new ArgumentException("A non-negative number is required.", "bytes");

            return RemainingLength >= bytes;
        }

        /// <summary>
        /// Skips a number of bytes from the current stream.
        /// </summary>
        /// <param name="bytes">The number of bytes to skip.</param>
        /// <returns>true if the provided number of bytes can be read; otherwise, false.</returns>
        /// <exception cref="System.ObjectDisposedException">
        /// Methods were called after the stream was closed.
        /// </exception>
        /// <exception cref="System.NotSupportedException">
        /// The stream does not support seeking.
        /// </exception>
        /// <exception cref="System.IO.IOException">
        /// An I/O error occurs.
        /// </exception>
        /// <exception cref="System.ArgumentException">
        /// bytes is negative.
        /// </exception>
        /// <exception cref="System.IO.EndOfStreamException">
        /// End of stream is reached.
        /// </exception>
        public void Skip(long bytes)
        {
            if (!CanRead(bytes))
                throw new EndOfStreamException();

            this.Flush();
            m_stream.Position += bytes;
        }
        #endregion

        #region Write Methods
        #region Unaligned Writing
        /// <summary>
        /// Flushes the unaligned bit buffer of the current stream.
        /// </summary>
        /// <returns>The current instance of <see cref="Kamilla.IO.StreamHandler"/>.</returns>
        public StreamHandler FlushUnalignedBits()
        {
            if (m_unalignedBits != 8)
                InternalFlushUnalignedBits();

            return this;
        }

        void InternalResetUnalignedBits()
        {
            m_unalignedBits = 8;
        }

        void InternalFlushUnalignedBits()
        {
            m_stream.Write(m_buffer, 0, 1);
            this.InternalResetUnalignedBits();
        }

        void InternalUnalignedWrite(ulong value, int bits)
        {
            while (bits > 0)
            {
                if (m_unalignedBits == 0)
                    InternalFlushUnalignedBits();

                int bitsWriteNow = Math.Min(bits, m_unalignedBits);
                int bitsWriteNowMask = (1 << bitsWriteNow) - 1;
                m_unalignedBits -= bitsWriteNow;
                bits -= bitsWriteNow;

                m_buffer[0] |= (byte)(((byte)(value >> bits) & bitsWriteNowMask)    // Grab the bits from the value
                    << m_unalignedBits);                                            // Store them in the buffer
            }

            if (m_unalignedBits == 0)
                InternalFlushUnalignedBits();
        }

        /// <summary>
        /// Writes a single bit of data to the current stream.
        /// </summary>
        /// <param name="value">
        /// The bit value to write.
        /// </param>
        /// <returns>The current instance of <see cref="Kamilla.IO.StreamHandler"/>.</returns>
        public StreamHandler UnalignedWriteBit(bool value)
        {
            InternalUnalignedWrite(value ? 1UL : 0UL, 1);
            return this;
        }

        /// <summary>
        /// Writes an integer value of the specified number of bits of 64 maximum to the current stream.
        /// </summary>
        /// <param name="value">
        /// The integer value to write.
        /// </param>
        /// <param name="bits">
        /// Number of bits to write.
        /// </param>
        /// <returns>The current instance of <see cref="Kamilla.IO.StreamHandler"/>.</returns>
        public StreamHandler UnalignedWriteInt(ulong value, int bits)
        {
            CheckBits(bits, 64);

            InternalUnalignedWrite((ulong)value, bits);
            return this;
        }

        /// <summary>
        /// Writes an integer value of the specified number of bits of 32 maximum to the current stream.
        /// </summary>
        /// <param name="value">
        /// The integer value to write.
        /// </param>
        /// <param name="bits">
        /// Number of bits to write.
        /// </param>
        /// <returns>The current instance of <see cref="Kamilla.IO.StreamHandler"/>.</returns>
        public StreamHandler UnalignedWriteInt(uint value, int bits)
        {
            CheckBits(bits, 32);

            InternalUnalignedWrite((ulong)value, bits);
            return this;
        }

        /// <summary>
        /// Writes an integer value of the specified number of bits of 16 maximum to the current stream.
        /// </summary>
        /// <param name="value">
        /// The integer value to write.
        /// </param>
        /// <param name="bits">
        /// Number of bits to write.
        /// </param>
        /// <returns>The current instance of <see cref="Kamilla.IO.StreamHandler"/>.</returns>
        public StreamHandler UnalignedWriteInt(ushort value, int bits)
        {
            CheckBits(bits, 16);

            InternalUnalignedWrite((ulong)value, bits);
            return this;
        }

        /// <summary>
        /// Writes an integer value of the specified number of bits of 8 maximum to the current stream.
        /// </summary>
        /// <param name="value">
        /// The integer value to write.
        /// </param>
        /// <param name="bits">
        /// Number of bits to write.
        /// </param>
        /// <returns>The current instance of <see cref="Kamilla.IO.StreamHandler"/>.</returns>
        public StreamHandler UnalignedWriteInt(byte value, int bits)
        {
            CheckBits(bits, 8);

            InternalUnalignedWrite((ulong)value, bits);
            return this;
        }
        #endregion

        /// <summary>
        /// Writes a one-byte Boolean value to the current stream, with 0 representing
        /// false and 1 representing true.
        /// </summary>
        /// <param name="value">
        /// The Boolean value to write (0 or 1).
        /// </param>
        /// <exception cref="System.IO.IOException">
        /// An I/O error occurs.
        /// </exception>
        /// <exception cref="System.ObjectDisposedException">
        /// The stream is closed.
        /// </exception>
        /// <returns>The current instance of <see cref="Kamilla.IO.StreamHandler"/>.</returns>
        public StreamHandler WriteBoolean(bool value)
        {
            this.FlushUnalignedBits();

            m_buffer[0] = value ? ((byte)1) : ((byte)0);
            m_stream.Write(m_buffer, 0, 1);

            return this;
        }

        /// <summary>
        /// Writes an unsigned byte to the current stream and advances the stream position
        /// by one byte.
        /// </summary>
        /// <param name="value">
        /// The unsigned byte to write.
        /// </param>
        /// <exception cref="System.IO.IOException">
        /// An I/O error occurs.
        /// </exception>
        /// <exception cref="System.ObjectDisposedException">
        /// The stream is closed.
        /// </exception>
        /// <returns>The current instance of <see cref="Kamilla.IO.StreamHandler"/>.</returns>
        public StreamHandler WriteByte(byte value)
        {
            this.FlushUnalignedBits();

            m_stream.WriteByte(value);

            return this;
        }

        /// <summary>
        /// Writes a byte array to the underlying stream.
        /// </summary>
        /// <param name="buffer">
        /// A byte array containing the data to write.
        /// </param>
        /// <exception cref="System.IO.IOException">
        /// An I/O error occurs.
        /// </exception>
        /// <exception cref="System.ObjectDisposedException">
        /// The stream is closed.
        /// </exception>
        /// <exception cref="System.ArgumentNullException">
        /// buffer is null.
        /// </exception>
        /// <returns>The current instance of <see cref="Kamilla.IO.StreamHandler"/>.</returns>
        public StreamHandler WriteBytes(byte[] buffer)
        {
            if (buffer == null)
                throw new ArgumentNullException("buffer");

            this.FlushUnalignedBits();

            m_stream.Write(buffer, 0, buffer.Length);

            return this;
        }

        /// <summary>
        /// Writes a Unicode character to the current stream and advances the current
        /// position of the stream in accordance with the Encoding used and the specific
        /// characters being written to the stream.
        /// </summary>
        /// <param name="ch">
        /// The non-surrogate, Unicode character to write.
        /// </param>
        /// <exception cref="System.IO.IOException">
        /// An I/O error occurs.
        /// </exception>
        /// <exception cref="System.ObjectDisposedException">
        /// The stream is closed.
        /// </exception>
        /// <exception cref="System.ArgumentException">
        /// ch is a single surrogate character.
        /// </exception>
        /// <returns>The current instance of <see cref="Kamilla.IO.StreamHandler"/>.</returns>
        public unsafe StreamHandler WriteChar(char ch)
        {
            if (char.IsSurrogate(ch))
                throw new ArgumentException("Single surrogate characters are not allowed.");

            int count = 0;

            fixed (byte* numRef = m_buffer)
            {
                count = m_encoder.GetBytes(&ch, 1, numRef, 16, true);
            }

            this.FlushUnalignedBits();

            m_stream.Write(m_buffer, 0, count);

            return this;
        }

        /// <summary>
        /// Writes a character array to the current stream and advances the current position
        /// of the stream in accordance with the Encoding used and the specific characters
        /// being written to the stream.
        /// </summary>
        /// <param name="chars">
        /// A character array containing the data to write.
        /// </param>
        /// <exception cref="System.ArgumentNullException">
        /// chars is null.
        /// </exception>
        /// <exception cref="System.ObjectDisposedException">
        /// The stream is closed.
        /// </exception>
        /// <exception cref="System.IO.IOException">
        /// An I/O error occurs.
        /// </exception>
        /// <returns>The current instance of <see cref="Kamilla.IO.StreamHandler"/>.</returns>
        public StreamHandler WriteChars(char[] chars)
        {
            if (chars == null)
                throw new ArgumentNullException("chars");

            byte[] buffer = m_encoding.GetBytes(chars, 0, chars.Length);

            this.FlushUnalignedBits();

            m_stream.Write(buffer, 0, buffer.Length);

            return this;
        }

        /// <summary>
        /// Writes a decimal value to the current stream and advances the stream position
        /// by sixteen bytes.
        /// </summary>
        /// <param name="value">
        /// The decimal value to write.
        /// </param>
        /// <exception cref="System.ObjectDisposedException">
        /// The stream is closed.
        /// </exception>
        /// <returns>The current instance of <see cref="Kamilla.IO.StreamHandler"/>.</returns>
        public StreamHandler WriteDecimal(decimal value)
        {
            int[] bits = decimal.GetBits(value);

            this.FlushUnalignedBits();

            this.WriteInt32(bits[0]);
            this.WriteInt32(bits[1]);
            this.WriteInt32(bits[2]);
            this.WriteInt32(bits[3]);

            return this;
        }

        /// <summary>
        /// Writes an eight-byte floating-point value to the current stream and advances
        /// the stream position by eight bytes.
        /// </summary>
        /// <param name="value">
        /// The eight-byte floating-point value to write.
        /// </param>
        /// <exception cref="System.IO.IOException">
        /// An I/O error occurs.
        /// </exception>
        /// <exception cref="System.ObjectDisposedException">
        /// The stream is closed.
        /// </exception>
        /// <returns>The current instance of <see cref="Kamilla.IO.StreamHandler"/>.</returns>
        public unsafe StreamHandler WriteDouble(double value)
        {
            fixed (byte* buf = m_buffer)
                *(double*)buf = value;

            this.FlushUnalignedBits();

            m_stream.Write(m_buffer, 0, 8);

            return this;
        }

        /// <summary>
        /// Writes a four-byte floating-point value to the current stream and advances
        /// the stream position by four bytes.
        /// </summary>
        /// <param name="value">
        /// The four-byte floating-point value to write.
        /// </param>
        /// <exception cref="System.IO.IOException">
        /// An I/O error occurs.
        /// </exception>
        /// <exception cref="System.ObjectDisposedException">
        /// The stream is closed.
        /// </exception>
        /// <returns>The current instance of <see cref="Kamilla.IO.StreamHandler"/>.</returns>
        public unsafe StreamHandler WriteSingle(float value)
        {
            fixed (byte* buf = m_buffer)
                *(float*)buf = value;

            this.FlushUnalignedBits();

            m_stream.Write(m_buffer, 0, 4);

            return this;
        }

        /// <summary>
        /// Writes a four-byte signed integer to the current stream and advances the
        /// stream position by four bytes.
        /// </summary>
        /// <param name="value">
        /// The four-byte signed integer to write.
        /// </param>
        /// <exception cref="System.IO.IOException">
        /// An I/O error occurs.
        /// </exception>
        /// <exception cref="System.ObjectDisposedException">
        /// The stream is closed.
        /// </exception>
        /// <returns>The current instance of <see cref="Kamilla.IO.StreamHandler"/>.</returns>
        public unsafe StreamHandler WriteInt32(int value)
        {
            fixed (byte* buf = m_buffer)
                *(int*)buf = value;

            this.FlushUnalignedBits();

            m_stream.Write(m_buffer, 0, 4);

            return this;
        }

        /// <summary>
        /// Writes an eight-byte signed integer to the current stream and advances the
        /// stream position by eight bytes.
        /// </summary>
        /// <param name="value">
        /// The eight-byte signed integer to write.
        /// </param>
        /// <exception cref="System.IO.IOException">
        /// An I/O error occurs.
        /// </exception>
        /// <exception cref="System.ObjectDisposedException">
        /// The stream is closed.
        /// </exception>
        /// <returns>The current instance of <see cref="Kamilla.IO.StreamHandler"/>.</returns>
        public unsafe StreamHandler WriteInt64(long value)
        {
            fixed (byte* buf = m_buffer)
                *(long*)buf = value;

            this.FlushUnalignedBits();

            m_stream.Write(m_buffer, 0, 8);

            return this;
        }

        /// <summary>
        /// Writes a signed byte to the current stream and advances the stream position
        /// by one byte.
        /// </summary>
        /// <param name="value">
        /// The signed byte to write.
        /// </param>
        /// <exception cref="System.IO.IOException">
        /// An I/O error occurs.
        /// </exception>
        /// <exception cref="System.ObjectDisposedException">
        /// The stream is closed.
        /// </exception>
        /// <returns>The current instance of <see cref="Kamilla.IO.StreamHandler"/>.</returns>
        public StreamHandler WriteSByte(sbyte value)
        {
            this.FlushUnalignedBits();

            m_stream.WriteByte((byte)value);

            return this;
        }

        /// <summary>
        /// Writes a two-byte signed integer to the current stream and advances the stream
        /// position by two bytes.
        /// </summary>
        /// <param name="value">
        /// The two-byte signed integer to write.
        /// </param>
        /// <exception cref="System.IO.IOException">
        /// An I/O error occurs.
        /// </exception>
        /// <exception cref="System.ObjectDisposedException">
        /// The stream is closed.
        /// </exception>
        /// <returns>The current instance of <see cref="Kamilla.IO.StreamHandler"/>.</returns>
        public unsafe StreamHandler WriteInt16(short value)
        {
            fixed (byte* buf = m_buffer)
                *(short*)buf = value;

            this.FlushUnalignedBits();

            m_stream.Write(m_buffer, 0, 2);

            return this;
        }

        /// <summary>
        /// Writes a null-terminated string to the current stream in the current encoding of
        /// the <see cref="Kamilla.IO.StreamHandler"/>, and advances the current position of the stream
        /// by the length of the string plus one.
        /// </summary>
        /// <param name="value">
        /// The string to write.
        /// </param>
        /// <exception cref="System.IO.IOException">
        /// An I/O error occurs.
        /// </exception>
        /// <exception cref="System.ArgumentNullException">
        /// value is null.
        /// </exception>
        /// <exception cref="System.ObjectDisposedException">
        /// The stream is closed.
        /// </exception>
        /// <exception cref="System.ArgumentException">
        /// value contains a null character before a not-null character.
        /// </exception>
        /// <returns>
        /// The current instance of <see cref="Kamilla.IO.StreamHandler"/>.
        /// </returns>
        public StreamHandler WriteCString(string value)
        {
            if (value == null)
                throw new ArgumentNullException("value");

            value = value.TrimEnd((char)0);
            if (value.IndexOf((char)0) != -1)
                throw new ArgumentException("value");

            byte[] buffer = m_encoding.GetBytes(value);

            this.FlushUnalignedBits();

            m_stream.Write(buffer, 0, buffer.Length);
            m_stream.WriteByte(0);

            return this;
        }

        public StreamHandler WriteString(string value)
        {
            this.FlushUnalignedBits();

            byte[] buffer = m_encoding.GetBytes(value);

            m_stream.Write(buffer, 0, buffer.Length);

            return this;
        }

        public StreamHandler WriteLine(string value)
        {
            return WriteString(value).WriteBytes(s_newLineBytes);
        }

        /// <summary>
        /// Writes a four-byte unsigned integer to the current stream and advances the
        /// stream position by four bytes.
        /// </summary>
        /// <param name="value">
        /// The four-byte unsigned integer to write.
        /// </param>
        /// <exception cref="System.IO.IOException">
        /// An I/O error occurs.
        /// </exception>
        /// <exception cref="System.ObjectDisposedException">
        /// The stream is closed.
        /// </exception>
        /// <returns>The current instance of <see cref="Kamilla.IO.StreamHandler"/>.</returns>
        public unsafe StreamHandler WriteUInt32(uint value)
        {
            fixed (byte* buf = m_buffer)
                *(uint*)buf = value;

            this.FlushUnalignedBits();

            m_stream.Write(m_buffer, 0, 4);

            return this;
        }

        /// <summary>
        /// Writes an eight-byte unsigned integer to the current stream and advances
        /// the stream position by eight bytes.
        /// </summary>
        /// <param name="value">
        /// The eight-byte unsigned integer to write.
        /// </param>
        /// <exception cref="System.IO.IOException">
        /// An I/O error occurs.
        /// </exception>
        /// <exception cref="System.ObjectDisposedException">
        /// The stream is closed.
        /// </exception>
        /// <returns>The current instance of <see cref="Kamilla.IO.StreamHandler"/>.</returns>
        public unsafe StreamHandler WriteUInt64(ulong value)
        {
            fixed (byte* buf = m_buffer)
                *(ulong*)buf = value;

            this.FlushUnalignedBits();

            m_stream.Write(m_buffer, 0, 8);

            return this;
        }

        /// <summary>
        /// Writes a two-byte unsigned integer to the current stream and advances the
        /// stream position by two bytes.
        /// </summary>
        /// <param name="value">
        /// The two-byte unsigned integer to write.
        /// </param>
        /// <exception cref="System.IO.IOException">
        /// An I/O error occurs.
        /// </exception>
        /// <exception cref="System.ObjectDisposedException">
        /// The stream is closed.
        /// </exception>
        /// <returns>The current instance of <see cref="Kamilla.IO.StreamHandler"/>.</returns>
        public unsafe StreamHandler WriteUInt16(ushort value)
        {
            fixed (byte* buf = m_buffer)
                *(ushort*)buf = value;

            this.FlushUnalignedBits();

            m_stream.Write(m_buffer, 0, 2);

            return this;
        }

        /// <summary>
        /// Writes a two-component vector to the current stream
        /// and advances the stream position by eight bytes.
        /// </summary>
        /// <param name="vector">
        /// The <see cref="Kamilla.Vector2"/> to write.
        /// </param>
        /// <returns>
        /// The current instance of <see cref="Kamilla.IO.StreamHandler"/>.
        /// </returns>
        /// <exception cref="System.IO.IOException">
        /// An I/O error occurs.
        /// </exception>
        /// <exception cref="System.ObjectDisposedException">
        /// The stream is closed.
        /// </exception>
        public unsafe StreamHandler WriteVector2(ref Vector2 vector)
        {
            fixed (byte* buf = m_buffer)
                *(Vector2*)buf = vector;

            this.FlushUnalignedBits();

            m_stream.Write(m_buffer, 0, 4 * 2);

            return this;
        }

        /// <summary>
        /// Writes a three-component vector to the current stream
        /// and advances the stream position by twelve bytes.
        /// </summary>
        /// <param name="vector">
        /// The <see cref="Kamilla.Vector3"/> to write.
        /// </param>
        /// <returns>
        /// The current instance of <see cref="Kamilla.IO.StreamHandler"/>.
        /// </returns>
        /// <exception cref="System.IO.IOException">
        /// An I/O error occurs.
        /// </exception>
        /// <exception cref="System.ObjectDisposedException">
        /// The stream is closed.
        /// </exception>
        public unsafe StreamHandler WriteVector3(ref Vector3 vector)
        {
            fixed (byte* buf = m_buffer)
                *(Vector3*)buf = vector;

            this.FlushUnalignedBits();

            m_stream.Write(m_buffer, 0, 4 * 3);

            return this;
        }

        /// <summary>
        /// Writes a region of a byte array to the current stream.
        /// </summary>
        /// <param name="buffer">
        /// A byte array containing the data to write.
        /// </param>
        /// <param name="index">
        /// The starting point in buffer at which to begin writing.
        /// </param>
        /// <param name="count">
        /// The number of bytes to write.
        /// </param>
        /// <exception cref="System.ArgumentException">
        /// The buffer length minus index is less than count.
        /// </exception>
        /// <exception cref="System.ArgumentNullException">
        /// buffer is null.
        /// </exception>
        /// <exception cref="System.ArgumentOutOfRangeException">
        /// index or count is negative.
        /// </exception>
        /// <exception cref="System.IO.IOException">
        /// An I/O error occurs.
        /// </exception>
        /// <exception cref="System.ObjectDisposedException">
        /// The stream is closed.
        /// </exception>
        /// <returns>The current instance of <see cref="Kamilla.IO.StreamHandler"/>.</returns>
        public StreamHandler WriteBytes(byte[] buffer, int index, int count)
        {
            this.FlushUnalignedBits();

            m_stream.Write(buffer, index, count);

            return this;
        }

        /// <summary>
        /// Writes a section of a character array to the current stream, and advances
        /// the current position of the stream in accordance with the Encoding used and
        /// perhaps the specific characters being written to the stream.
        /// </summary>
        /// <param name="chars">
        /// A character array containing the data to write.
        /// </param>
        /// <param name="index">
        /// The starting point in buffer from which to begin writing.
        /// </param>
        /// <param name="count">
        /// The number of characters to write.
        /// </param>
        /// <exception cref="System.ArgumentException">
        /// The buffer length minus index is less than count.
        /// </exception>
        /// <exception cref="System.ArgumentNullException">
        /// chars is null.
        /// </exception>
        /// <exception cref="System.ArgumentOutOfRangeException">
        /// index or count is negative.
        /// </exception>
        /// <exception cref="System.IO.IOException">
        /// An I/O error occurs.
        /// </exception>
        /// <exception cref="System.ObjectDisposedException">
        /// The stream is closed.
        /// </exception>
        /// <returns>The current instance of <see cref="Kamilla.IO.StreamHandler"/>.</returns>
        public StreamHandler WriteChars(char[] chars, int index, int count)
        {
            byte[] buffer = m_encoding.GetBytes(chars, index, count);

            this.FlushUnalignedBits();

            m_stream.Write(buffer, 0, buffer.Length);

            return this;
        }

        /// <summary>
        /// Writes a byte representation of a structure to the current stream.
        /// </summary>
        /// <typeparam name="T">
        /// Type of a structure.
        /// </typeparam>
        /// <param name="structure">
        /// A structure to write into the current stream.
        /// </param>
        /// <returns>The current instance of <see cref="Kamilla.IO.StreamHandler"/>.</returns>
        public StreamHandler WriteStruct<T>(T structure) where T : struct
        {
            lock (StructHelper<T>.SyncRoot)
            {
                Marshal.StructureToPtr(structure, StructHelper<T>.UnmanagedDataBank, false);
                Marshal.Copy(StructHelper<T>.UnmanagedDataBank, StructHelper<T>.ManagedDataBank, 0, StructHelper<T>.Size);

                this.FlushUnalignedBits();

                WriteBytes(StructHelper<T>.ManagedDataBank);
            }

            return this;
        }
        #endregion

        #region Read Methods
        private int InternalReadOneChar()
        {
            long position = 0L;
            int num = 0;
            int byteCount = 0;

            if (m_stream.CanSeek)
                position = m_stream.Position;

            if (m_charBytes == null)
                m_charBytes = new byte[0x80];

            if (m_singleChar == null)
                m_singleChar = new char[1];

            while (num == 0)
            {
                byteCount = m_2BytesPerChar ? 2 : 1;
                int num4 = m_stream.ReadByte();
                m_charBytes[0] = (byte)num4;
                if (num4 == -1)
                    byteCount = 0;

                if (byteCount == 2)
                {
                    num4 = m_stream.ReadByte();
                    m_charBytes[1] = (byte)num4;
                    if (num4 == -1)
                        byteCount = 1;
                }

                if (byteCount == 0)
                    return -1;

                try
                {
                    num = m_decoder.GetChars(m_charBytes, 0, byteCount, m_singleChar, 0);
                    continue;
                }
                catch
                {
                    if (m_stream.CanSeek)
                        m_stream.Seek(position - m_stream.Position, SeekOrigin.Current);

                    throw;
                }
            }

            if (num == 0)
                return -1;

            return m_singleChar[0];
        }

        private void FillBuffer(int numBytes)
        {
            m_unalignedBits = 8;

            int offset = 0;
            int num2 = 0;
            if (m_stream == null)
                throw new ObjectDisposedException("stream");

            if (numBytes == 1)
            {
                num2 = m_stream.ReadByte();
                if (num2 == -1)
                    throw new EndOfStreamException();

                m_buffer[0] = (byte)num2;
            }
            else
            {
                do
                {
                    num2 = m_stream.Read(m_buffer, offset, numBytes - offset);
                    if (num2 == 0)
                        throw new EndOfStreamException();

                    offset += num2;
                }
                while (offset < numBytes);
            }
        }

        /// <summary>
        /// Returns the next available character and does not advance the byte or character
        /// position.
        /// </summary>
        /// <returns>
        /// The next available character, or -1 if no more characters are available or
        /// the stream does not support seeking.
        /// </returns>
        /// <exception cref="System.IO.IOException">
        /// An I/O error occurs.
        /// </exception>
        public int PeekChar()
        {
            if (m_stream == null)
                throw new ObjectDisposedException("stream");

            if (!m_stream.CanSeek)
                return -1;

            long position = m_stream.Position;

            this.InternalResetUnalignedBits();

            int num2 = this.Read();

            m_stream.Position = position;

            return num2;
        }

        /// <summary>
        /// Reads characters from the underlying stream and advances the current position
        /// of the stream in accordance with the Encoding used and the specific character
        /// being read from the stream.
        /// </summary>
        /// <returns>
        /// The next character from the input stream, or -1 if no characters are currently
        /// available.
        /// </returns>
        /// <exception cref="System.IO.IOException">
        /// An I/O error occurs.
        /// </exception>
        /// <exception cref="System.ObjectDisposedException">
        /// The stream is closed.
        /// </exception>
        public int Read()
        {
            if (m_stream == null)
                throw new ObjectDisposedException("stream");

            this.InternalResetUnalignedBits();

            return this.InternalReadOneChar();
        }

        /// <summary>
        /// Reads the specified number of bytes from the stream, starting from a specified
        /// point in the byte array.
        /// </summary>
        /// <param name="buffer">
        /// The buffer to read data into.
        /// </param>
        /// <param name="offset">
        /// The starting point in the buffer at which to begin reading into the buffer.
        /// </param>
        /// <param name="count">
        /// The number of bytes to read.
        /// </param>
        /// <returns>
        /// The number of bytes read into buffer. This might be less than the number
        /// of bytes requested if that many bytes are not available, or it might be zero
        /// if the end of the stream is reached.
        /// </returns>
        /// <exception cref="System.ArgumentException">
        /// The sum of offset and count is larger than the buffer length.
        /// </exception>
        /// <exception cref="System.ArgumentNullException">
        /// buffer is null.
        /// </exception>
        /// <exception cref="System.ArgumentOutOfRangeException">
        /// offset or count is negative.
        /// </exception>
        /// <exception cref="System.IO.IOException">
        /// An I/O error occurs.
        /// </exception>
        /// <exception cref="System.NotSupportedException">
        /// The stream does not support reading.
        /// </exception>
        /// <exception cref="System.ObjectDisposedException">
        ///  Methods were called after the stream was closed.
        /// </exception>
        public int Read(byte[] buffer, int offset, int count)
        {
            this.InternalResetUnalignedBits();

            return m_stream.Read(buffer, offset, count);
        }

        public unsafe int Read(byte* ptr, int count)
        {
            if (ptr == null)
                throw new ArgumentNullException();

            if (count < 0)
                throw new ArgumentOutOfRangeException();

            this.InternalResetUnalignedBits();

            int bufferSize = s_bufferSize;
            if (bufferSize != 16)
                throw new NotImplementedException();

            fixed (byte* buffer = m_buffer)
            {
                var intptr = new IntPtr(ptr);
                for (int i = count; i > 0; i -= bufferSize)
                {
                    int shouldRead = i >= bufferSize ? bufferSize : i;
                    int read = m_stream.Read(m_buffer, 0, shouldRead);
                    if (read != shouldRead)
                        throw new EndOfStreamException();

                    Marshal.Copy(m_buffer, 0, intptr, bufferSize);

                    intptr += bufferSize;
                }
            }

            return count;
        }

        /// <summary>
        /// Reads the specified number of characters from the stream, starting from a
        /// specified point in the character array.
        /// </summary>
        /// <param name="buffer">
        /// The buffer to read data into.
        /// </param>
        /// <param name="index">
        /// The starting point in the buffer at which to begin reading into the buffer.
        /// </param>
        /// <param name="count">
        /// The number of characters to read.
        /// </param>
        /// <returns>
        /// The total number of characters read into the buffer. This might be less than
        /// the number of characters requested if that many characters are not currently
        /// available, or it might be zero if the end of the stream is reached.
        /// </returns>
        /// <exception cref="System.ArgumentException">
        /// The buffer length minus index is less than count. -or-The number of decoded
        /// characters to read is greater than count. This can happen if a Unicode decoder
        /// returns fallback characters or a surrogate pair.
        /// </exception>
        /// <exception cref="System.ArgumentNullException">
        /// buffer is null.
        /// </exception>
        /// <exception cref="System.ArgumentOutOfRangeException">
        /// index or count is negative.
        /// </exception>
        /// <exception cref="System.ObjectDisposedException">
        /// The stream is closed.
        /// </exception>
        /// <exception cref="System.IO.IOException">
        /// An I/O error occurs.
        /// </exception>
        public int Read(char[] buffer, int index, int count)
        {
            if (buffer == null)
                throw new ArgumentNullException("buffer");

            if (index < 0)
                throw new ArgumentOutOfRangeException("index", "A non-negative number is required.");

            if (count < 0)
                throw new ArgumentOutOfRangeException("count", "A non-negative number is required.");

            if ((buffer.Length - index) < count)
                throw new ArgumentException("Invalid index and count arguments.");

            if (m_stream == null)
                throw new ObjectDisposedException("stream");

            this.InternalResetUnalignedBits();

            char[] buf = this.ReadChars(count);

            Array.Copy(buf, 0, buffer, index, buf.Length);

            return buf.Length;
        }

        #region Unaligned Reading
        void CheckBits(int bits, int max)
        {
            if (bits > max || bits < 0)
                throw new ArgumentOutOfRangeException("bits", "bits must be from 0 to " + max + ".");
        }

        ulong InternalReadBits(int count)
        {
            ulong ret = 0;

            while (count > 0)
            {
                if (m_unalignedBits == 8)
                {
                    this.FillBuffer(1);
                    m_unalignedBits = 0;
                }

                int readNow = Math.Min(8 - m_unalignedBits, count);
                m_unalignedBits += readNow;
                count -= readNow;
                ret |= (ulong)(m_buffer[0] >> (8 - readNow)) << count;
                m_buffer[0] <<= readNow;
            }

            return ret;
        }

        /// <summary>
        /// Reads a single unaligned bit from the underlying stream.
        /// </summary>
        /// <returns>
        /// Value of the read bit.
        /// </returns>
        /// <exception cref="System.IO.EndOfStreamException">
        /// The end of the stream is reached.
        /// </exception>
        /// <exception cref="System.ObjectDisposedException">
        /// The stream is closed.
        /// </exception>
        /// <exception cref="System.IO.IOException">
        /// An I/O error occurs.
        /// </exception>
        public bool UnalignedReadBit()
        {
            return InternalReadBits(1) != 0;
        }

        /// <summary>
        /// Reads an unaligned unsigned integer of maximum 64 bits at max from the underlying stream.
        /// </summary>
        /// <param name="bits">
        /// Number of bits to read.
        /// </param>
        /// <returns>
        /// The read integer.
        /// </returns>
        /// <exception cref="System.IO.EndOfStreamException">
        /// The end of the stream is reached.
        /// </exception>
        /// <exception cref="System.ObjectDisposedException">
        /// The stream is closed.
        /// </exception>
        /// <exception cref="System.IO.IOException">
        /// An I/O error occurs.
        /// </exception>
        public ulong UnalignedReadBigInt(int bits)
        {
            CheckBits(bits, 64);

            return InternalReadBits(bits);
        }

        /// <summary>
        /// Reads an unaligned unsigned integer of 32 bits at max from the underlying stream.
        /// </summary>
        /// <param name="bits">
        /// Number of bits to read.
        /// </param>
        /// <returns>
        /// The read integer.
        /// </returns>
        /// <exception cref="System.IO.EndOfStreamException">
        /// The end of the stream is reached.
        /// </exception>
        /// <exception cref="System.ObjectDisposedException">
        /// The stream is closed.
        /// </exception>
        /// <exception cref="System.IO.IOException">
        /// An I/O error occurs.
        /// </exception>
        public uint UnalignedReadInt(int bits)
        {
            CheckBits(bits, 32);

            return (uint)InternalReadBits(bits);
        }

        public StreamHandler UnalignedReadInt(int bits, out uint value)
        {
            CheckBits(bits, 32);

            value = (uint)InternalReadBits(bits);

            return this;
        }

        /// <summary>
        /// Reads an unaligned unsigned integer of 16 bits at max from the underlying stream.
        /// </summary>
        /// <param name="bits">
        /// Number of bits to read.
        /// </param>
        /// <returns>
        /// The read integer.
        /// </returns>
        /// <exception cref="System.IO.EndOfStreamException">
        /// The end of the stream is reached.
        /// </exception>
        /// <exception cref="System.ObjectDisposedException">
        /// The stream is closed.
        /// </exception>
        /// <exception cref="System.IO.IOException">
        /// An I/O error occurs.
        /// </exception>
        public ushort UnalignedReadSmallInt(int bits)
        {
            CheckBits(bits, 16);

            return (ushort)InternalReadBits(bits);
        }

        /// <summary>
        /// Reads an unsigned integer of 8 bits at max from the underlying stream.
        /// </summary>
        /// <param name="bits">
        /// Number of bits to read.
        /// </param>
        /// <returns>
        /// The read integer.
        /// </returns>
        /// <exception cref="System.IO.EndOfStreamException">
        /// The end of the stream is reached.
        /// </exception>
        /// <exception cref="System.ObjectDisposedException">
        /// The stream is closed.
        /// </exception>
        /// <exception cref="System.IO.IOException">
        /// An I/O error occurs.
        /// </exception>
        public byte UnalignedReadTinyInt(int bits)
        {
            CheckBits(bits, 8);

            return (byte)InternalReadBits(bits);
        }
        #endregion

        /// <summary>
        /// Reads a Boolean value from the current stream and advances the current position
        /// of the stream by one byte.
        /// </summary>
        /// <returns>
        /// true if the byte is nonzero; otherwise, false.
        /// </returns>
        /// <exception cref="System.IO.EndOfStreamException">
        /// The end of the stream is reached.
        /// </exception>
        /// <exception cref="System.ObjectDisposedException">
        /// The stream is closed.
        /// </exception>
        /// <exception cref="System.IO.IOException">
        /// An I/O error occurs.
        /// </exception>
        public bool ReadBoolean()
        {
            this.InternalResetUnalignedBits();

            this.FillBuffer(1);
            return (m_buffer[0] != 0);
        }

        /// <summary>
        /// Reads the next byte from the current stream and advances the current position
        /// of the stream by one byte.
        /// </summary>
        /// <returns>
        /// The next byte read from the current stream.
        /// </returns>
        /// <exception cref="System.IO.EndOfStreamException">
        /// The end of the stream is reached.
        /// </exception>
        /// <exception cref="System.ObjectDisposedException">
        /// The stream is closed.
        /// </exception>
        /// <exception cref="System.IO.IOException">
        /// An I/O error occurs.
        /// </exception>
        public byte ReadByte()
        {
            if (m_stream == null)
                throw new ObjectDisposedException("m_stream");

            this.InternalResetUnalignedBits();

            int num = m_stream.ReadByte();
            if (num == -1)
                throw new EndOfStreamException();

            return (byte)num;
        }

        /// <summary>
        /// Reads the next byte from the current stream and advances the current position
        /// of the stream by one byte.
        /// </summary>
        /// <param name="value">
        /// The next byte read from the current stream.
        /// </param>
        /// <returns>
        /// The current instance of <see cref="Kamilla.IO.StreamHandler"/>.
        /// </returns>
        /// <exception cref="System.IO.EndOfStreamException">
        /// The end of the stream is reached.
        /// </exception>
        /// <exception cref="System.ObjectDisposedException">
        /// The stream is closed.
        /// </exception>
        /// <exception cref="System.IO.IOException">
        /// An I/O error occurs.
        /// </exception>
        public StreamHandler ReadByte(out byte value)
        {
            this.InternalResetUnalignedBits();

            value = this.ReadByte();
            return this;
        }

        /// <summary>
        /// Reads the specified number of bytes from the current stream into a byte array
        /// and advances the current position by that number of bytes.
        /// </summary>
        /// <param name="count">
        /// The number of bytes to read.
        /// </param>
        /// <returns>
        /// A byte array containing data read from the underlying stream. This might
        /// be less than the number of bytes requested if the end of the stream is reached.
        /// </returns>
        /// <exception cref="System.IO.IOException">
        /// An I/O error occurs.
        /// </exception>
        /// <exception cref="System.ObjectDisposedException">
        /// The stream is closed.
        /// </exception>
        /// <exception cref="System.ArgumentOutOfRangeException">
        /// count is negative.
        /// </exception>
        public byte[] ReadBytes(int count)
        {
            if (count < 0)
                throw new ArgumentOutOfRangeException("count", "A non-negative number is required.");

            if (m_stream == null)
                throw new ObjectDisposedException("m_stream");

            byte[] buffer = new byte[count];

            this.InternalResetUnalignedBits();

            int read = m_stream.Read(buffer, 0, count);
            if (read != count)
                throw new EndOfStreamException("Expected " + count + ", read " + read);

            return buffer;
        }

        /// <summary>
        /// Reads the next character from the current stream and advances the current
        /// position of the stream in accordance with the Encoding used and the specific
        /// character being read from the stream.
        /// </summary>
        /// <returns>
        /// A character read from the current stream.
        /// </returns>
        /// <exception cref="System.IO.EndOfStreamException">
        /// The end of the stream is reached.
        /// </exception>
        /// <exception cref="System.ObjectDisposedException">
        /// The stream is closed.
        /// </exception>
        /// <exception cref="System.IO.IOException">
        /// An I/O error occurs.
        /// </exception>
        /// <exception cref="System.ArgumentException">
        /// A surrogate character was read.
        /// </exception>
        public char ReadChar()
        {
            this.InternalResetUnalignedBits();

            int num = this.Read();
            if (num == -1)
                throw new EndOfStreamException();

            return (char)num;
        }

        /// <summary>
        /// Reads the next character from the current stream and advances the current
        /// position of the stream in accordance with the Encoding used and the specific
        /// character being read from the stream.
        /// </summary>
        /// <param name="value">
        /// A character read from the current stream.
        /// </param>
        /// <returns>
        /// </returns>
        /// <exception cref="System.IO.EndOfStreamException">
        /// The end of the stream is reached.
        /// </exception>
        /// <exception cref="System.ObjectDisposedException">
        /// The stream is closed.
        /// </exception>
        /// <exception cref="System.IO.IOException">
        /// An I/O error occurs.
        /// </exception>
        /// <exception cref="System.ArgumentException">
        /// A surrogate character was read.
        /// </exception>
        public StreamHandler ReadChar(out char value)
        {
            this.InternalResetUnalignedBits();

            value = this.ReadChar();
            return this;
        }

        /// <summary>
        /// Reads the specified number of characters from the current stream, returns
        /// the data in a character array, and advances the current position in accordance
        /// with the Encoding used and the specific character being read from the stream.
        /// </summary>
        /// <param name="count">
        /// The number of characters to read.
        /// </param>
        /// <returns>
        /// A character array containing data read from the underlying stream. This might
        /// be less than the number of characters requested if the end of the stream
        /// is reached.
        /// </returns>
        /// <exception cref="System.ObjectDisposedException">
        /// The stream is closed.
        /// </exception>
        /// <exception cref="System.IO.IOException">
        /// An I/O error occurs.
        /// </exception>
        /// <exception cref="System.ArgumentOutOfRangeException">
        /// count is negative.
        /// </exception>
        public char[] ReadChars(int count)
        {
            if (count < 0)
                throw new ArgumentOutOfRangeException("count", "A non-negative number is required.");

            if (m_stream == null)
                throw new ObjectDisposedException("m_stream");

            var buffer = new List<char>(count);

            this.InternalResetUnalignedBits();

            for (int i = 0; i < count; ++i)
                buffer.Add(this.ReadChar());

            return buffer.ToArray();
        }

        /// <summary>
        /// Reads a decimal value from the current stream and advances the current position
        /// of the stream by sixteen bytes.
        /// </summary>
        /// <returns>
        /// A decimal value read from the current stream.
        /// </returns>
        /// <exception cref="System.IO.EndOfStreamException">
        /// The end of the stream is reached.
        /// </exception>
        /// <exception cref="System.ObjectDisposedException">
        /// The stream is closed.
        /// </exception>
        /// <exception cref="System.IO.IOException">
        /// An I/O error occurs.
        /// </exception>
        public decimal ReadDecimal()
        {
            this.InternalResetUnalignedBits();

            this.FillBuffer(16);
            int lo = ((m_buffer[0] | (m_buffer[1] << 8)) | (m_buffer[2] << 16)) | (m_buffer[3] << 24);
            int mid = ((m_buffer[4] | (m_buffer[5] << 8)) | (m_buffer[6] << 16)) | (m_buffer[7] << 24);
            int hi = ((m_buffer[8] | (m_buffer[9] << 8)) | (m_buffer[10] << 16)) | (m_buffer[11] << 24);
            return new decimal(new int[] { lo, mid, hi,
                ((m_buffer[12] | (m_buffer[13] << 8)) | (m_buffer[14] << 16)) | (m_buffer[15] << 24) });
        }

        /// <summary>
        /// Reads an 8-byte floating point value from the current stream and advances
        /// the current position of the stream by eight bytes.
        /// </summary>
        /// <returns>
        /// An 8-byte floating point value read from the current stream.
        /// </returns>
        /// <exception cref="System.IO.EndOfStreamException">
        /// The end of the stream is reached.
        /// </exception>
        /// <exception cref="System.ObjectDisposedException">
        /// The stream is closed.
        /// </exception>
        /// <exception cref="System.IO.IOException">
        /// An I/O error occurs.
        /// </exception>
        public unsafe double ReadDouble()
        {
            this.InternalResetUnalignedBits();

            this.FillBuffer(8);
            fixed (byte* buf = m_buffer)
                return *(double*)buf;
        }

        /// <summary>
        /// Reads an 8-byte floating point value from the current stream and advances
        /// the current position of the stream by eight bytes.
        /// </summary>
        /// <param name="value">
        /// An 8-byte floating point value read from the current stream.
        /// </param>
        /// <returns>
        /// The current instance of <see cref="Kamilla.IO.StreamHandler"/>.
        /// </returns>
        /// <exception cref="System.IO.EndOfStreamException">
        /// The end of the stream is reached.
        /// </exception>
        /// <exception cref="System.ObjectDisposedException">
        /// The stream is closed.
        /// </exception>
        /// <exception cref="System.IO.IOException">
        /// An I/O error occurs.
        /// </exception>
        public unsafe StreamHandler ReadDouble(out double value)
        {
            this.InternalResetUnalignedBits();

            this.FillBuffer(8);
            fixed (byte* buf = m_buffer)
            fixed (double* pvalue = &value)
                *pvalue = *(double*)buf;

            return this;
        }

        /// <summary>
        /// Reads a 2-byte signed integer from the current stream and advances the current
        /// position of the stream by two bytes.
        /// </summary>
        /// <returns>
        /// A 2-byte signed integer read from the current stream.
        /// </returns>
        /// <exception cref="System.IO.EndOfStreamException">
        /// The end of the stream is reached.
        /// </exception>
        /// <exception cref="System.ObjectDisposedException">
        /// The stream is closed.
        /// </exception>
        /// <exception cref="System.IO.IOException">
        /// An I/O error occurs.
        /// </exception>
        public unsafe short ReadInt16()
        {
            this.InternalResetUnalignedBits();

            this.FillBuffer(2);
            fixed (byte* buf = m_buffer)
                return *(short*)buf;
        }

        /// <summary>
        /// Reads a 2-byte signed integer from the current stream and advances the current
        /// position of the stream by two bytes.
        /// </summary>
        /// <param name="value">
        /// A 2-byte signed integer read from the current stream.
        /// </param>
        /// <returns>
        /// The current instance of <see cref="Kamilla.IO.StreamHandler"/>.
        /// </returns>
        /// <exception cref="System.IO.EndOfStreamException">
        /// The end of the stream is reached.
        /// </exception>
        /// <exception cref="System.ObjectDisposedException">
        /// The stream is closed.
        /// </exception>
        /// <exception cref="System.IO.IOException">
        /// An I/O error occurs.
        /// </exception>
        public unsafe StreamHandler ReadInt16(out short value)
        {
            this.InternalResetUnalignedBits();

            this.FillBuffer(2);
            fixed (byte* buf = m_buffer)
            fixed (short* pvalue = &value)
                *pvalue = *(short*)buf;

            return this;
        }

        /// <summary>
        /// Reads a 4-byte signed integer from the current stream and advances the current
        /// position of the stream by four bytes.
        /// </summary>
        /// <returns>
        /// A 4-byte signed integer read from the current stream.
        /// </returns>
        /// <exception cref="System.IO.EndOfStreamException">
        /// The end of the stream is reached.
        /// </exception>
        /// <exception cref="System.ObjectDisposedException">
        /// The stream is closed.
        /// </exception>
        /// <exception cref="System.IO.IOException">
        /// An I/O error occurs.
        /// </exception>
        public unsafe int ReadInt32()
        {
            this.InternalResetUnalignedBits();

            this.FillBuffer(4);
            fixed (byte* buf = m_buffer)
                return *(int*)buf;
        }

        /// <summary>
        /// Reads a 4-byte signed integer from the current stream and advances the current
        /// position of the stream by four bytes.
        /// </summary>
        /// <param name="value">
        /// A 4-byte signed integer read from the current stream.
        /// </param>
        /// <returns>
        /// The current instance of <see cref="Kamilla.IO.StreamHandler"/>.
        /// </returns>
        /// <exception cref="System.IO.EndOfStreamException">
        /// The end of the stream is reached.
        /// </exception>
        /// <exception cref="System.ObjectDisposedException">
        /// The stream is closed.
        /// </exception>
        /// <exception cref="System.IO.IOException">
        /// An I/O error occurs.
        /// </exception>
        public unsafe StreamHandler ReadInt32(out int value)
        {
            this.InternalResetUnalignedBits();

            this.FillBuffer(4);
            fixed (byte* buf = m_buffer)
            fixed (int* pvalue = &value)
                *pvalue = *(int*)buf;

            return this;
        }

        /// <summary>
        /// Reads an 8-byte signed integer from the current stream and advances the current
        /// position of the stream by eight bytes.
        /// </summary>
        /// <returns>
        /// An 8-byte signed integer read from the current stream.
        /// </returns>
        /// <exception cref="System.IO.EndOfStreamException">
        /// The end of the stream is reached.
        /// </exception>
        /// <exception cref="System.ObjectDisposedException">
        /// The stream is closed.
        /// </exception>
        /// <exception cref="System.IO.IOException">
        /// An I/O error occurs.
        /// </exception>
        public unsafe long ReadInt64()
        {
            this.InternalResetUnalignedBits();

            this.FillBuffer(8);
            fixed (byte* buf = m_buffer)
                return *(long*)buf;
        }

        /// <summary>
        /// Reads an 8-byte signed integer from the current stream and advances the current
        /// position of the stream by eight bytes.
        /// </summary>
        /// <param name="value">
        /// An 8-byte signed integer read from the current stream.
        /// </param>
        /// <returns>
        /// The current instance of <see cref="Kamilla.IO.StreamHandler"/>.
        /// </returns>
        /// <exception cref="System.IO.EndOfStreamException">
        /// The end of the stream is reached.
        /// </exception>
        /// <exception cref="System.ObjectDisposedException">
        /// The stream is closed.
        /// </exception>
        /// <exception cref="System.IO.IOException">
        /// An I/O error occurs.
        /// </exception>
        public unsafe StreamHandler ReadInt64(out long value)
        {
            this.InternalResetUnalignedBits();

            this.FillBuffer(8);
            fixed (byte* buf = m_buffer)
            fixed (long* pvalue = &value)
                *pvalue = *(long*)buf;

            return this;
        }

        /// <summary>
        /// Reads a signed byte from this stream and advances the current position of
        /// the stream by one byte.
        /// </summary>
        /// <returns>
        /// A signed byte read from the current stream.
        /// </returns>
        /// <exception cref="System.IO.EndOfStreamException">
        /// The end of the stream is reached.
        /// </exception>
        /// <exception cref="System.ObjectDisposedException">
        /// The stream is closed.
        /// </exception>
        /// <exception cref="System.IO.IOException">
        /// An I/O error occurs.
        /// </exception>
        public sbyte ReadSByte()
        {
            this.InternalResetUnalignedBits();

            this.FillBuffer(1);
            return (sbyte)m_buffer[0];
        }

        /// <summary>
        /// Reads a signed byte from this stream and advances the current position of
        /// the stream by one byte.
        /// </summary>
        /// <param name="value">
        /// A signed byte read from the current stream.
        /// </param>
        /// <returns>
        /// The current instance of <see cref="Kamilla.IO.StreamHandler"/>.
        /// </returns>
        /// <exception cref="System.IO.EndOfStreamException">
        /// The end of the stream is reached.
        /// </exception>
        /// <exception cref="System.ObjectDisposedException">
        /// The stream is closed.
        /// </exception>
        /// <exception cref="System.IO.IOException">
        /// An I/O error occurs.
        /// </exception>
        public StreamHandler ReadSByte(out sbyte value)
        {
            this.InternalResetUnalignedBits();

            this.FillBuffer(1);
            value = (sbyte)m_buffer[0];

            return this;
        }

        /// <summary>
        /// Reads a 4-byte floating point value from the current stream and advances
        /// the current position of the stream by four bytes.
        /// </summary>
        /// <returns>
        /// A 4-byte floating point value read from the current stream.
        /// </returns>
        /// <exception cref="System.IO.EndOfStreamException">
        /// The end of the stream is reached.
        /// </exception>
        /// <exception cref="System.ObjectDisposedException">
        /// The stream is closed.
        /// </exception>
        /// <exception cref="System.IO.IOException">
        /// An I/O error occurs.
        /// </exception>
        public unsafe float ReadSingle()
        {
            this.InternalResetUnalignedBits();

            this.FillBuffer(4);
            fixed (byte* buf = m_buffer)
                return *(float*)buf;
        }

        /// <summary>
        /// Reads a 4-byte floating point value from the current stream and advances
        /// the current position of the stream by four bytes.
        /// </summary>
        /// <param name="value">
        /// A 4-byte floating point value read from the current stream.
        /// </param>
        /// <returns>
        /// The current instance of <see cref="Kamilla.IO.StreamHandler"/>.
        /// </returns>
        /// <exception cref="System.IO.EndOfStreamException">
        /// The end of the stream is reached.
        /// </exception>
        /// <exception cref="System.ObjectDisposedException">
        /// The stream is closed.
        /// </exception>
        /// <exception cref="System.IO.IOException">
        /// An I/O error occurs.
        /// </exception>
        public unsafe StreamHandler ReadSingle(out float value)
        {
            this.InternalResetUnalignedBits();

            this.FillBuffer(4);
            fixed (byte* buf = m_buffer)
            fixed (float* pvalue = &value)
                *pvalue = *(float*)buf;

            return this;
        }

        /// <summary>
        /// Reads a string from the current stream using the current <see cref="System.Text.Encoding"/>,
        /// and advances the stream position by the number of bytes.
        /// </summary>
        /// <param name="numBytes">
        /// Number of bytes that represent the stream.
        /// </param>
        /// <returns>
        /// The read string.
        /// </returns>
        /// <exception cref="System.IO.EndOfStreamException">
        /// The end of the stream is reached.
        /// </exception>
        /// <exception cref="System.ObjectDisposedException">
        /// The stream is closed.
        /// </exception>
        /// <exception cref="System.IO.IOException">
        /// An I/O error occurs.
        /// </exception>
        /// <exception cref="System.ArgumentOutOfRangeException">
        /// numBytes is negative.
        /// </exception>
        public string ReadString(int numBytes)
        {
            if (numBytes < 0)
                throw new ArgumentOutOfRangeException("numBytes");

            this.InternalResetUnalignedBits();

            var bytes = this.ReadBytes(numBytes);
            return m_encoding.GetString(bytes);
        }

        /// <summary>
        /// Reads a null-terminated string from the current stream.
        /// </summary>
        /// <returns>
        /// The read string.
        /// </returns>
        /// <exception cref="System.IO.EndOfStreamException">
        /// The end of the stream is reached.
        /// </exception>
        /// <exception cref="System.ObjectDisposedException">
        /// The stream is closed.
        /// </exception>
        /// <exception cref="System.IO.IOException">
        /// An I/O error occurs.
        /// </exception>
        public string ReadCString()
        {
            const int allocationCount = 100;

            long left = RemainingLength;
            if (left == 0)
                throw new EndOfStreamException();

            var list = new List<byte>((int)Math.Min(left, allocationCount));

            this.InternalResetUnalignedBits();

            byte b;
            bool caretReturn = false;
            bool terminatorFound = false;
            do
            {
                b = ReadByte();
                switch (b)
                {
                    case 0:
                        terminatorFound = true;
                        break;
                    case (byte)'\r':
                        caretReturn = true;
                        break;
                    case (byte)'\n':
                        if (list.Count == list.Capacity)
                            list.Capacity += list.Capacity;

                        list.AddRange(s_newLineBytes);
                        caretReturn = false;
                        break;
                    default:
                        if (list.Count == list.Capacity)
                            list.Capacity += list.Capacity;

                        if (caretReturn)
                        {
                            list.AddRange(s_newLineBytes);
                            caretReturn = false;
                        }

                        list.Add(b);
                        break;
                }
            } while (!terminatorFound && !IsRead);

            return m_encoding.GetString(list.ToArray());
        }

        /// <summary>
        /// Reads a 2-byte unsigned integer from the current stream using little-endian
        /// encoding and advances the position of the stream by two bytes.
        /// </summary>
        /// <returns>
        /// A 2-byte unsigned integer read from this stream.
        /// </returns>
        /// <exception cref="System.IO.EndOfStreamException">
        /// The end of the stream is reached.
        /// </exception>
        /// <exception cref="System.ObjectDisposedException">
        /// The stream is closed.
        /// </exception>
        /// <exception cref="System.IO.IOException">
        /// An I/O error occurs.
        /// </exception>
        public unsafe ushort ReadUInt16()
        {
            this.InternalResetUnalignedBits();

            this.FillBuffer(2);
            fixed (byte* buf = m_buffer)
                return *(ushort*)buf;
        }

        /// <summary>
        /// Reads a 4-byte unsigned integer from the current stream and advances the
        /// position of the stream by four bytes.
        /// </summary>
        /// <returns>
        /// A 4-byte unsigned integer read from this stream.
        /// </returns>
        /// <exception cref="System.IO.EndOfStreamException">
        /// The end of the stream is reached.
        /// </exception>
        /// <exception cref="System.ObjectDisposedException">
        /// The stream is closed.
        /// </exception>
        /// <exception cref="System.IO.IOException">
        /// An I/O error occurs.
        /// </exception>
        public unsafe uint ReadUInt32()
        {
            this.InternalResetUnalignedBits();

            this.FillBuffer(4);
            fixed (byte* buf = m_buffer)
                return *(uint*)buf;
        }

        /// <summary>
        /// Reads a 4-byte unsigned integer from the current stream and advances the
        /// position of the stream by four bytes.
        /// </summary>
        /// <param name="value">
        /// A 4-byte unsigned integer read from this stream.
        /// </param>
        /// <returns>
        /// The current instance of <see cref="Kamilla.IO.StreamHandler"/>.
        /// </returns>
        /// <exception cref="System.IO.EndOfStreamException">
        /// The end of the stream is reached.
        /// </exception>
        /// <exception cref="System.ObjectDisposedException">
        /// The stream is closed.
        /// </exception>
        /// <exception cref="System.IO.IOException">
        /// An I/O error occurs.
        /// </exception>
        public unsafe StreamHandler ReadUInt32(out uint value)
        {
            this.InternalResetUnalignedBits();

            this.FillBuffer(4);
            fixed (byte* buf = m_buffer)
                value = *(uint*)buf;

            return this;
        }

        /// <summary>
        /// Reads an 8-byte unsigned integer from the current stream and advances the
        /// position of the stream by eight bytes.
        /// </summary>
        /// <returns>
        /// An 8-byte unsigned integer read from this stream.
        /// </returns>
        /// <exception cref="System.IO.EndOfStreamException">
        /// The end of the stream is reached.
        /// </exception>
        /// <exception cref="System.IO.IOException">
        /// An I/O error occurs.
        /// </exception>
        /// <exception cref="System.ObjectDisposedException">
        /// The stream is closed.
        /// </exception>
        public unsafe ulong ReadUInt64()
        {
            this.InternalResetUnalignedBits();

            this.FillBuffer(8);
            fixed (byte* buf = m_buffer)
                return *(ulong*)buf;
        }

        /// <summary>
        /// Reads an 8-byte unsigned integer from the current stream and advances the
        /// position of the stream by eight bytes.
        /// </summary>
        /// <param name="value">
        /// An 8-byte unsigned integer read from this stream.
        /// </param>
        /// <returns>
        /// The current instance of <see cref="Kamilla.IO.StreamHandler"/>.
        /// </returns>
        /// <exception cref="System.IO.EndOfStreamException">
        /// The end of the stream is reached.
        /// </exception>
        /// <exception cref="System.IO.IOException">
        /// An I/O error occurs.
        /// </exception>
        /// <exception cref="System.ObjectDisposedException">
        /// The stream is closed.
        /// </exception>
        public unsafe StreamHandler ReadUInt64(out ulong value)
        {
            this.InternalResetUnalignedBits();

            this.FillBuffer(8);
            fixed (byte* buf = m_buffer)
                value = *(ulong*)buf;

            return this;
        }

        /// <summary>
        /// Reads a three-component vector from the current stream
        /// and advences the position of the stream by twelve bytes.
        /// </summary>
        /// <returns>
        /// A <see cref="Kamilla.Vector3"/> read from the stream.
        /// </returns>
        /// <exception cref="System.IO.EndOfStreamException">
        /// The end of the stream is reached.
        /// </exception>
        /// <exception cref="System.IO.IOException">
        /// An I/O error occurs.
        /// </exception>
        /// <exception cref="System.ObjectDisposedException">
        /// The stream is closed.
        /// </exception>
        public unsafe Vector3 ReadVector3()
        {
            this.InternalResetUnalignedBits();

            this.FillBuffer(4 * 3);
            fixed (byte* buf = m_buffer)
                return *(Vector3*)buf;
        }

        /// <summary>
        /// Reads a two-component vector from the current stream
        /// and advences the position of the stream by eight bytes.
        /// </summary>
        /// <returns>
        /// A <see cref="Kamilla.Vector2"/> read from the stream.
        /// </returns>
        /// <exception cref="System.IO.EndOfStreamException">
        /// The end of the stream is reached.
        /// </exception>
        /// <exception cref="System.IO.IOException">
        /// An I/O error occurs.
        /// </exception>
        /// <exception cref="System.ObjectDisposedException">
        /// The stream is closed.
        /// </exception>
        public unsafe Vector2 ReadVector2()
        {
            this.InternalResetUnalignedBits();

            this.FillBuffer(4 * 2);
            fixed (byte* buf = m_buffer)
                return *(Vector2*)buf;
        }

        /// <summary>
        /// Reads all bytes from the current stream, not advancing the current position.
        /// </summary>
        /// <returns>
        /// The byte array that contains all bytes from the current stream.
        /// </returns>
        /// <exception cref="System.IO.EndOfStreamException">
        /// The end of the stream is reached.
        /// </exception>
        /// <exception cref="System.IO.IOException">
        /// An I/O error occurs.
        /// </exception>
        /// <exception cref="System.ObjectDisposedException">
        /// The stream is closed.
        /// </exception>
        public byte[] ToByteArray()
        {
            this.Flush();
            this.FlushUnalignedBits();

            long pos = m_stream.Position;
            m_stream.Position = 0;

            byte[] buffer = new byte[m_stream.Length];
            m_stream.Read(buffer, 0, (int)m_stream.Length);

            m_stream.Position = pos;
            return buffer;
        }

        /// <summary>
        /// Reads a structure from the current stream, advancing
        /// the current position by the number of bytes in the structure.
        /// </summary>
        /// <typeparam name="T">
        /// Type of the structure.
        /// </typeparam>
        /// <returns>
        /// A structure read from the stream.
        /// </returns>
        public T ReadStruct<T>() where T : struct
        {
            lock (StructHelper<T>.SyncRoot)
            {
                this.InternalResetUnalignedBits();

                m_stream.Read(StructHelper<T>.ManagedDataBank, 0, StructHelper<T>.Size);
                Marshal.Copy(StructHelper<T>.ManagedDataBank, 0, StructHelper<T>.UnmanagedDataBank, StructHelper<T>.Size);
                return (T)Marshal.PtrToStructure(StructHelper<T>.UnmanagedDataBank, StructHelper<T>.Type);
            }
        }
        #endregion

        #region DoAt Methods
        /// <summary>
        /// Runs the given function temporary setting the current stream's position to the specified value.
        /// </summary>
        /// <typeparam name="TArg">Type of the function argument.</typeparam>
        /// <typeparam name="TResult">The return type of the function.</typeparam>
        /// <param name="position">The position to run the function at.</param>
        /// <param name="func">The function to run at the given position.</param>
        /// <param name="arg">The argument passed to the function.</param>
        /// <returns>The result of the function.</returns>
        public TResult DoAt<TArg, TResult>(long position, Func<TArg, TResult> func, TArg arg)
        {
            if (position == this.Position)
                return func(arg);
            else
            {
                long old_position = this.Position;
                this.Position = position;
                TResult result = func(arg);
                this.Position = old_position;
                return result;
            }
        }

        /// <summary>
        /// Runs the given function temporary setting the current stream's position to the specified value.
        /// </summary>
        /// <typeparam name="TResult">The return type of the function.</typeparam>
        /// <param name="position">The position to run the function at.</param>
        /// <param name="func">The function to run at the given position.</param>
        /// <returns>The result of the function.</returns>
        public TResult DoAt<TResult>(long position, Func<TResult> func)
        {
            if (position == this.Position)
                return func();
            else
            {
                long old_position = this.Position;
                this.Position = position;
                TResult result = func();
                this.Position = old_position;
                return result;
            }
        }

        /// <summary>
        /// Runs the given function temporary setting the current stream's position to the specified value.
        /// </summary>
        /// <typeparam name="TArg">Type of the function argument.</typeparam>
        /// <param name="position">The position to run the function at.</param>
        /// <param name="func">The function to run at the given position.</param>
        /// <param name="arg">The argument passed to the function.</param>
        public void DoAt<TArg>(long position, Action<TArg> func, TArg arg)
        {
            if (position == this.Position)
                func(arg);
            else
            {
                long old_position = this.Position;
                this.Position = position;
                func(arg);
                this.Position = old_position;
            }
        }

        /// <summary>
        /// Runs the given function temporary setting the current stream's position to the specified value.
        /// </summary>
        /// <param name="position">The position to run the function at.</param>
        /// <param name="func">The function to run at the given position.</param>
        public void DoAt(long position, Action func)
        {
            if (position == this.Position)
                func();
            else
            {
                long old_position = this.Position;
                this.Position = position;
                func();
                this.Position = old_position;
            }
        }
        #endregion
    }
}
