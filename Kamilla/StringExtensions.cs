using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;

namespace Kamilla
{
    /// <summary>
    /// Contains string extension methods.
    /// </summary>
    public static class StringExtensions
    {
        /// <summary>
        /// Escapes a character with a backslash.
        /// </summary>
        /// <param name="input">The input string.</param>
        /// <param name="symbol">The character to escape.</param>
        /// <returns>The input string with the character escaped.</returns>
        /// <exception cref="System.ArgumentNullException">
        /// input is null.
        /// </exception>
        public static string Escape(this string input, char symbol)
        {
            if (input == null)
                throw new ArgumentNullException();

            return input.Replace(string.Empty + symbol, "\\" + symbol);
        }

        /// <summary>
        /// Escapes single and double quotes in the input string.
        /// </summary>
        /// <param name="input">The input string.</param>
        /// <returns>The input string with escaped quotes.</returns>
        /// <exception cref="System.ArgumentNullException">
        /// input is null.
        /// </exception>
        public static string EscapeQuotes(this string input)
        {
            return input.Escape('\'').Escape('"');
        }

        /// <summary>
        /// Escapes special mysql characters: % and _
        /// </summary>
        /// <param name="input">The input string.</param>
        /// <returns>The input string escapes characters.</returns>
        /// <exception cref="System.ArgumentNullException">
        /// input is null.
        /// </exception>
        public static string EscapeMysqlSpecial(this string input)
        {
            return input.Escape('%').Escape('_');
        }

        /// <summary>
        /// Escapes all characters that may interfere with a query: " % _ '
        /// </summary>
        /// <param name="input">The input string.</param>
        /// <returns>The input string with escaped characters.</returns>
        /// <exception cref="System.ArgumentNullException">
        /// input is null.
        /// </exception>
        public static string EscapeMysql(this string input)
        {
            return input.EscapeQuotes().EscapeMysqlSpecial();
        }

        /// <summary>
        /// Parses the byte array represented by a string.
        /// </summary>
        /// <param name="input">The input string that contains a byte array.</param>
        /// <returns>The parsed byte array.</returns>
        /// <exception cref="System.ArgumentNullException">
        /// input is null.
        /// </exception>
        /// <exception cref="System.ArgumentException">
        /// input is invalid string.
        /// </exception>
        public static byte[] AsByteArray(this string input)
        {
            return AsByteArrayNoSpaces(input);
        }

        /// <summary>
        /// Parses the byte array represented by a string.
        /// </summary>
        /// <param name="input">The input string that contains a byte array.</param>
        /// <param name="allowWhiteSpace">
        /// Indicates whether whitespace characters should be skipped or not.
        /// </param>
        /// <returns>The parsed byte array.</returns>
        /// <exception cref="System.ArgumentNullException">
        /// input is null.
        /// </exception>
        /// <exception cref="System.ArgumentException">
        /// input is invalid string.
        /// </exception>
        public static byte[] AsByteArray(this string input, bool allowWhiteSpace)
        {
            if (allowWhiteSpace)
                return AsByteArrayWithSpaces(input);
            else
                return AsByteArrayNoSpaces(input);
        }

        static byte[] AsByteArrayNoSpaces(string input)
        {
            if (input == null)
                throw new ArgumentNullException();

            var len = input.Length;
            if ((len & 1) != 0)
                throw new ArgumentException();

            var bytes = new List<byte>(len / 2);

            for (int i = 0; i < len; ++i)
            {
                if ((i & 1) == 0)
                    bytes.Add(0);

                int lastByte = bytes.Count - 1;
                bytes[lastByte] *= 0x10;

                var ch = input[i];
                if (ch >= '0' && ch <= '9')
                    bytes[lastByte] += (byte)(ch - '0');
                else if (ch >= 'a' & ch <= 'f')
                    bytes[lastByte] += (byte)(ch - 'a' + 10);
                else if (ch >= 'A' & ch <= 'F')
                    bytes[lastByte] += (byte)(ch - 'A' + 10);
                else
                    throw new ArgumentException();
            }

            return bytes.ToArray();
        }

