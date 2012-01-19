using System;
using System.Linq;
using System.Windows.Controls;
using System.Windows.Data;
using Kamilla.Network.Logging;
using Kamilla.Network.Parsing;
using Kamilla.Network.Viewing;

namespace Kamilla.Network.Protocols
{
    public sealed class DefaultProtocol : Protocol
    {
        NetworkLogViewerBase m_viewer;
        GridView m_view;
        ViewerItemEventHandler m_itemQueriedHandler;

        public DefaultProtocol()
        {
            m_itemQueriedHandler = new ViewerItemEventHandler(viewer_ItemQueried);
        }

        public override string Name
        {
            get { return NetworkStrings.Protocol_Default; }
        }

        void viewer_ItemQueried(object sender, ViewerItemEventArgs e)
        {
            var item = e.Item;

            var arr = new string[s_columnWidths.Length];

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

        class GridViewColumnWithId : GridViewColumn
        {
            public int ColumnId;
        }

        static readonly double[] s_columnWidths = new double[]
        {
            64,
            85,
            69,
            180,
            180,
            59
        };

        public override ViewBase View
        {
            get { return m_view; }
        }

        public override void Load(NetworkLogViewerBase viewer)
        {
            if (m_viewer != null)
                throw new InvalidOperationException();

            m_viewer = viewer;
            viewer.ItemQueried += m_itemQueriedHandler;

            var view = m_view = new GridView();

            var nColumns = s_columnWidths.Length;
            var headers = new string[]
            {
                NetworkStrings.CH_Number,
                NetworkStrings.CH_Time,
                NetworkStrings.CH_Ticks,
                NetworkStrings.CH_C2S,
                NetworkStrings.CH_S2C,
                NetworkStrings.CH_Length,
            };
            if (headers.Length != nColumns)
                throw new InvalidOperationException();

            double[] widths = Configuration.GetValue("Column Widths", (double[])null);
            if (widths == null || widths.Length != nColumns)
                widths = s_columnWidths;

            int[] columnOrder = Configuration.GetValue("Column Order", (int[])null);
            if (columnOrder == null || columnOrder.Length != nColumns
                || columnOrder.Any(val => val >= nColumns || val < 0))
                columnOrder = Enumerable.Range(0, nColumns).ToArray();

            for (int i = 0; i < nColumns; i++)
            {
                int col = columnOrder[i];

                var item = new GridViewColumnWithId();
                item.ColumnId = col;
                item.Header = headers[col];
                item.Width = widths[col];
                item.DisplayMemberBinding = new Binding(".Data[" + col + "]");
                view.Columns.Add(item);
            }
        }

        public override void Unload()
        {
            if (m_viewer == null)
                throw new InvalidOperationException();

            var view = m_view;
            var columns = view.Columns;
            var nColumns = columns.Count;

            var widths = new double[nColumns];
            var order = new int[nColumns];

            for (int i = 0; i < nColumns; i++)
            {
                var column = (GridViewColumnWithId)columns[i];

                order[i] = column.ColumnId;
                widths[column.ColumnId] = column.Width;
            }

            Configuration.SetValue("Column Widths", widths);
            Configuration.SetValue("Column Order", order);
            m_view = null;

            m_viewer.ItemQueried -= m_itemQueriedHandler;
            m_viewer = null;
        }

        protected override PacketParser InternalCreateParser(ViewerItem item)
        {
            return null;
        }
    }
}
