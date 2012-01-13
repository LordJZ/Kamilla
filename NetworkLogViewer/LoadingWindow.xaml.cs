using System;
using System.Collections.Generic;
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

namespace NetworkLogViewer
{
    partial class LoadingWindow : Window
    {
        Action m_onCancel;

        public LoadingWindow()
        {
            InitializeComponent();
        }

        public void SetLoadingState(LoadingState state)
        {
            ui_btnCancel.IsEnabled = state.OnCancel != null;
            m_onCancel = state.OnCancel;
            ui_pbMain.Value = state.Progress * 100;
            ui_tbJobDescription.Text = state.Description;
        }

        public void SetProgress(float progress)
        {
            ui_pbMain.Value = progress * 100;
        }
    }
}
