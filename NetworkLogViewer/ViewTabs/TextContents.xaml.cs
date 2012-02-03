using System;
using System.IO;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using Kamilla;
using Kamilla.Network;
using Kamilla.Network.Protocols;
using Kamilla.Network.Viewing;
using Kamilla.WPF;
using Microsoft.Win32;

namespace NetworkLogViewer.ViewTabs
{
    /// <summary>
    /// Interaction logic for TextContents.xaml
    /// </summary>
    public partial class TextContents : UserControl, IViewTab
    {
        string SavedEncoding
        {
            get { return Configuration.GetValue("Encoding", "UTF-8"); }
            set { Configuration.SetValue("Encoding", value); }
        }

        public TextContents()
        {
            InitializeComponent();

            ui_cbEncodings.Text = this.SavedEncoding;
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
                    var obj = i >= nStrings ? m_binaryDatas[i - nStrings].Item1 : m_strings[i].Item1;
                    ui_cbDatas.Items.Add(ParsingHelper.GetContentName(obj, i));
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

            this.SavedEncoding = encoding.WebName;

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
                Encoding encoding;
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

                this.SavedEncoding = encoding.WebName;

                byte[] data;
                try
                {
                    data = encoding.GetBytes(m_strings[index].Item2);
                }
                catch
                {
                    MessageWindow.Show(Window.GetWindow(this), Strings.Error, Strings.View_ErrorEncoding);
                    return;
                }

                var dialog = new SaveFileDialog();
                dialog.AddExtension = true;
                dialog.Filter = Strings.TextFiles + " (*.txt)|*.txt|" + NetworkStrings.AllFiles + " (*.*)|*.*";
                dialog.FilterIndex = 0;
                try
                {
                    var file = MainWindow.SaveFileName;
                    dialog.FileName = Path.GetFileName(file);
                    dialog.InitialDirectory = Path.GetDirectoryName(file);
                }
                catch
                {
                }

                if (dialog.ShowDialog(Window.GetWindow(this)) != true)
                    return;

                var filename = dialog.FileName;

                try
                {
                    File.WriteAllBytes(filename, data);
                }
                catch
                {
                    MessageWindow.Show(Window.GetWindow(this), Strings.Error, Strings.View_FailedToSaveIntoFile);
                    return;
                }

                MainWindow.SaveFileName = filename;
            }
        }
    }
}
