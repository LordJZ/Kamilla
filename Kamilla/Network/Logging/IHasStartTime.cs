using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Kamilla.Network.Logging
{
    public interface IHasStartTime
    {
        DateTime StartTime { get; set; }
    }
}
