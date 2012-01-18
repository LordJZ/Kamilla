using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Kamilla.Network.Protocols
{
    /// <summary>
    /// Contains identification data of a
    /// <see cref="Kamilla.Network.Protocols.Protocol"/>.
    /// </summary>
    public class ProtocolWrapper
    {
        internal ProtocolWrapper(int index, Type type)
        {
            this.Index = index;
            this.Type = type;
            this.TypeName = this.Type.Name;
            this.Name = this.Activate().Name;
        }

        /// <summary>
        /// Represents opcode/parserType storage.
        /// </summary>
        internal readonly SortedDictionary<uint, Type> m_parsers = new SortedDictionary<uint, Type>();

        /// <summary>
        /// Gets the opcode/parserType storage.
        /// </summary>
        public SortedDictionary<uint, Type> Parsers { get { return m_parsers; } }

        /// <summary>
        /// Gets the zero-based index of the underlying
        /// <see cref="Kamilla.Network.Protocols.Protocol"/>.
        /// </summary>
        public readonly int Index;

        /// <summary>
        /// Gets the <see cref="System.Type"/> of the
        /// underlying <see cref="Kamilla.Network.Protocols.Protocol"/>.
        /// </summary>
        public readonly Type Type;

        /// <summary>
        /// Gets the name of the <see cref="System.Type"/>
        /// of the underlying <see cref="Kamilla.Network.Protocols.Protocol"/>.
        /// </summary>
        public readonly string TypeName;

        /// <summary>
        /// Gets the localized name of the underlying
        /// <see cref="Kamilla.Network.Protocols.Protocol"/>.
        /// </summary>
        public readonly string Name;

        /// <summary>
        /// Creates a new instance of the underlying <see cref="Kamilla.Network.Protocols.Protocol"/>.
        /// </summary>
        /// <returns>
        /// A new instance of the underlying <see cref="Kamilla.Network.Protocols.Protocol"/>.
        /// </returns>
        public Protocol Activate()
        {
            var protocol = (Protocol)Activator.CreateInstance(this.Type);
            protocol.m_wrapper = this;
            return protocol;
        }
    }
}
