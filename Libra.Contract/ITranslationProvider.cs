using System.Globalization;

namespace Libra.Contract
{
    public interface ITranslationProvider
    {
        string GetString(string key);
        string GetString<TEnum>(TEnum key);
        string GetString(string key, CultureInfo culture);
    }
}
