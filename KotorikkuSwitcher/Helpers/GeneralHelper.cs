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
    }
}