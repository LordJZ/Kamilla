using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Kamilla.Network.Viewing
{
    /// <summary>
    /// Provides an interface for plugins to the
    /// <see cref="Kamilla.Network.Viewing.INetworkLogViewer"/>.
    /// </summary>
    public interface INetworkLogViewerPlugin
    {
        /// <summary>
        /// Loads the current instance of
        /// <see cref="Kamilla.Network.Viewing.INetworkLogViewerPlugin"/>.
        /// </summary>
        /// <param name="viewer">
        /// The <see cref="Kamilla.Network.Viewing.INetworkLogViewer"/>
        /// that the current plugin is attached to.
        /// </param>
        void Load(INetworkLogViewer viewer);

        /// <summary>
        /// Unloads and releases all resources used by the current instance of
        /// <see cref="Kamilla.Network.Viewing.INetworkLogViewerPlugin"/>.
        /// </summary>
        void Unload();
    }
}
