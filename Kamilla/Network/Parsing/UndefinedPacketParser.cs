using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Kamilla.Network.Parsing
{
    /// <summary>
    /// Represents an unkown or undefined packet parser.  This parser does nothing.
    /// </summary>
    public sealed class UndefinedPacketParser : PacketParser
    {
        protected override void InternalParse()
        {
        }
    }
}
