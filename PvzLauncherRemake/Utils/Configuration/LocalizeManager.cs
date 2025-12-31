using System.Globalization;
using System.Reflection;
using WPFLocalizeExtension.Engine;
using WPFLocalizeExtension.Extensions;

namespace PvzLauncherRemake.Utils.Configuration
{
    public class LocalizeManager
    {
        private static readonly string AssemblyName = Assembly.GetExecutingAssembly().GetName().Name!;

        public static void SwitchLanguage(string languageCode = "zh-CN")
        {
            var newCulture = new CultureInfo(languageCode);
            Thread.CurrentThread.CurrentCulture = newCulture;
            Thread.CurrentThread.CurrentUICulture = newCulture;

            LocalizeDictionary.Instance.Culture = newCulture;
        }

        public static string GetLoc(string key)
        {
            return LocExtension.GetLocalizedValue<string>(key) ?? key;
        }

        public static string GetLoc(string directory, string key)
        {
            string fullKey = $"{AssemblyName}:{directory}:{key}";
            string value = LocExtension.GetLocalizedValue<string>(fullKey);
            return value ?? fullKey;
        }
    }
}
