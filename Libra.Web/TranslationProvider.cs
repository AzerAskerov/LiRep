using Libra.Contract;
using SecondLanguage;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Threading;
using System.Web;

namespace Libra.Web
{
    public class TranslationProvider : ITranslationProvider
    {
        private const string TRANSLATIONS_LOCATION = "~/Resources";
        private readonly Lazy<IDictionary<string, GettextPOTranslation>> translations;

        public TranslationProvider()
        {
            translations = new Lazy<IDictionary<string, GettextPOTranslation>>(LoadTranslations);
        }

        public string GetString(string key)
        {
            return GetString(key, new CultureInfo("en-US"));
        }

        public string GetString<TEnum>(TEnum key)
        {
            return GetString($"{typeof(TEnum).Name}_{key}", new CultureInfo("en-US"));
        }

        public string GetString(string key, CultureInfo culture)
        {
            if (culture == null || key == null)
            {
                return key;
            }

            GettextPOTranslation dictionary;

            if (!translations.Value.TryGetValue(culture.ToString(), out dictionary))
            {
                return key;
            }

            var result = dictionary.GetString(key.ToUpperInvariant());

            return string.IsNullOrEmpty(result)
                ? key
                : result;
        }

        private static IDictionary<string, GettextPOTranslation> LoadTranslations()
        {
            var result = new Dictionary<string, GettextPOTranslation>(StringComparer.OrdinalIgnoreCase);
            foreach (var path in Directory.GetFiles(HttpContext.Current.Server.MapPath(TRANSLATIONS_LOCATION), "*.po"))
            {
                var locale = Path.GetFileNameWithoutExtension(path);
                if (string.IsNullOrEmpty(locale))
                {
                    continue;
                }
                var catalog = new GettextPOTranslation();
                catalog.Load(path);
                result.Add(locale, catalog);
            }
            return result;
        }
    }
}