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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Kamilla.WPF
{
    public partial class MessageWindow : Window
    {
        MessageWindow()
        {
            InitializeComponent();

            this.CommandBindings.AddRange(new[]
            {
                new CommandBinding(ApplicationCommands.Close, ApplicationCommand_CloseExecuted),
            });

            this.InputBindings.AddRange(new[]
            {
                new KeyBinding(ApplicationCommands.Close, Key.Escape, ModifierKeys.None),
            });
        }

        public static void Show(Window parent, string title, string text)
        {
            var wnd = new MessageWindow();
            wnd.Owner = parent;
            wnd.Icon = wnd.Icon;
            wnd.Style = parent.Style;
            wnd.Title = title;
            wnd.ui_tbMain.Text = text;
            wnd.ShowDialog();
            wnd.Close();
        }

        void ApplicationCommand_CloseExecuted(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
