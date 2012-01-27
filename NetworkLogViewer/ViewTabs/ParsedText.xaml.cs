using System.Windows.Controls;
using Kamilla.Network.Protocols;
using Kamilla.Network.Viewing;

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
