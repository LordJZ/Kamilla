using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Xml;
using System.Xml.Serialization;
using Stub = Kamilla.ConfigurationStub;

namespace Kamilla
{
    [Browsable(false), EditorBrowsable(EditorBrowsableState.Never)]
    public class ConfigurationStub
    {
        public class Option : IComparable<Option>
        {
            struct OptionComparer : IComparer<Option>
            {
                internal static int Compare(Option x, Option y)
                {
                    if ((object)x == (object)y)
                        return 0;

                    return Compare(x, y.Owner, y.Name);
                }

                internal static int Compare(Option x, string owner, string name)
                {
                    int result = StringReverseComparer.Compare(x.Owner, owner);
                    if (result != 0)
                        return result;

                    return x.Name.CompareTo(name);
                }

                int IComparer<Option>.Compare(Option x, Option y)
                {
                    return Compare(x, y);
                }
            }

            public Option()
            {
            }

            public int CompareTo(Option other)
            {
                return OptionComparer.Compare(this, other);
            }

            public int CompareTo(string owner, string name)
            {
                return OptionComparer.Compare(this, owner, name);
            }

            [XmlAttribute("Name")]
            public string Name;

            [XmlAttribute("Owner")]
            public string Owner;

            [XmlText]
            public string Value;
        }

        [XmlRoot("KamillaConfiguration")]
        public class Configuration
        {
            public Configuration()
            {
            }

            [XmlElement("Option")]
            public List<Option> InternalOptions;

            internal void CompleteDeserialization()
            {
                this.Options.Capacity = this.InternalOptions.Count;

                foreach (var opt in this.InternalOptions)
                    this.Options.Add(opt, null);

                this.InternalOptions = null;
            }

            internal void PrepareSerialization()
            {
                this.InternalOptions = new List<Option>(this.Options.Count);

                foreach (var opt in this.Options.Keys)
                    this.InternalOptions.Add(opt);
            }

            internal void CompleteSerialization()
            {
                this.InternalOptions = null;
            }

            // Second type-param is dummy
            [XmlIgnore]
            internal SortedList<Option, object> Options = new SortedList<Option, object>();
        }
    }

    /// <summary>
    /// Stores configuration options.
    /// </summary>
    public static class Configuration
    {
        private static object s_syncRoot;
        private static string s_configurationFilename;
        private static Stub.Configuration s_currentConfiguration;
        private static XmlSerializer s_configurationSerializer;

        static Configuration()
        {
            s_syncRoot = new object();
            s_configurationFilename = "Kamilla.Configuration.xml";
            s_configurationSerializer = new XmlSerializer(typeof(Stub.Configuration));

            string[] args = Environment.GetCommandLineArgs();
            for (int i = 0; i < args.Length; ++i)
            {
                if (args[i] == "-c" || args[i] == "--config")
                {
                    if (args.Length > i)
                    {
                        s_configurationFilename = args[i + 1];
                        break;
                    }
                }
            }

            // Load the configuration file.
            string path = Path.Combine(".", s_configurationFilename);
            if (File.Exists(path))
            {
                var sw = Stopwatch.StartNew();

                using (var reader = new StreamReader(path))
                    s_currentConfiguration = (Stub.Configuration)s_configurationSerializer.Deserialize(reader);

                s_currentConfiguration.CompleteDeserialization();

                sw.Stop();
                Console.WriteLine("Configuration file {0} opened in {2}: {1} configuration options.",
                    s_configurationFilename, s_currentConfiguration.Options.Count, sw.Elapsed);
            }
            else
                s_currentConfiguration = new Stub.Configuration();
        }

        static bool s_suspendSaving;
        static bool s_shouldSave;

        /// <summary>
        /// Prevents the configuration from saving to file automatically.
        /// </summary>
        public static void SuspendSaving()
        {
            s_suspendSaving = true;
        }

        /// <summary>
        /// Resumes automatical saving to file.
        /// </summary>
        public static void ResumeSaving()
        {
            s_suspendSaving = false;
            if (s_shouldSave)
                SaveConfiguration();
        }

