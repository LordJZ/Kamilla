using System;
using System.Windows;

namespace NetworkLogViewer
{
    partial class LoadingWindow : Window
    {
        MainWindow m_window;
        Action<MainWindow> m_onCancel;

        public LoadingWindow(MainWindow window)
        {
            InitializeComponent();

            this.Owner = window;
            this.Style = window.Style;
            window.Implementation.StyleChanged += (o, e) => this.Style = this.Owner.Style;
            m_window = window;
        }

        public void SetLoadingState(LoadingState state)
        {
            ui_btnCancel.IsEnabled = state.OnCancel != null;
            m_onCancel = state.OnCancel;
            ui_pbMain.Value = 0.0;
            ui_tbJobDescription.Text = state.Description;
        }

        public void SetProgress(int progress)
        {
            ui_pbMain.Value = progress;
        }

        private void ui_btnCancel_Click(object sender, RoutedEventArgs e)
        {
            m_onCancel(m_window);
        }
    }
}
