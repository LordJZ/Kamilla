using System;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using Kamilla;
using Kamilla.Network.Protocols;
using Kamilla.Network.Viewing;
using Kamilla.WPF;

namespace NetworkLogViewer.ViewTabs
{
    /// <summary>
    /// Interaction logic for TextContents.xaml
    /// </summary>
    public partial class TextContents : UserControl, IViewTab
    {
        public TextContents()
        {
            InitializeComponent();
        }

        string IViewTab.Header { get { return Strings.View_TextContents; } }

        public bool IsFilled { get; set; }

        ValueTuple<object, string>[] m_strings;
        ValueTuple<object, byte[]>[] m_binaryDatas;

        void IViewTab.Reset()
        {
            m_strings = null;
            m_binaryDatas = null;
            ui_tbMain.Text = string.Empty;
            ui_cbDatas.IsEnabled = false;
            ui_cbDatas.Items.Clear();
            ui_cbDatas.Items.Add(Strings.View_NoTextData);
            ui_cbDatas.SelectedIndex = 0;
            ui_btn.IsEnabled = false;

            this.IsFilled = false;
        }

        void SelectData(int index)
        {
            ui_btn.IsEnabled = true;

            if (index >= m_strings.Length)
            {
                ui_btn.Content = Strings.View_Redecode;

                var tuple = m_binaryDatas[index - m_strings.Length];

                this.Redecode(tuple.Item2);
            }
            else
            {
                ui_btn.Content = Strings.Menu_SaveEllipsis;

                var tuple = m_strings[index];

                ui_tbMain.Text = tuple.Item2;
            }
        }

        void IViewTab.Fill(Protocol protocol, ViewerItem item)
        {
            ((IViewTab)this).Reset();

            m_strings = ParsingHelper.ExtractStrings(protocol, item);
            m_binaryDatas = ParsingHelper.ExtractBinaryDatas(protocol, item);
            int nStrings = m_strings.Length;
            int count = nStrings + m_binaryDatas.Length;

            if (count > 0)
            {
                ui_cbDatas.Items.Clear();

                for (int i = 0; i < count; i++)
                {
                    var cbItem = new ComboBoxItem();

                    var obj = i >= nStrings ? m_binaryDatas[i - nStrings].Item1 : m_strings[i].Item1;
                    cbItem.Content = ParsingHelper.GetContentName(obj, i);

                    ui_cbDatas.Items.Add(cbItem);
                }

                ui_cbDatas.SelectedIndex = 0;

                if (count > 1)
                    ui_cbDatas.IsEnabled = true;

                //this.SelectData(0);
            }

            this.IsFilled = true;
        }

        private void ui_cbDatas_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            int index = ui_cbDatas.SelectedIndex;

            if (index >= 0 &&
                (m_strings != null && m_strings.Length > 0
                || m_binaryDatas != null && m_binaryDatas.Length > 0))
                this.SelectData(index);

            e.Handled = true;
        }

        void Redecode(byte[] data)
        {
            Encoding encoding = null;
            try
            {
                encoding = Encoding.GetEncoding(ui_cbEncodings.Text);
            }
            catch
            {
                var text = Strings.EncodingNotFound.LocalizedFormat(ui_cbEncodings.Text);
                ui_tbMain.Text = text;
                MessageWindow.Show(Window.GetWindow(this), Strings.Error, text);
                return;
            }

            try
            {
                ui_tbMain.Text = encoding.GetString(data);
            }
            catch (Exception e)
            {
                ui_tbMain.Text = Strings.View_ErrorDecoding
                    .LocalizedFormat(data.Length, ui_cbEncodings.Text, e.ToString());
            }
        }

        private void ui_btn_Click(object sender, RoutedEventArgs e)
        {
            e.Handled = true;

            int index = ui_cbDatas.SelectedIndex;

            if (index >= m_strings.Length)
            {
                var tuple = m_binaryDatas[index - m_strings.Length];

                this.Redecode(tuple.Item2);
            }
            else
            {
                // TODO: save text here
            }
        }
    }
}
