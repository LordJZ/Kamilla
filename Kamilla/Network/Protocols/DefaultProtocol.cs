using System;
using Kamilla.Network.Logging;
using Kamilla.Network.Parsing;
using Kamilla.Network.Viewing;

namespace Kamilla.Network.Protocols
{
    public sealed class DefaultProtocol : Protocol
    {
        ViewerItemEventHandler m_itemQueriedHandler;

        public DefaultProtocol()
        {
            m_itemQueriedHandler = new ViewerItemEventHandler(viewer_ItemQueried);
        }

        #region Immutable Overrides
        public override string Name
        {
            get { return NetworkStrings.Protocol_Default; }
        }

        public override int ListViewColumns
        {
            get { return s_columnWidths.Length; }
        }

        static readonly int[] s_columnWidths = new int[]
        {
            64,
            85,
            69,
            180,
            180,
            59
        };

        public override string[] ListViewColumnHeaders
        {
            get
            {
                return new string[]
                {
                    NetworkStrings.CH_Number,
                    NetworkStrings.CH_Time,
                    NetworkStrings.CH_Ticks,
                    NetworkStrings.CH_C2S,
                    NetworkStrings.CH_S2C,
                    NetworkStrings.CH_Length,
                };
            }
        }

        public override int[] ListViewColumnWidths
        {
            get { return s_columnWidths; }
        }
        #endregion

        INetworkLogViewer m_viewer;

        void viewer_ItemQueried(object sender, ViewerItemEventArgs e)
        {
            var item = e.Item;

            var arr = new string[this.ListViewColumns];

            var packet = item.Packet;

            arr[0] = item.Index.ToString();
            arr[1] = packet.ArrivalTime.ToString("HH:mm:ss");

            uint startTicks = 0;
            if (item.Log is IHasStartTicks)
                startTicks = ((IHasStartTicks)item.Log).StartTicks;

            arr[2] = ((int)(packet.ArrivalTicks - startTicks)).ToString();

            if (packet.Direction == TransferDirection.ToServer)
            {
                arr[3] = NetworkStrings.bytes.LocalizedFormat(packet.Data.Length);
                arr[4] = string.Empty;
            }
            else
            {
                arr[3] = string.Empty;
                arr[4] = NetworkStrings.bytes.LocalizedFormat(packet.Data.Length);
            }

            arr[5] = packet.Data.Length.ToString();

            item.Data = arr;
        }

        public override void Load(INetworkLogViewer viewer)
        {
            if (m_viewer != null)
                throw new InvalidOperationException();

            viewer.ItemQueried += m_itemQueriedHandler;

            m_viewer = viewer;
        }

        public override void Unload()
        {
            if (m_viewer == null)
                throw new InvalidOperationException();

            m_viewer.ItemQueried -= m_itemQueriedHandler;

            m_viewer = null;
        }

        protected override PacketParser InternalCreateParser(ViewerItem item)
        {
            return null;
        }
    }
}
