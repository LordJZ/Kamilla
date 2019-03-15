﻿using System;
using System.IO;
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
    /// Interaction logic for BinaryContents.xaml
    /// </summary>
    public partial class BinaryContents : UserControl, IViewTab
    {
        public BinaryContents()
        {
            InitializeComponent();
        }

        string IViewTab.Header { get { return Strings.View_BinaryContents; } }

        public bool IsFilled { get; set; }

        ValueTuple<object, byte[]>[] m_datas;

        void IViewTab.Reset()
        {
            m_datas = null;
            ui_tbMain.Text = string.Empty;
            ui_cbDatas.IsEnabled = false;
            ui_cbDatas.Items.Clear();
            ui_cbDatas.Items.Add(Strings.View_NoBinaryData);
            ui_cbDatas.SelectedIndex = 0;
            ui_btnSave.IsEnabled = false;

            this.IsFilled = false;
        }

        void SelectData(int index)
        {
            ui_tbMain.Text = m_datas[index].Item2.ToHexDump();

            var obj = m_datas[index].Item1;
            if (obj is byte[])
                ui_btnSave.IsEnabled = true;
            else
                ui_btnSave.IsEnabled = false;
        }

        void IViewTab.Fill(Protocol protocol, ViewerItem item)
        {
            ((IViewTab)this).Reset();

            m_datas = ParsingHelper.ExtractBinaryDatas(protocol, item);
            int count = m_datas.Length;

            if (count > 0)
            {
                ui_cbDatas.Items.Clear();

                for (int i = 0; i < count; i++)
                    ui_cbDatas.Items.Add(ParsingHelper.GetContentName(m_datas[i].Item1, i));

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

            if (index >= 0 && m_datas != null && m_datas.Length > 0)
                this.SelectData(index);

            e.Handled = true;
        }

        private void ui_btnSave_Click(object sender, RoutedEventArgs e)
        {
            e.Handled = true;

            var dialog = new SaveFileDialog();
            dialog.AddExtension = true;
            dialog.Filter = Strings.BinaryFiles + " (*.bin)|*.bin|" + NetworkStrings.AllFiles + " (*.*)|*.*";
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
                File.WriteAllBytes(filename, m_datas[ui_cbDatas.SelectedIndex].Item2);
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
