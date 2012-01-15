using Kamilla.Network.Parsing;
using Kamilla.Network.Logging;

namespace Kamilla.Network.Viewing
{
    public class ViewerItem
    {
        public INetworkLogViewer Viewer { get; private set; }

        public Packet Packet { get; private set; }

        public PacketParser Parser { get; internal set; }

        public NetworkLog Log { get; private set; }

        /// <summary>
        /// Gets the counter of the viewer item.
        /// </summary>
        public int Index { get; private set; }

        /// <summary>
        /// Gets or sets the data associated with the current item.
        /// </summary>
        public string[] Data { get; set; }

        public ViewerItem(INetworkLogViewer viewer, NetworkLog log, int index, Packet packet)
        {
            this.Log = log;
            this.Viewer = viewer;
            this.Index = index;
            this.Packet = packet;
        }
    }
}
