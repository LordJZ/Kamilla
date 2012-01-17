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
using Kamilla.Network.Protocols;
using Kamilla.Network.Viewing;
using Kamilla;

namespace NetworkLogViewer.ViewTabs
{
    partial class ParsedText : UserControl, IViewTab
    {
        public ParsedText()
        {
            InitializeComponent();
        }

        string IViewTab.Header { get { return Strings.View_ParsedText; } }

        public bool IsFilled { get; set; }

        void IViewTab.Reset()
        {
            this.IsFilled = false;
        }

        void IViewTab.Fill(Protocol protocol, ViewerItem item)
        {
            var parser = item.Parser;
            if (parser == null)
            {
                protocol.CreateParser(item);
                parser = item.Parser;
            }

            if (!parser.IsParsed)
                parser.Parse();

            ui_tbMain.Text = parser.ParsedText ?? string.Empty;
            this.IsFilled = true;
        }
    }
}
