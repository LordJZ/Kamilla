using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Kamilla.Network.Logging.Wow
{
    public interface IHasPktSnifferId
    {
        PktSnifferId SnifferId { get; set; }
    }
}
