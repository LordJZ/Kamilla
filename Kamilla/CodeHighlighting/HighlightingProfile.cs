using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Kamilla.CodeHighlighting
{
    public class HighlightingProfile
    {
        public string OneLineCommentsStartWith;

        public string MultilineCommentsStartWith;
        public string MultilineCommentsEndWith;

        public string[] Keywords;
        public bool KeywordsCaseSensitive;
    }
}
