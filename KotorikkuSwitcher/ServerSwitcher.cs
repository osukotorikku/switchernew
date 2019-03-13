using System.Linq;
using KurikkuSwitcher.Extensions;
using KurikkuSwitcher.Helpers;
using System.Threading.Tasks;

namespace KurikkuSwitcher
{
    class ServerSwitcher
    {
        private readonly string serverAddress, serverBMAddress;

        public ServerSwitcher(string KurikkuIpAddress, string KurikkuBMIpAddress)
        {
            this.serverAddress = KurikkuIpAddress;
            this.serverBMAddress = KurikkuBMIpAddress;
        }

        public void SwitchToKurikku()
        {
            var lines = HostsFile.ReadAllLines();
            var result = lines.Where(x => !x.Contains("ppy.sh")).ToList();
            result.AddRange
            (
                serverAddress + " osu.ppy.sh",
                serverAddress + " c.ppy.sh",
                serverAddress + " c1.ppy.sh",
                serverAddress + " c2.ppy.sh",
                serverAddress + " c3.ppy.sh",
                serverAddress + " c4.ppy.sh",
                serverAddress + " c5.ppy.sh",
                serverAddress + " c6.ppy.sh",
                serverAddress + " ce.ppy.sh",
                serverAddress + " a.ppy.sh",
                serverAddress + " s.ppy.sh",
                serverAddress + " i.ppy.sh",
                serverBMAddress + " bm6.ppy.sh"
            );
            HostsFile.WriteAllLines(result);
        }

        public void SwitchToOfficial()
        {
            HostsFile.WriteAllLines(HostsFile.ReadAllLines().Where(x => !x.Contains("ppy.sh")));
        }

        public Task<Server> GetCurrentServerAsync()
        {
            return Task.Run<Server>(() => GetCurrentServer());
        }

        public Server GetCurrentServer()
        {
            bool isKurikku = HostsFile.ReadAllLines().Any(x => x.Contains("osu.ppy.sh") && !x.Contains("#"));
            return isKurikku ? Server.Kurikku : Server.Official;
        }
    }

    public enum Server
    {
        Official,
        Kurikku
    }
}
