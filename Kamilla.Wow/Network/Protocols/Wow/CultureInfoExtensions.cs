using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Globalization;

namespace Kamilla.Network.Protocols.Wow
{
    public static class CultureInfoExtensions
    {
        static SortedDictionary<string, WowLocales> s_code2locales = new SortedDictionary<string, WowLocales>()
        {
            { "en-US", WowLocales.enUS },
            { "en-GB", WowLocales.enGB },
            { "ko-KR", WowLocales.koKR },
            { "fr-FR", WowLocales.frFR },
            { "de-DE", WowLocales.deDE },
            { "zh-CN", WowLocales.zhCN },
            { "zh-TW", WowLocales.zhTW },
            { "es-ES", WowLocales.esES },
            { "es-MX", WowLocales.esMX },
            { "ru-RU", WowLocales.ruRU },
            { "pt-BR", WowLocales.ptBR },
        };

        public static bool IsWowCompatibleCulture(this CultureInfo culture)
        {
            if (culture == null)
                throw new ArgumentNullException("culture");

            return s_code2locales.ContainsKey(culture.Name);
        }

        public static WowLocales GetWowLocale(this CultureInfo culture)
        {
            if (culture == null)
                throw new ArgumentNullException("culture");

            WowLocales locale;
            if (!s_code2locales.TryGetValue(culture.Name, out locale))
                throw new ArgumentException("culture is not WoW-compatible.", "culture");

            return locale;
        }
    }
}
