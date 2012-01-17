using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Kamilla.Network.Logging.Wow
{
    public enum PktSnifferId : byte
    {
        WadSniffer = 0,
        NomadSniffer = 1,
        WowCoreSniffer = 2,
        MaNGOS__TOM_RUS = 3,
        User456Sniffer = 4,
        DelfinSniffer = 5,
        BurlexSniffer = 6,
        WCellSniffer = 7,
        KoboldSniffer = 8,
        abdula123Sniffer = 9,
        Kamilla = 10,
        YohaSniffer = 11,
        UnknownSniffer = byte.MaxValue,
    }
}
