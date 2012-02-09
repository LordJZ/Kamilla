using System.Windows;

namespace Kamilla.WPF
{
    public partial class MessageWindow : Window
    {
        MessageWindow()
        {
            InitializeComponent();
        }

        public static void Show(Window parent, string title, string text)
        {
            var wnd = new MessageWindow();
            wnd.WindowStyle = WindowStyle.None;
            wnd.AllowsTransparency = true;
            WindowBehavior.SetHasTitleBarMaximizeButton(wnd, false);
            WindowBehavior.SetHasTitleBarMinimizeButton(wnd, false);
            wnd.Owner = parent;
            wnd.Icon = wnd.Icon;
            wnd.Style = parent.Style;
            wnd.Title = title;
            wnd.ui_tbMain.Text = text;
            wnd.ShowDialog();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
