using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Kamilla.Network.Logging
{
    /// <summary>
    /// Defines flags of a packet dump reading class.
    /// </summary>
    [Flags]
    public enum NetworkLogFlags
    {
        /// <summary>
        /// Indicates no flags. 
        /// </summary>
        None = 0x00,

        /// <summary>
        /// Indicates that the underlying dump reading class only supports reading data.
        /// </summary>
        ReadOnly = 0x04,

        /// <summary>
        /// Indicates that the underlying dump reading class implementation is experimental.
        /// </summary>
        Experimental = 0x08,
    }
}
