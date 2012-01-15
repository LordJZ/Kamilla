using System;

namespace Kamilla.Network.Viewing
{
    /// <summary>
    /// Represents the method that handles an event
    /// related to a <see cref="Kamilla.Network.Viewing.ViewerItem"/>.
    /// </summary>
    /// <param name="sender">
    /// The object that raised the event.
    /// </param>
    /// <param name="e">
    /// Information about the event.
    /// </param>
    public delegate void ViewerItemEventHandler(object sender, ViewerItemEventArgs e);

    /// <summary>
    /// Provides data for an event related to a <see cref="Kamilla.Network.Viewing.ViewerItem"/>.
    /// </summary>
    public sealed class ViewerItemEventArgs : EventArgs
    {
        ViewerItem m_item;

        /// <summary>
        /// Initializes a new instance of <see cref="Kamilla.Network.Viewing.ViewerItemEventArgs"/>
        /// for the specified <see cref="Kamilla.Network.Viewing.ViewerItem"/>.
        /// </summary>
        /// <param name="item">
        /// The <see cref="Kamilla.Network.Viewing.ViewerItem"/> related to the event.
        /// </param>
        public ViewerItemEventArgs(ViewerItem item)
        {
            m_item = item;
        }

        /// <summary>
        /// Gets the <see cref="Kamilla.Network.Viewing.ViewerItem"/> related to the event.
        /// </summary>
        public ViewerItem Item { get { return m_item; } }
    }
}
