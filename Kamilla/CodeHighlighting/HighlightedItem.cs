using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Kamilla.CodeHighlighting
{
    public struct HighlightedItem
    {
        public HighlightedItem(int start, int end, CodeTokens token)
        {
            this.Start = start;
            this.End = end;
            this.Token = token;
        }

        public int Start;
        public int End;
        public CodeTokens Token;
    }
}
