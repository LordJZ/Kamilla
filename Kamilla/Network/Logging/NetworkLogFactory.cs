using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Kamilla.IO;

namespace Kamilla.Network.Logging
{
    public static class NetworkLogFactory
    {
        public static IEnumerable<NetworkLogWrapper> Wrappers { get; private set; }
        public static string AllFileFiltersWithAny { get; private set; }
        public static int AllFileFiltersWithAnyCount { get; private set; }
        public static string AllFileFiltersReadOnly { get; private set; }
        public static int AllFileFiltersReadOnlyCount { get; private set; }

        static readonly List<NetworkLogWrapper> s_logs = new List<NetworkLogWrapper>();
        static int s_maxHeaderLength = 0;
        static bool s_initialized = false;

        public static void Initialize()
        {
            if (s_initialized)
                throw new InvalidOperationException("NetworkLogFactory is already initialized.");

            s_initialized = true;

            var ctorTypes = new[] { typeof(NetworkLogMode) };
            var networkLogType = typeof(NetworkLog);
            var networkLogAttrType = typeof(NetworkLogAttribute);

            foreach (var type in TypeManager.Types)
            {
                if (!type.IsAbstract && type.IsSubclassOf(networkLogType))
                {
                    if (type.GetConstructor(ctorTypes) == null)
                        Console.WriteLine("Error: Failed to find .ctor(NetworkLogMode) for class " + type);

                    var attributes = (NetworkLogAttribute[])type.GetCustomAttributes(networkLogAttrType, false);
                    if (attributes.Length != 0)
                    {
                        var wrapper = new NetworkLogWrapper(attributes[0], type);

                        if (s_maxHeaderLength < wrapper.ReqHeaderLength)
                            s_maxHeaderLength = wrapper.ReqHeaderLength;

                        s_logs.Add(wrapper);
                    }
                }
            }

            Console.WriteLine("Loaded {0} network logs", s_logs.Count);

            s_logs.Sort((x, y) => string.Compare(x.Name, y.Name));
            Wrappers = s_logs.ToArray();

            // AllFileFiltersReadOnly
            {
                int count = 0;
                var filters = new StringBuilder();
                for (int i = 0; i < s_logs.Count; )
                {
                    if (!s_logs[i].IsReadOnly)
                        filters.Append(s_logs[i].FileFilter);

                    if (++i != s_logs.Count)
                        filters.Append('|');

                    ++count;
                }

                AllFileFiltersReadOnly = filters.ToString();
                AllFileFiltersReadOnlyCount = count;
            }

            // AllFileFiltersWithAny
            {
                int count = 0;
                var filters = new StringBuilder();
                for (int i = 0; i < s_logs.Count; ++i)
                {
                    filters.Append(s_logs[i].FileFilter);
                    filters.Append('|');

                    ++count;
                }
                filters.Append(NetworkStrings.AllFiles + " (*.*)|*.*");
                ++count;

                AllFileFiltersWithAny = filters.ToString();
                AllFileFiltersWithAnyCount = count;
            }
        }

        public static NetworkLog GetNetworkLog(string filename)
        {
            if (!s_initialized)
                Initialize();

            var ext = Path.GetExtension(filename);

            var extFittingDumps = new List<NetworkLogWrapper>();
            foreach (var info in s_logs)
            {
                if (info.FileExtension == ext)
                    extFittingDumps.Add(info);
            }

            if (extFittingDumps.Count == 0)
                extFittingDumps = s_logs;
            else if (extFittingDumps.Count == 1)
                return extFittingDumps[0].Activate(NetworkLogMode.Reading);

            byte[] header;
            using (var sh = new StreamHandler(filename, FileMode.Open))
                header = sh.ReadBytes(Math.Min((int)sh.Length, s_maxHeaderLength));

            foreach (var wrapper in extFittingDumps)
            {
                if (wrapper.Fits(header))
                    return wrapper.Activate(NetworkLogMode.Reading);
            }

            return null;
        }
    }
}
