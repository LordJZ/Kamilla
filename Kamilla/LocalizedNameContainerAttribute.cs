using System;

namespace Kamilla
{
    /// <summary>
    /// Indicates that the localized names of the enumeration members are
    /// stored inside a particular <see cref="System.Resources.ResourceManager"/>.
    /// </summary>
    [AttributeUsage(AttributeTargets.Enum, Inherited = false, AllowMultiple = false)]
    public sealed class LocalizedNameContainerAttribute : Attribute
    {
        readonly Type m_resmgr;

        /// <summary>
        /// Initializes a new instance of
        /// <see cref="Kamilla.LocalizedNameContainerAttribute"/> class.
        /// </summary>
        /// <param name="resourceManager">
        /// The <see cref="System.Type"/> that exposes access to
        /// a <see cref="System.Resources.ResourceManager"/> that
        /// contains the localized names of the enumeration members.
        /// </param>
        public LocalizedNameContainerAttribute(Type resourceManager)
        {
            if (resourceManager == null)
                throw new ArgumentNullException();

            m_resmgr = resourceManager;
        }

        /// <summary>
        /// Gets the <see cref="System.Type"/> that exposes access to
        /// a <see cref="System.Resources.ResourceManager"/> that
        /// contains the localized names of the enumeration members.
        /// </summary>
        public Type ResourceManager { get { return m_resmgr; } }
    }
}
