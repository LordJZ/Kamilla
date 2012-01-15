using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NetworkLogViewer
{
    internal class LoadingState
    {
        public LoadingState(string desc)
        {
            this.Description = desc;
            this.OnCancel = null;
        }

        public LoadingState(string desc, Action onCancel)
        {
            this.Description = desc;
            this.OnCancel = onCancel;
        }

        public readonly string Description;
        public readonly Action OnCancel;
    }
}
