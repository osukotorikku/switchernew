using System.Configuration;
using System.Net;
using System.Threading.Tasks;

namespace KurikkuSwitcher
{
    static class GeneralHelper
    {
        public async static Task<string[]> GetKurikkuAddressAsync()
        {
            using (var webClient = new WebClient())
            {
                string result = string.Empty;
                try
                {
                    var line = await webClient.DownloadStringTaskAsync(Constants.KurikkuIpApiAddress);
                    result = line;
                }
                catch { }

                string[] resultToReturn = result.Trim().ToString().Split('|');
                if (result.Trim() == string.Empty) {
                    resultToReturn = new string[] { "", "" };
                }
                return resultToReturn;
            }
        }

        // StackOverFlow
        public static string GetSetting(string key)
        {
            return ConfigurationManager.AppSettings[key];
        }

        public static void SetSetting(string key, string value)
        {
            Configuration configuration = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
            configuration.AppSettings.Settings[key].Value = value;
            configuration.Save(ConfigurationSaveMode.Full, true);
            ConfigurationManager.RefreshSection("appSettings");
        }

        /*public static void AddSetting(string key, string value) {
            Configuration configuration = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
            configuration.AppSettings.Settings.Add()
            configuration.Save(ConfigurationSaveMode.Full, true);
            ConfigurationManager.RefreshSection("appSettings");
        }*/
    }
}