        static byte[] AsByteArrayWithSpaces(string input)
        {
            if (input == null)
                throw new ArgumentNullException();

            var len = input.Length;
            var bytes = new List<byte>(len / 2);
            int nDigits = 0;

            for (int i = 0; i < len; ++i)
            {
                var ch = input[i];
                if (char.IsWhiteSpace(ch))
                {
                    nDigits = 0;
                }
                else if (ch >= '0' && ch <= '9')
                {
                    if (nDigits == 2)
                        throw new ArgumentException();
                    else if (nDigits == 0)
                        bytes.Add(0);

                    int lastByte = bytes.Count - 1;
                    bytes[lastByte] *= 0x10;
                    bytes[lastByte] += (byte)(ch - '0');
                    ++nDigits;
                }
                else if (ch >= 'a' & ch <= 'f')
                {
                    if (nDigits == 2)
                        throw new ArgumentException();
                    else if (nDigits == 0)
                        bytes.Add(0);

                    int lastByte = bytes.Count - 1;
                    bytes[lastByte] *= 0x10;
                    bytes[lastByte] += (byte)(ch - 'a' + 10);
                    ++nDigits;
                }
                else if (ch >= 'A' & ch <= 'F')
                {
                    if (nDigits == 2)
                        throw new ArgumentException();
                    else if (nDigits == 0)
                        bytes.Add(0);

                    int lastByte = bytes.Count - 1;
                    bytes[lastByte] *= 0x10;
                    bytes[lastByte] += (byte)(ch - 'A' + 10);
                    ++nDigits;
                }
                else
                    throw new ArgumentException();
            }

            return bytes.ToArray();
        }

        /// <summary>
        /// Urlizes a string, removing all characters except letters, numbers, underscore and minus.
        /// </summary>
        /// <param name="input">The input string to be urlized.</param>
        /// <returns>The urlized string.</returns>
        public static string Urlize(this string input)
        {
            input = input
                .Replace(" / ", "-")
                .Replace("'", "")
                .Trim();

            input = Regex.Replace(input, @"[^\w\d_]", "-");

            input = input
                .Trim('-')
                .Replace("--", "-")
                .Replace("--", "-");

            return input.ToLower();
        }

        /// <summary>
        /// Determines whether the first character of this
        /// string instance matches the specified character.
        /// </summary>
        /// <param name="input">The original string.</param>
        /// <param name="value">The character to compare.</param>
        /// <returns>true if value matches the beginning of this string; otherwise, false.</returns>
        /// <exception cref="System.ArgumentNullException">value is null.</exception>
        public static bool StartsWith(this string input, char value)
        {
            return input.StartsWith(String.Empty + value);
        }

        /// <summary>
        /// Determines whether the first character of this string instance matches
        /// the specified character when compared using the specified culture.
        /// </summary>
        /// <param name="input">The original string.</param>
        /// <param name="value">The character to compare.</param>
        /// <param name="ignoreCase">true to ignore case during the comparison; otherwise, false.</param>
        /// <param name="culture">
        /// Cultural information that determines how this string and value are compared.
        /// If culture is null, the current culture is used.
        /// </param>
        /// <returns>true if value matches the beginning of this string; otherwise, false.</returns>
        /// <exception cref="System.ArgumentNullException">value is null.</exception>
        public static bool StartsWith(this string input, char value, bool ignoreCase, CultureInfo culture)
        {
            return input.StartsWith(String.Empty + value, ignoreCase, culture);
        }

        /// <summary>
        /// Determines whether the first character of this string instance matches
        /// the specified character when compared using the specified comparison option.
        /// </summary>
        /// <param name="input">The original string.</param>
        /// <param name="value">The character to compare.</param>
        /// <param name="comparisonType">
        /// One of the enumeration values that determines
        /// how this string and value are compared.
        /// </param>
        /// <returns>true if value matches the beginning of this string; otherwise, false.</returns>
        /// <exception cref="System.ArgumentNullException">value is null.</exception>
        public static bool StartsWith(this string input, char value, StringComparison comparisonType)
        {
            return input.StartsWith(String.Empty + value, comparisonType);
        }

        /// <summary>
        /// Determines whether the last character of this
        /// string instance matches the specified character.
        /// </summary>
        /// <param name="input">The original string.</param>
        /// <param name="value">The character to compare.</param>
        /// <returns>true if value matches the end of this string; otherwise, false.</returns>
        /// <exception cref="System.ArgumentNullException">value is null.</exception>
        public static bool EndsWith(this string input, char value)
        {
            return input.EndsWith(String.Empty + value);
        }

