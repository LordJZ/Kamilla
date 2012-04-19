using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Kamilla;
using Kamilla.Network.Parsing;
using Kamilla.Network.Protocols;
using Kamilla.Network.Viewing;

namespace NetworkLogViewer
{
    public static class ParsingHelper
    {
        public static string GetContentName(object obj, params object[] args)
        {
            bool format = args != null && args.Length == 1;

            if (obj is char[] || obj is string)
                return !format ? Strings.View_TextContents : Strings.View_TextContentsN.LocalizedFormat(args);

            if (obj is byte[])
                return !format ? Strings.View_BinaryContents : Strings.View_BinaryContentsN.LocalizedFormat(args);

            if (obj is ImageSource)
                return !format ? Strings.View_Image : Strings.View_ImageN.LocalizedFormat(args);

            return Strings.View_UnknownObjectN.LocalizedFormat(args);
        }

        static PacketParser ParseIfNeed(Protocol protocol, ViewerItem item)
        {
            if (item == null)
                throw new ArgumentNullException("item");

            if (protocol == null)
                throw new ArgumentNullException("protocol");

            if (item.Parser == null)
                protocol.CreateParser(item);

            var parser = item.Parser;

            if (!parser.IsParsed)
                parser.Parse();

            return parser;
        }

        static ValueTuple<object, string>[] s_emptyStrings = new ValueTuple<object, string>[0];
        public static ValueTuple<object, string>[] ExtractStrings(Protocol protocol, ViewerItem item,
            Encoding stringEncoding = null)
        {
            var parser = ParseIfNeed(protocol, item);

            if (!parser.HasContainedData)
                return s_emptyStrings;

            var result = new List<ValueTuple<object, string>>(parser.ContainedData.Count);

            foreach (var obj in parser.ContainedData)
            {
                var str = obj as string;
                if (str != null)
                {
                    result.Add(new ValueTuple<object, string>(obj, str));
                    continue;
                }

                var chars = obj as char[];
                if (chars != null)
                {
                    result.Add(new ValueTuple<object, string>(obj, new string(chars)));
                    continue;
                }

                if (stringEncoding != null)
                {
                    var bytes = obj as byte[];
                    if (bytes != null)
                    {
                        try
                        {
                            result.Add(new ValueTuple<object, string>(obj, stringEncoding.GetString(bytes)));
                            continue;
                        }
                        catch
                        {
                        }
                    }
                }

                // cannot interpret this
            }

            return result.ToArray();
        }

        static ValueTuple<object, byte[]>[] s_emptyBinaryDatas = new ValueTuple<object, byte[]>[0];
        public static ValueTuple<object, byte[]>[] ExtractBinaryDatas(Protocol protocol, ViewerItem item,
            Encoding stringEncoding = null, Type imageEncoderType = null)
        {
            var parser = ParseIfNeed(protocol, item);
            ConstructorInfo ctor = null;
            object[] ctorArgs = null;
            if (imageEncoderType != null)
            {
                ctor = imageEncoderType.GetConstructor(BindingFlags.Public, null,
                    new[] { typeof(Stream), typeof(BitmapCreateOptions), typeof(BitmapCacheOption) }, null);

                if (ctor == null)
                {
                    ctor = imageEncoderType.GetConstructor(BindingFlags.Public, null,
                        new[] { typeof(Stream) }, null);

                    ctorArgs = new object[]
                    {
                        null,
                        BitmapCreateOptions.PreservePixelFormat,
                        BitmapCacheOption.Default
                    };
                }
                else
                {
                    ctorArgs = new object[]
                    {
                        null
                    };
                }

                if (ctor == null)
                    throw new ArgumentException("imageEncoderType");
            }

            if (!parser.HasContainedData)
                return s_emptyBinaryDatas;

            var result = new List<ValueTuple<object, byte[]>>(parser.ContainedData.Count);

            foreach (var obj in parser.ContainedData)
            {
                var bytes = obj as byte[];
                if (bytes != null)
                {
                    result.Add(new ValueTuple<object, byte[]>(obj, bytes));
                    continue;
                }

                if (stringEncoding != null)
                {
                    var str = obj as string;
                    if (str != null)
                    {
                        try
                        {
                            result.Add(new ValueTuple<object, byte[]>(obj, stringEncoding.GetBytes(str)));
                        }
                        catch
                        {
                        }
                        continue;
                    }
                }

                if (imageEncoderType != null)
                {
                    var img = obj as ImageSource;
                    if (img != null)
                    {
                        try
                        {
                            using (var stream = new MemoryStream())
                            {
                                ctorArgs[0] = stream;
                                var encoder = (BitmapEncoder)ctor.Invoke(ctorArgs);
                                encoder.Save(stream);
                                result.Add(new ValueTuple<object, byte[]>(obj, stream.ToArray()));
                            }
                        }
                        catch
                        {
                        }
                        continue;
                    }
                }

                // cannot interpret this
            }

            return result.ToArray();
        }

        static ValueTuple<object, ImageSource>[] s_emptyImages = new ValueTuple<object, ImageSource>[0];
        public static ValueTuple<object, ImageSource>[] ExtractImages(Protocol protocol, ViewerItem item,
            bool convertImages = false)
        {
            var parser = ParseIfNeed(protocol, item);
            if (!parser.HasContainedData)
                return s_emptyImages;

            var result = new List<ValueTuple<object, ImageSource>>(parser.ContainedData.Count);

            foreach (var obj in parser.ContainedData)
            {
                var img = obj as ImageSource;
                if (img != null)
                {
                    result.Add(new ValueTuple<object, ImageSource>(obj, img));
                    continue;
                }

                if (convertImages)
                {
                    var bytes = obj as byte[];
                    if (bytes != null)
                    {
                        try
                        {
                            using (var stream = new MemoryStream(bytes))
                            {
                                var decoder = BitmapDecoder.Create(stream,
                                    BitmapCreateOptions.PreservePixelFormat, BitmapCacheOption.Default);

                                result.Add(new ValueTuple<object, ImageSource>(obj, decoder.Frames[0]));
                            }
                        }
                        catch
                        {
                        }
                        continue;
                    }
                }

                // cannot interpret this
            }

            return result.ToArray();
        }
    }
}
