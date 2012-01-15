using System;
using System.Globalization;
using System.Threading;

namespace Kamilla
{
    /// <summary>
    /// Controls user interface culture information.
    /// </summary>
    public static class UICulture
    {
        static CultureInfo s_culture;

        /// <summary>
        /// Initializes the user interface culture.
        /// </summary>
        public static void Initialize()
        {
            if (s_culture != null)
                return;

            var cultureName = Configuration.GetValue("UI Language", string.Empty);
            if (!string.IsNullOrEmpty(cultureName))
            {
                try
                {
                    s_culture = CultureInfo.GetCultureInfo(cultureName);
                }
                catch
                {
                }
            }

            if (s_culture != null)
                Thread.CurrentThread.CurrentUICulture = s_culture;
            else
                s_culture = Thread.CurrentThread.CurrentUICulture;
        }

        /// <summary>
        /// Gets or sets current user interface culture.
        /// </summary>
        public static CultureInfo Culture
        {
            get
            {
                if (s_culture == null)
                    Initialize();

                return s_culture;
            }
            set
            {
                if (value == null)
                    throw new ArgumentNullException("value");

                if (s_culture == value)
                    return;

                s_culture = value;
                Thread.CurrentThread.CurrentUICulture = value;
                Configuration.SetValue("UI Language", value.Name);

                if (UICultureChanged != null)
                    UICultureChanged(value, EventArgs.Empty);
            }
        }

        /// <summary>
        /// Occurs when <see cref="Kamilla.UICulture.Culture"/> property changes.
        /// </summary>
        public static event EventHandler UICultureChanged;
    }
}
