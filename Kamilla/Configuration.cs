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
    [Browsable(false)]
    [EditorBrowsable(EditorBrowsableState.Never)]
    public class ConfigurationStub
    {
        public class Option
        {
            public Option()
            {
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
            public List<Option> Options;
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
                using (var reader = new StreamReader(path))
                    s_currentConfiguration = (Stub.Configuration)s_configurationSerializer.Deserialize(reader);
            }
            else
            {
                s_currentConfiguration = new Stub.Configuration();
                s_currentConfiguration.Options = new List<Stub.Option>();
            }

            Console.WriteLine("Configuration file {0} opened: {1} configuration options.",
                s_configurationFilename, s_currentConfiguration.Options.Count);
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
                string path = Path.Combine(".", s_configurationFilename);
                using (var writer = new StreamWriter(path))
                    s_configurationSerializer.Serialize(writer, s_currentConfiguration);

                Console.WriteLine("Configuration file saved.");
            }
        }

        static string InternalGetValue(string owner, string name)
        {
            foreach (var option in s_currentConfiguration.Options)
            {
                if (option.Owner == owner && option.Name == name)
                    return option.Value ?? string.Empty;
            }

            return null;
        }

        static void InternalAddValue(string owner, string name, string value)
        {
            s_currentConfiguration.Options.Add(new Stub.Option
            {
                Name = name,
                Owner = owner,
                Value = value,
            });
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
            lock (s_syncRoot)
            {
                var trace = new StackTrace(false);
                string owner = trace.GetFrame(1).GetMethod().DeclaringType.FullName;
                string value = InternalGetValue(owner, name);
                if (value == null)
                {
                    string stringValue;
                    if (defaultValue is IConvertible)
                        stringValue = (defaultValue as IConvertible).ToString(CultureInfo.InvariantCulture);
                    else
                        stringValue = InternalSerialize(defaultValue);

                    InternalAddValue(owner, name, stringValue);
                    SaveConfiguration();
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

                for (int i = 0; i < options.Count; ++i)
                {
                    var option = options[i];
                    if (option.Owner == owner && option.Name == name)
                    {
                        option.Value = stringValue;
                        SaveConfiguration();
                        return;
                    }
                }

                InternalAddValue(owner, name, stringValue);
                SaveConfiguration();
            }
        }
    }
}
