using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using Kamilla;

namespace NetworkLogViewer
{
    /// <summary>
    /// Interaction logic for GoToPacketWindow.xaml
    /// </summary>
    partial class GoToPacketWindow : Window
    {
        int m_count;
        internal int Index { get; private set; }

        public GoToPacketWindow(MainWindow window)
        {
            this.Owner = window;
            InitializeComponent();

            this.Style = window.Style;
            m_count = window.Implementation.CurrentLog.Count;

            ui_lblPacketNumber.Content = Strings.GoTo_PacketNumber.LocalizedFormat(0, m_count - 1);

            //var item = window.SelectedItem;
            //if (item == null)
            //    ui_tbValue.Text = "0";
            //else
            //    ui_tbValue.Text = window.SelectedItem.Index.ToString();
        }

        private void ui_btnOK_Click(object sender, RoutedEventArgs e)
        {
            this.Index = int.Parse(ui_tbValue.Text);
            this.DialogResult = true;
        }

        private void ui_btnCancel_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
        }

        private void ui_tbValue_TextChanged(object sender, TextChangedEventArgs e)
        {
            uint result;
            ui_btnOK.IsEnabled = uint.TryParse(ui_tbValue.Text, out result) && result < m_count;
        }
    }
}
