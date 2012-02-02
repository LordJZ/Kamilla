using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Kamilla.CodeHighlighting
{
    public static class SimpleCodeHighlighter
    {
        public static HighlightedItem[] Highlight(string text, HighlightingProfile profile)
        {
            var oneLineCommentsStartWith = profile.OneLineCommentsStartWith;
            var oneLineCommentsStartWithLength = oneLineCommentsStartWith.Length;

            var result = new List<HighlightedItem>(64);

            var len = text.Length;
            for (int i = 0; i < len; i++)
            {
                if (char.IsLetter(text[i]))
                {
                    // Read identifier
                    var builder = new StringBuilder(text[i] + "", 32);
                    int j = i + 1;
                    while (j < len && char.IsLetterOrDigit(text[j]))
                    {
                        builder.Append(text[j]);
                        ++j;
                    }
                    var ident = builder.ToString();
                    if (profile.Keywords.Contains(ident, !profile.KeywordsCaseSensitive
                        ? StringComparer.InvariantCultureIgnoreCase : StringComparer.InvariantCulture))
                    {
                        result.Add(new HighlightedItem(i, j, CodeTokens.Keyword));
                        i = j;
                    }
                }
                else if (char.IsDigit(text[i]))
                {
                    // Read numeric
                    int begin = i;
                    ++i;
                    while (i < len && (char.IsDigit(text[i]) || text[i] == '.'))
                        ++i;

                    result.Add(new HighlightedItem(begin, i, CodeTokens.Number));
                }
                else if (text[i] == '"')
                {
                    int begin = i;
                    ++i;
                    while (i < len && (text[i] != '"' || text[i - 1] == '\\'))
                        ++i;
                    ++i;

                    result.Add(new HighlightedItem(begin, i, CodeTokens.String));
                }
                else if (text[i] == '\'')
                {
                    int begin = i;
                    ++i;
                    while (i < len && (text[i] != '\'' || text[i - 1] == '\\'))
                        ++i;
                    ++i;

                    result.Add(new HighlightedItem(begin, i, CodeTokens.String));
                }
                else if (oneLineCommentsStartWithLength > 0 && i >= oneLineCommentsStartWithLength
                    && text.SubstringEquals(i - oneLineCommentsStartWithLength, oneLineCommentsStartWith))
                {
                    int end = text.IndexOf('\n', i);
                    if (text[i - 1] == '\r')
                        --end;

                    result.Add(new HighlightedItem(i - oneLineCommentsStartWithLength, end, CodeTokens.OneLineComment));
                    i = end;
                }
            }

            return result.ToArray();
        }
    }
}
