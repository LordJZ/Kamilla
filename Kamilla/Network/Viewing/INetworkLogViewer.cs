using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Kamilla.Network.Logging;

namespace Kamilla.Network.Viewing
{
    /// <summary>
    /// Represents an abstract <see cref="Kamilla.Network.Logging.NetworkLog"/> viewer.
    /// </summary>
    public interface INetworkLogViewer
    {
        /// <summary>
        /// Gets the handle of the viewer window. This value can be <see href="System.IntPtr.Zero"/>.
        /// </summary>
        IntPtr WindowHandle { get; }

        /// <summary>
        /// Gets the currently viewed <see cref="Kamilla.Network.Logging.NetworkLog"/>. This value can be null.
        /// </summary>
        NetworkLog CurrentLog { get; }
    }
}
