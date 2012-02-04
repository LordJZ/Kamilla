using System;
using System.Collections.Generic;
using System.Text;
using Kamilla.IO;
using Kamilla.Network.Logging;
using Kamilla.Network.Viewing;

namespace Kamilla.Network.Parsing
{
    /// <summary>
    /// Provides an interface to convert instances of
    /// <see cref="Kamilla.Network.Packet"/> to
    /// human-understandable representation.
    /// </summary>
    public abstract class PacketParser
    {
        internal ViewerItem m_item;

        /// <summary>
        /// Gets the <see cref="Kamilla.Network.Viewing.ViewerItem"/> to which
        /// the current <see cref="Kamilla.Network.Parsing.PacketParser"/> is attached.
        /// </summary>
        public ViewerItem Item { get { return m_item; } }

        /// <summary>
        /// Exposes access to an underlying <see cref="Kamilla.Network.Packet"/>
        /// of the current <see cref="Kamilla.Network.Parsing.PacketParser"/>.
        /// </summary>
        public Packet Packet { get { return m_item.Packet; } }

        /// <summary>
        /// Exposes access to an underlying <see cref="Kamilla.Network.Logging.NetworkLog"/>
        /// of the current <see cref="Kamilla.Network.Parsing.PacketParser"/>.
        /// </summary>
        public NetworkLog Log { get { return m_item.Log; } }

        /// <summary>
        /// Gets a value indicating whether the current instance
        /// of <see cref="Kamilla.Network.Parsing.PacketParser"/> has finished
        /// interpreting the underlying <see cref="Kamilla.Network.Packet"/>.
        /// </summary>
        public bool IsParsed { get; private set; }

        /// <summary>
        /// Gets a value indicating whether an error occured while
        /// interpreting the underlying <see cref="Kamilla.Network.Packet"/>.
        /// </summary>
        public bool ParsingError { get; protected set; }

        /// <summary>
        /// Gets the <see cref="Kamilla.IO.StreamHandler"/> used to read
        /// contents of the underlying <see cref="Kamilla.Network.Packet"/>.
        /// </summary>
        /// <remarks>
        /// Intentionally left as a field.
        /// </remarks>
        protected StreamHandler Reader;

        /// <summary>
        /// Gets the <see cref="System.Text.StringBuilder"/> used to create the output text.
        /// </summary>
        /// <remarks>
        /// Intentionally left as a field.
        /// </remarks>
        protected StringBuilder Output;

        /// <summary>
        /// When implemented in a derived class, performs the interpreting
        /// operations on the underlying <see cref="Kamilla.Network.Packet"/>.
        /// </summary>
        protected abstract void InternalParse();

        object m_syncRoot = new object();

        /// <summary>
        /// Interprets the underlying <see cref="Kamilla.Network.Packet"/>.
        /// </summary>
        public virtual void Parse()
        {
            if (this.IsParsed)
                return;

            lock (m_syncRoot)
            {
                if (this.IsParsed)
                    return;

                this.Output = new StringBuilder();
                this.Reader = new StreamHandler(this.Packet.Data);

                try
                {
                    this.InternalParse();
                }
                catch (Exception ex)
                {
                    if (this.Output == null)
                        this.Output = new StringBuilder();

                    Output.AppendLine();
                    Output.AppendLine(NetworkStrings.ParserException);
                    Output.AppendLine(ex.ToString());
                    this.ParsingError = true;
                }

                if (this.Output == null)
                    this.Output = new StringBuilder();

                if (!(this is UndefinedPacketParser))
                {
                    if (this.Output.Length == 0)
                    {
                        if (this.Packet.Data.Length == 0)
                            Output.AppendLine("(packet is empty)");
                        else
                            Output.AppendLine("(error: packet should be empty).");
                    }

                    Output.AppendLine();
                    Output.AppendLine("________________________________");
                    Output.AppendLine(NetworkStrings.ParserClass.LocalizedFormat(GetType().Name));

                    if (this.Reader != null && !this.Reader.IsRead)
                    {
                        Output.AppendLine(NetworkStrings.BytesRead.LocalizedFormat(
                            this.Reader.Position, this.Reader.Length));
                        ParsingError = true;
                    }
                }

                if (this.Output != null)
                {
                    m_output = this.Output.ToString();
                    this.Output = null;
                }
                else if (m_output == null)
                    m_output = string.Empty;

                this.Reader = null;

                this.IsParsed = true;
            }

            m_item.Viewer.OnItemParsingDone(m_item);
        }

        protected string m_output;
        IList<object> m_containedData;

        /// <summary>
        /// Gets the text result of the underlying <see cref="Kamilla.Network.Packet"/> interpretation.
        /// 
        /// This value can be null.
        /// </summary>
        public string ParsedText { get { return m_output; } }

        /// <summary>
        /// Gets the data that is contained in the underlying <see cref="Kamilla.Network.Packet"/>.
        /// 
        /// This value can be null.
        /// </summary>
        public IList<object> ContainedData
        {
            get { return m_containedData ?? (m_containedData = new List<object>()); }
        }
    }
}
