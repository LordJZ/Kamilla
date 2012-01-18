using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Kamilla.Network
{
    /// <summary>
    /// Extends the <see cref="Kamilla.Network.Packet"/> class allowing it to store opcode.
    /// </summary>
    public interface IPacketWithOpcode
    {
        /// <summary>
        /// Gets the opcode of the underlying packet.
        /// </summary>
        uint Opcode { get; }
    }
}