        /// <summary>
        /// Determines whether the last character of this string instance matches
        /// the specified character when compared using the specified culture.
        /// </summary>
        /// <param name="input">The original string.</param>
        /// <param name="value">The character to compare.</param>
        /// <param name="ignoreCase">true to ignore case during the comparison; otherwise, false.</param>
        /// <param name="culture">
        /// Cultural information that determines how this string and value are compared.
        /// If culture is null, the current culture is used.
        /// </param>
        /// <returns>true if value matches the end of this string; otherwise, false.</returns>
        /// <exception cref="System.ArgumentNullException">value is null.</exception>
        public static bool EndsWith(this string input, char value, bool ignoreCase, CultureInfo culture)
        {
            return input.EndsWith(String.Empty + value, ignoreCase, culture);
        }

        /// <summary>
        /// Determines whether the last character of this string instance matches
        /// the specified character when compared using the specified comparison option.
        /// </summary>
        /// <param name="input">The original string.</param>
        /// <param name="value">The character to compare.</param>
        /// <param name="comparisonType">
        /// One of the enumeration values that determines
        /// how this string and value are compared.
        /// </param>
        /// <returns>true if value matches the end of this string; otherwise, false.</returns>
        /// <exception cref="System.ArgumentNullException">value is null.</exception>
        public static bool EndsWith(this string input, char value, StringComparison comparisonType)
        {
            return input.EndsWith(String.Empty + value, comparisonType);
        }

        public static StringBuilder AppendFormatLine(this StringBuilder sb, string format, params object[] args)
        {
            return sb.AppendFormat(format, args).AppendLine();
        }

        public static StringBuilder AppendFormatLine(this StringBuilder sb, IFormatProvider provider,
            string format, params object[] args)
        {
            return sb.AppendFormat(provider, format, args).AppendLine();
        }

        public static StringBuilder AppendLine(this StringBuilder sb, string text)
        {
            return sb.Append(text).AppendLine();
        }

        public static string ToHexString(this byte[] byteArray)
        {
            return ToHexString(byteArray, true);
        }

        public static string ToHexString(this byte[] byteArray, bool insertSpaces)
        {
            var builder = new StringBuilder(byteArray.Length * (insertSpaces ? 3 : 2));
            byteArray.ToHexString(builder, insertSpaces);
            return builder.ToString();
        }

        public static void ToHexString(this byte[] byteArray, StringBuilder builder)
        {
            ToHexString(byteArray, builder, true);
        }

        public static void ToHexString(this byte[] byteArray, StringBuilder builder, bool insertSpaces)
        {
            for (int i = 0; i < byteArray.Length; i++)
            {
                builder.Append(byteArray[i].ToString("X2"));

                if (insertSpaces)
                    builder.Append(' ');
            }
        }

        public static string ToHexDump(this byte[] byteArray)
        {
            int length = byteArray.Length;
            if (length == 0)
                return string.Empty;

            StringBuilder output = new StringBuilder(80 * ((length + 15) / 16 + 4));
            byteArray.ToHexDump(output, false);
            return output.ToString();
        }

        public static string ToHexDump(this byte[] byteArray, bool detectRussian)
        {
            int length = byteArray.Length;
            if (length == 0)
                return string.Empty;

            StringBuilder output = new StringBuilder(80 * ((length + 15) / 16 + 4));
            byteArray.ToHexDump(output, detectRussian);
            return output.ToString();
        }

        public static void ToHexDump(this byte[] byteArray, StringBuilder output)
        {
            ToHexDump(byteArray, output, false);
        }

