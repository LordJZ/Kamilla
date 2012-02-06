using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Kamilla.Network
{
    /// <summary>
    /// Defines opcode values with special meaning.
    /// </summary>
    public class SpecialOpcodes
    {
        /// <summary>
        /// Defines an unknown opcode. This field is constant.
        /// </summary>
        public const uint UnknownOpcode = uint.MaxValue;
    }
}
