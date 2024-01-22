using System.Configuration;
using System.Reflection;

namespace Libra.Web
{
    public class Config
    {
        private static string version;
        public static string Version => version ?? (version = new System.IO.FileInfo(Assembly.GetExecutingAssembly().Location).LastWriteTime.Ticks.ToString());

        public static string AuthenticationType => "Libra";

        public static int SessionTimeout => GetValue("session-timeout", 60);

        private static T GetValue<T>(string key, T defaultValue = default(T))
        {
            return ConfigurationManager.AppSettings[key].Cast(defaultValue);
        }
    }
}