        public static void ToHexDump(this byte[] byteArray, StringBuilder output, bool detectRussian)
        {
            int length = byteArray.Length;
            if (length == 0)
                return;

            output.AppendLine("|---------------------------------------------------------------------------|");
            output.AppendLine("| OFFSET |  0  1  2  3  4  5  6  7  8  9  A  B  C  D  E  F |0123456789ABCDEF|");
            output.AppendLine("|----------------------------------------------------------|----------------|");

            int byteIndex = 0;
            int iterator = -1;

            int lines = (length + 15) / 16;

            var chars = new StringBuilder(16);

            bool noData = false;

            for (int i = 0; i < lines; ++i, byteIndex += 16)
            {
                output
                    .Append('|')
                    .Append(byteIndex.ToString("X8"))
                    .Append("| ");

                for (int j = 0; j < 16; ++j)
                {
                    ++iterator;
                    if (!noData && byteArray.Length <= iterator)
                        noData = true;

                    if (noData)
                    {
                        output.Append("-- ");
                        chars.Append('.');
                    }
                    else
                    {
                        byte c = byteArray[iterator];
                        output
                            .Append(c.ToString("X2"))
                            .Append(' ');

                        // UTF-8
                        if (detectRussian && (c == 0xD0 || c == 0xD1) && iterator < byteArray.Length - 1)
                        {
                            byte peek = byteArray[iterator + 1];

                            if (peek >= 0x80 && peek <= 0xBF)
                            {
                                if (c == 0xD0)
                                    chars.Append((char)(0x400 | (peek - 0x80)));
                                else
                                    chars.Append((char)(0x400 | (peek - 0x40)));

                                continue;
                            }
                        }

                        chars.Append(c >= 0x20 && c < 0x7F ? (char)c : '.');
                    }
                }

                output
                    .Append('|')
                    .Append(chars.ToString())
                    .Append('|')
                    .AppendLine();

                chars.Clear();
            }

            output.AppendLine("|----------------------------------------------------------|----------------|");
        }

        public static string PadMultiline(this string input, string with)
        {
            // Speedup
            if (input.IndexOf(Environment.NewLine) == -1)
                return input;

            string[] lines = input.Split(new string[] { Environment.NewLine }, StringSplitOptions.None);
            return String.Join(Environment.NewLine + with, lines);
        }

        public static string PadMultiline(this string input, int padderLength)
        {
            return input.PadMultiline(string.Empty.PadRight(padderLength));
        }

        public static string Repeat(this char chatToRepeat, int repeat)
        {
            if (repeat < 0)
                throw new ArgumentOutOfRangeException("repeat");

            return new string(chatToRepeat, repeat);
        }

        public static string Repeat(this string stringToRepeat, int repeat)
        {
            if (stringToRepeat == null)
                throw new ArgumentNullException("stringToRepeat");

            if (repeat < 0)
                throw new ArgumentOutOfRangeException("repeat");

            var builder = new StringBuilder(repeat * stringToRepeat.Length);

            for (int i = 0; i < repeat; i++)
                builder.Append(stringToRepeat);

            return builder.ToString();
        }

        public static string FixNewLines(this string tofix)
        {
            var builder = new StringBuilder((int)(tofix.Length * 1.1));

            char lastChar = '\0';
            for (int i = 0; i < tofix.Length; ++i)
            {
                switch (tofix[i])
                {
                    case '\n':
                        break;
                    case '\r':
                        builder.Append(Environment.NewLine);
                        break;
                    default:
                        if (lastChar == '\n')
                            builder.Append(Environment.NewLine);

                        builder.Append(tofix[i]);
                        break;
                }

                lastChar = tofix[i];
            }

            return builder.ToString();
        }

        public static string TrimWhiteSpace(this string str)
        {
            return TrimWhiteSpace(str, str.Length);
        }

        public static string TrimWhiteSpace(this string str, int maxChars)
        {
            return TrimCharacters(str, maxChars, c => char.IsWhiteSpace(c));
        }

        public static string TrimCharacters(this string str, Predicate<char> charsToTrimSelector)
        {
            return TrimCharacters(str, str.Length, charsToTrimSelector);
        }

        public static string TrimCharacters(this string str, int maxChars, Predicate<char> charsToTrimSelector)
        {
            int len = Math.Min(str.Length, maxChars);
            var result = new StringBuilder(len);

            bool lastCharFits = true;

            foreach (var c in str)
            {
                if (!(result.Length < len))
                    break;

                bool fits = charsToTrimSelector(c);
                if (lastCharFits && fits)
                    continue;

                result.Append(c);
            }

            return result.ToString();
        }

        public static string LocalizedFormat(this string format, params object[] args)
        {
            return LocalizedFormat(null, format, args);
        }

