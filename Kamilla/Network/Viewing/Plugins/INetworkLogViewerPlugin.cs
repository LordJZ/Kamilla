
namespace Kamilla.Network.Viewing.Plugins
{
    /// <summary>
    /// Provides an interface for plugins to a
    /// <see cref="Kamilla.Network.Viewing.NetworkLogViewerBase"/>.
    /// </summary>
    public interface INetworkLogViewerPlugin
    {
        ///// <summary>
        ///// Gets the localized name of the plugin.
        ///// </summary>
        //string Name { get; }

        /// <summary>
        /// Initializes the current instance of
        /// <see cref="Kamilla.Network.Viewing.Plugins.INetworkLogViewerPlugin"/>.
        /// </summary>
        /// <param name="viewer">
        /// The <see cref="Kamilla.Network.Viewing.NetworkLogViewerBase"/>
        /// that the current plugin is attached to.
        /// </param>
        void Initialize(NetworkLogViewerBase viewer);
    }
}
