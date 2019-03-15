﻿using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using Kamilla.Network;
using Kamilla.Network.Logging;
using Kamilla.Network.Parsing;
using Kamilla.Network.Protocols;
using Kamilla.Network.Viewing;

namespace Kamilla.PoE.Viewer
{
    public class PoELoginProtocol : Protocol
    {
        sealed class ItemVisualData
        {
            readonly ViewerItem m_item;
            string m_c2sStr;
            string m_s2cStr;

            /// <summary>
            /// Initializes a new instance of
            /// <see cref="Kamilla.Network.Protocols.DefaultProtocol.ItemVisualData"/> class.
            /// </summary>
            /// <param name="item">
            /// The underlying instance of <see cref="Kamilla.Network.Viewing.ViewerItem"/> class.
            /// </param>
            /// <exception cref="System.ArgumentNullException">
            /// <c>item</c> is null.
            /// </exception>
            public ItemVisualData(ViewerItem item)
            {
                if (item == null)
                    throw new ArgumentNullException("item");

                m_item = item;
            }

            public string ArrivalTime
            {
                get { return m_item.Packet.ArrivalTime.ToString("HH:mm:ss"); }
            }

            public string ArrivalTicks
            {
                get
                {
                    var log = m_item.Log as IHasStartTicks;
                    if (log != null)
                        return ((int)(m_item.Packet.ArrivalTicks - log.StartTicks)).ToString();

                    return m_item.Packet.ArrivalTicks.ToString();
                }
            }

            public string C2sStr
            {
                get
                {
                    if (m_c2sStr == null)
                    {
                        var packet = m_item.Packet;
                        if (packet.Direction == TransferDirection.ToServer)
                            m_c2sStr = PoELoginParser.GetOpcode(packet)?.ToString() ??
                                       NetworkStrings.bytes.LocalizedFormat(packet.Data.Length);
                        else
                            m_c2sStr = string.Empty;
                    }

                    return m_c2sStr;
                }
            }

            public string S2cStr
            {
                get
                {
                    if (m_s2cStr == null)
                    {
                        var packet = m_item.Packet;
                        if (packet.Direction == TransferDirection.ToClient)
                            m_s2cStr = PoELoginParser.GetOpcode(packet)?.ToString() ??
                                       NetworkStrings.bytes.LocalizedFormat(packet.Data.Length);
                        else
                            m_s2cStr = string.Empty;
                    }

                    return m_s2cStr;
                }
            }
        }

        NetworkLogViewerBase m_viewer;
        GridView m_view;
        ViewerItemEventHandler m_itemVisualDataQueriedHandler;

        public PoELoginProtocol()
        {
            m_itemVisualDataQueriedHandler = new ViewerItemEventHandler(viewer_ItemVisualDataQueried);
        }

        public override string Name
        {
            get { return "Path of Exile (Login)"; }
        }

        public override string CodeName
        {
            get { return "Path of Exile (Login)"; }
        }

        public override Type OpcodesEnumType
        {
            get { return null; }
        }

        void viewer_ItemVisualDataQueried(object sender, ViewerItemEventArgs e)
        {
            var item = e.Item;
            if (item.VisualData == null)
                item.VisualData = new ItemVisualData(item);
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

        static readonly string[] s_columnBindings = new string[]
        {
            ".Index",
            ".VisualData.ArrivalTime",
            ".VisualData.ArrivalTicks",
            ".VisualData.C2sStr",
            ".VisualData.S2cStr",
            ".Packet.Data.Length",
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
            viewer.ItemVisualDataQueried += m_itemVisualDataQueriedHandler;

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

                var dataTemplate = new DataTemplate();
                dataTemplate.DataType = typeof(ItemVisualData);

                var block = new FrameworkElementFactory(typeof(TextBlock));
                block.SetValue(TextBlock.TextProperty, new Binding(s_columnBindings[col]));

                dataTemplate.VisualTree = block;
                item.CellTemplate = dataTemplate;

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

            m_viewer.ItemVisualDataQueried -= m_itemVisualDataQueriedHandler;
            m_viewer = null;
        }

        protected override PacketParser InternalCreateParser(ViewerItem item)
        {
            return new PoELoginParser();
        }

        public override string PacketContentsViewHeader(ViewerItem item)
        {
            return string.Empty;
        }
    }
}