        public static string LocalizedFormat(this string format, IFormatProvider provider, params object[] args)
        {
            return LocalizedFormat(provider, format, args);
        }

        public static string LocalizedFormat(IFormatProvider provider, string format, params object[] args)
        {
            var builder = new StringBuilder(format.Length + args.Length * 8);

            int length = format.Length;
            for (int i = 0; i < length; ++i)
            {
                var ch = format[i];

                if (ch == '{')
                {
                    if (length > i + 1 && format[i + 1] == '{')
                        goto writeChar;

                    int j = i + 1;
                    try
                    {
                        int index = 0;
                        while (char.IsNumber(format[j]))
                        {
                            index = index * 10 + format[j] - '0';
                            ++j;
                        }

                        if (index >= args.Length)
                            throw new FormatException();

                        if (format[j] != '?')
                            goto writeChar;

                        Func<string> readString = () =>
                        {
                            var builder2 = new StringBuilder(16);

                            if (format[j] != ':' && format[j] != '?')
                                throw new FormatException();

                            ++j;
                            while (format[j] != ':' && format[j] != '}')
                            {
                                builder2.Append(format[j]);
                                ++j;
                            }
                            return builder2.ToString();
                        };

                        switch (readString())
                        {
                            // left, if value is true or non-zero or non-null
                            // otherwise, right
                            case "bool":
                            {
                                var ifTrue = readString();
                                var ifFalse = readString();
                                bool cond;
                                var obj = args[index];
                                if (obj == null)
                                {
                                    cond = false;
                                }
                                else if (obj is IConvertible)
                                {
                                    try
                                    {
                                        cond = ((IConvertible)obj).ToInt64(null) != 0L;
                                    }
                                    catch
                                    {
                                        cond = false;
                                    }
                                }
                                else
                                    throw new ArgumentException();

                                if (cond)
                                    builder.Append(ifTrue);
                                else
                                    builder.Append(ifFalse);

                                break;
                            }
                            case "plural-ru":
                            {
                                var one = readString();
                                var two = readString();
                                var three = readString();

                                var obj = args[index];
                                ulong value;
                                if (obj == null)
                                {
                                    value = 0L;
                                }
                                else if (obj is IConvertible)
                                {
                                    try
                                    {
                                        value = ((IConvertible)obj).ToUInt64(null);
                                    }
                                    catch
                                    {
                                        try
                                        {
                                            value = (ulong)Math.Abs(((IConvertible)obj).ToInt64(null));
                                        }
                                        catch
                                        {
                                            value = 0;
                                        }
                                    }
                                }
                                else
                                    throw new ArgumentException();

                                var ones = value % 10;
                                var tens = value % 100;

                                if (ones == 1 && tens != 11)
                                    builder.Append(one);
                                else if (ones > 1 && ones < 5 && (tens < 10 || tens > 19))
                                    builder.Append(two);
                                else
                                    builder.Append(three);

                                break;
                            }
                            case "plural-en":
                            {
                                var ifOne = readString();
                                var ifMany = readString();

                                bool isOne;
                                var obj = args[index];
                                if (obj == null)
                                {
                                    isOne = false;
                                }
                                else if (obj is IConvertible)
                                {
                                    try
                                    {
                                        var val = ((IConvertible)obj).ToInt64(null);
                                        isOne = val == 1L || val == -1L;
                                    }
                                    catch
                                    {
                                        isOne = false;
                                    }
                                }
                                else
                                    throw new ArgumentException();

                                if (isOne)
                                    builder.Append(ifOne);
                                else
                                    builder.Append(ifMany);
                                break;
                            }
                            default:
                                throw new FormatException();
                        }

                        if (format[j] != '}')
                            throw new FormatException();

                        i = j;
                        continue;
                    }
                    catch (ArgumentOutOfRangeException)
                    {
                        throw new FormatException();
                    }
                }

            writeChar:
                builder.Append(ch);
            }

            return string.Format(builder.ToString(), args);
        }

        public static bool SubstringEquals(this string str, int index, string equals)
        {
            for (int i = 0; i < equals.Length; i++)
            {
                if (str[index + i] != equals[i])
                    return false;
            }

            return true;
        }
    }
}
