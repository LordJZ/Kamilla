using System;
using System.Windows.Controls;
using Kamilla;
using Kamilla.Network.Protocols;
using Kamilla.Network.Viewing;

namespace NetworkLogViewer.ViewTabs
{
    partial class PacketContents : UserControl, IViewTab
    {
        public PacketContents()
        {
            InitializeComponent();
        }

        string IViewTab.Header { get { return Strings.View_PacketContents; } }

        public bool IsFilled { get; set; }

        void IViewTab.Reset()
        {
            ui_tbMain.Text = string.Empty;

            this.IsFilled = false;
        }

        void IViewTab.Fill(Protocol protocol, ViewerItem item)
        {
            var header = protocol.PacketContentsViewHeader(item);

            if (header != null && header != string.Empty)
                ui_tbMain.Text = header + Environment.NewLine + item.Packet.Data.ToHexDump();
            else
                ui_tbMain.Text = item.Packet.Data.ToHexDump();

            this.IsFilled = true;
        }
    }
}
