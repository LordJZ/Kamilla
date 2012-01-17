using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Globalization;

namespace Kamilla.Network.Logging
{
    public interface IHasCultureInfo
    {
        CultureInfo Culture { get; set; }
    }
}
