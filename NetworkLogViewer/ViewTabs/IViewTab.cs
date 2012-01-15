using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Controls;
using Kamilla.Network.Viewing;
using Kamilla.Network.Protocols;

namespace NetworkLogViewer.ViewTabs
{
    // Can't mark this as abstract because it breaks the designer
    internal interface IViewTab
    {
        string Header { get; }
        bool IsFilled { get; }

        void Reset();
        void Fill(Protocol protocol, ViewerItem item);
    }
}
