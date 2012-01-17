using System;

namespace Kamilla
{
    /// <summary>
    /// Indicates which string of the enumeration's <see cref="System.Resources.ResourceManager"/>
    /// should be used as the localized name of the current enumeration member.
    /// </summary>
    [AttributeUsage(AttributeTargets.Field, Inherited = false, AllowMultiple = false)]
    public sealed class LocalizedNameAttribute : Attribute
    {
        string m_key;

        /// <summary>
        /// Initializes a new instance of <see cref="Kamilla.LocalizedNameAttribute"/> class.
        /// </summary>
        /// <param name="key">
        /// The name of the string stored inside the enumeration's
        /// <see cref="System.Resources.ResourceManager"/> that
        /// should be used as the localized name of the current enumeration member.
        /// </param>
        public LocalizedNameAttribute(string key)
        {
            m_key = key;
        }

        /// <summary>
        /// Gets the name of the string stored inside the enumeration's
        /// <see cref="System.Resources.ResourceManager"/> that
        /// should be used as the localized name of the current enumeration member.
        /// </summary>
        public string Key { get { return m_key; } }
    }
}
