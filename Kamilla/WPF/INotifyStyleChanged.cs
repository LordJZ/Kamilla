using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Kamilla.WPF
{
    public interface INotifyStyleChanged
    {
        event EventHandler StyleChanged;
    }
}
