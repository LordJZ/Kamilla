using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Kamilla.Network.Viewing
{
    /// <summary>
    /// Provides an interface for plugins to the
    /// <see cref="Kamilla.Network.Viewing.NetworkLogViewerBase"/>.
    /// </summary>
    public interface NetworkLogViewerBasePlugin
    {
        /// <summary>
        /// Loads the current instance of
        /// <see cref="Kamilla.Network.Viewing.NetworkLogViewerBasePlugin"/>.
        /// </summary>
        /// <param name="viewer">
        /// The <see cref="Kamilla.Network.Viewing.NetworkLogViewerBase"/>
        /// that the current plugin is attached to.
        /// </param>
        /// <returns>
        /// true if the plugin has successfully integrated with
        /// the <see cref="Kamilla.Network.Viewing.NetworkLogViewerBase"/>,
        /// the current <see cref="Kamilla.Network.Logging.NetworkLog"/>,
        /// and the current <see cref="Kamilla.Network.Protocols.Protocol"/>;
        /// otherwise, false.
        /// </returns>
        bool Load(NetworkLogViewerBase viewer);

        /// <summary>
        /// Unloads and releases all resources used by the current instance of
        /// <see cref="Kamilla.Network.Viewing.NetworkLogViewerBasePlugin"/>.
        /// </summary>
        void Unload();
    }
}
