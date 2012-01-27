using Kamilla.Network.Protocols;
using Kamilla.Network.Viewing;

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