        static void SaveConfiguration()
        {
            if (s_suspendSaving)
                s_shouldSave = true;
            else
            {
                var sw = Stopwatch.StartNew();
                s_currentConfiguration.PrepareSerialization();

                string path = Path.Combine(".", s_configurationFilename);
                using (var writer = new StreamWriter(path))
                    s_configurationSerializer.Serialize(writer, s_currentConfiguration);

                s_currentConfiguration.CompleteSerialization();

                sw.Stop();
                Console.WriteLine("Configuration file saved in {0}.", sw.Elapsed);
            }
        }

        static string InternalGetValue(string owner, string name)
        {
            var keys = s_currentConfiguration.Options.Keys;
            int index = keys.BinaryIndexOf(opt => opt.CompareTo(owner, name));

            if (index >= 0)
                return keys[index].Value ?? string.Empty;

            return null;
        }

        static void InternalAddValue(string owner, string name, string value)
        {
            s_currentConfiguration.Options.Add(new Stub.Option
            {
                Name = name,
                Owner = owner,
                Value = value,
            }, null);
        }

        static string InternalSerialize(object value)
        {
            string stringValue;
            var serializer = new XmlSerializer(value.GetType());
            using (var writer = new StringWriter())
            {
                serializer.Serialize(writer, value);
                stringValue = writer.ToString();
            }
            return stringValue;
        }

        static object InternalDeserialize(string input, Type T)
        {
            object ret;
            var serializer = new XmlSerializer(T);
            using (var reader = new StringReader(input))
                ret = serializer.Deserialize(reader);

            return ret;
        }

        /// <summary>
        /// Gets the value of an option from the configuration file.
        /// </summary>
        /// <typeparam name="T">
        /// Type of the value.
        /// </typeparam>
        /// <param name="name">
        /// Name of the configuration option.
        /// </param>
        /// <param name="defaultValue">
        /// Default value of the option.
        /// </param>
        /// <returns>
        /// Value of the option if found; otherwise, defaultValue.
        /// </returns>
        public static T GetValue<T>(string name, T defaultValue)
        {
            if (name == null)
                throw new ArgumentNullException("name");

            lock (s_syncRoot)
            {
                var trace = new StackTrace(false);
                string owner = trace.GetFrame(1).GetMethod().DeclaringType.FullName;
                string value = InternalGetValue(owner, name);
                if (value == null)
                {
                    if (defaultValue != null)
                    {
                        string stringValue;
                        if (defaultValue is IConvertible)
                            stringValue = (defaultValue as IConvertible).ToString(CultureInfo.InvariantCulture);
                        else
                            stringValue = InternalSerialize(defaultValue);

                        InternalAddValue(owner, name, stringValue);
                        SaveConfiguration();
                    }

                    return defaultValue;
                }
                else if (defaultValue is Enum)
                {
                    return (T)Enum.Parse(typeof(T), value);
                }
                else if (defaultValue is IConvertible)
                {
                    return (T)(value as IConvertible).ToType(typeof(T), CultureInfo.InvariantCulture);
                }
                else
                {
                    try
                    {
                        return (T)InternalDeserialize(value, typeof(T));
                    }
                    catch
                    {
                        return defaultValue;
                    }
                }
            }
        }

        /// <summary>
        /// Sets the value for a configuration option.
        /// </summary>
        /// <param name="name">
        /// Name of the configuration option.
        /// </param>
        /// <param name="value">
        /// Value of the configuration option.
        /// </param>
        public static void SetValue(string name, object value)
        {
            if (name == null)
                throw new ArgumentNullException("name");

            lock (s_syncRoot)
            {
                var trace = new StackTrace(false);
                string owner = trace.GetFrame(1).GetMethod().DeclaringType.FullName;
                string stringValue;
                if (value is IConvertible)
                    stringValue = (value as IConvertible).ToString(CultureInfo.InvariantCulture);
                else
                    stringValue = InternalSerialize(value);

                var options = s_currentConfiguration.Options;
                var keys = options.Keys;

                int index = keys.BinaryIndexOf(opt => opt.CompareTo(owner, name));
                if (index >= 0)
                {
                    if (keys[index].Value == stringValue)
                        return;

                    keys[index].Value = stringValue;
                }
                else
                    InternalAddValue(owner, name, stringValue);

                SaveConfiguration();
            }
        }
    }
}
