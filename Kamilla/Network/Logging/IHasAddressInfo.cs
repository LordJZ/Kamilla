using System.Net;

namespace Kamilla.Network.Logging
{
    public interface IHasAddressInfo
    {
        IPAddress ClientAddress { get; set; }
        IPAddress ServerAddress { get; set; }
        int ClientPort { get; }
        int ServerPort { get; }
    }
}
