using System;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using Kamilla.Network.Logging;
using Kamilla.Network.Parsing;
using Kamilla.Network.Viewing;

namespace Kamilla.Network.Protocols
{
    public sealed class DefaultProtocol : Protocol
    {
        public abstract class BaseItemData : INotifyPropertyChanged
        {
            /// <summary>
            /// Occurs when a property is changed.
            /// </summary>
            public event PropertyChangedEventHandler PropertyChanged;

            protected readonly ViewerItem m_item;
            protected string m_index;
            protected string m_arrivalTime;
            protected string m_arrivalTicks;
            protected string m_c2sStr;
            protected string m_s2cStr;
            protected string m_dataLength;

            /// <summary>
            /// Initializes a new instance of
            /// <see cref="Kamilla.Network.Protocols.DefaultProtocol.BaseItemData"/> class.
            /// </summary>
            /// <param name="item">
            /// The underlying instance of <see cref="Kamilla.Network.Viewing.ViewerItem"/> class.
            /// </param>
            /// <exception cref="System.ArgumentNullException">
            /// <c>item</c> is null.
            /// </exception>
            public BaseItemData(ViewerItem item)
            {
                if (item == null)
                    throw new ArgumentNullException("item");

                m_item = item;
            }

            /// <summary>
            /// Fires the
            /// <see cref="Kamilla.Network.Protocols.DefaultProtocol.BaseItemData.PropertyChanged"/>
            /// event.
            /// </summary>
            /// <param name="propertyName">
            /// Name of the property that was changed.
            /// </param>
            /// <exception cref="System.ArgumentNullException">
            /// <c>propertyName</c> is null.
            /// </exception>
            protected void Changed(string propertyName)
            {
                if (propertyName == null)
                    throw new ArgumentNullException("propertyName");

                if (this.PropertyChanged != null)
                    this.PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }

            public virtual string Index
            {
                get { return m_index ?? (m_index = m_item.Index.ToString()); }
            }
            public virtual string ArrivalTime
            {
                get { return m_arrivalTime ?? (m_arrivalTime = m_item.Packet.ArrivalTime.ToString("HH:mm:ss")); }
            }
            public virtual string ArrivalTicks
            {
                get
                {
                    if (m_arrivalTicks == null)
                    {
                        var log = m_item.Log as IHasStartTicks;
                        if (log != null)
                            m_arrivalTicks = ((int)(m_item.Packet.ArrivalTicks - log.StartTicks)).ToString();
                        else
                            m_arrivalTicks = m_item.Packet.ArrivalTicks.ToString();
                    }

                    return m_arrivalTicks;
                }
            }
            public virtual string C2sStr
            {
                get
                {
                    if (m_c2sStr == null)
                    {
                        var packet = m_item.Packet;
                        if (packet.Direction == TransferDirection.ToServer)
                            m_c2sStr = NetworkStrings.bytes.LocalizedFormat(packet.Data.Length);
                        else
                            m_c2sStr = string.Empty;
                    }

                    return m_c2sStr;
                }
            }
            public virtual string S2cStr
            {
                get
                {
                    if (m_s2cStr == null)
                    {
                        var packet = m_item.Packet;
                        if (packet.Direction == TransferDirection.ToClient)
                            m_s2cStr = NetworkStrings.bytes.LocalizedFormat(packet.Data.Length);
                        else
                            m_s2cStr = string.Empty;
                    }

                    return m_s2cStr;
                }
            }
            public virtual string DataLength
            {
                get { return m_dataLength ?? (m_dataLength = m_item.Packet.Data.Length.ToString()); }
            }
        }

        sealed class ItemData : BaseItemData
        {
            internal ItemData(ViewerItem item)
                : base(item)
            {
            }
        }

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
            e.Item.Data = new ItemData(e.Item);
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
            ".Data.Index",
            ".Data.ArrivalTime",
            ".Data.ArrivalTicks",
            ".Data.C2sStr",
            ".Data.S2cStr",
            ".Data.DataLength",
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

                var dataTemplate = new DataTemplate();
                dataTemplate.DataType = typeof(ItemData);

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

            m_viewer.ItemQueried -= m_itemQueriedHandler;
            m_viewer = null;
        }

        protected override PacketParser InternalCreateParser(ViewerItem item)
        {
            return null;
        }
    }
}
