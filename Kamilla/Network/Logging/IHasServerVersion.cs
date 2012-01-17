using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Kamilla.Network.Logging
{
    public interface IHasServerVersion
    {
        Version ServerVersion { get; set; }
    }
}
