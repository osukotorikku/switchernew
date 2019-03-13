using System;
using System.Security.Cryptography.X509Certificates;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace KurikkuSwitcher
{
    class CertificateManager
    {
        public Task<bool> GetStatusAsync()
        {
            return Task.Run<bool>(() => GetStatus());
        }

        public Task<String> GetOrganisationAsync()
        {
            return Task.Run<String>(() => GetStatusOrganisation());
        }

        public void Install()
        {
            var store = new X509Store(StoreName.Root, StoreLocation.CurrentUser);
            store.Open(OpenFlags.ReadWrite);

            var certificate = new X509Certificate2(KurikkuSwitcher.Properties.Resources.Kurikku);
            store.Add(certificate);

            store.Close();
        }

        public void Uninstall()
        {
            var store = new X509Store(StoreName.Root, StoreLocation.CurrentUser);
            store.Open(OpenFlags.ReadWrite);

            var certificates = store.Certificates.Find(X509FindType.FindBySubjectName, "*.ppy.sh", true);

            foreach (var cert in certificates)
            {
                try
                {
                    store.Remove(cert);
                }
                catch (Exception ex)
                {
                    throw ex;
                }
            }

            if (store != null)
            {
                store.Close();
            }
        }

        public bool GetStatus()
        {
            var store = new X509Store(StoreName.Root, StoreLocation.CurrentUser);
            store.Open(OpenFlags.ReadOnly);

            var c = store.Certificates.Find(X509FindType.FindBySubjectName, "*.ppy.sh", true);
            bool result = c.Count >= 1;

            store.Close();

            return result;
        }

        public string GetStatusOrganisation()
        {
            var store = new X509Store(StoreName.Root, StoreLocation.CurrentUser);
            store.Open(OpenFlags.ReadOnly);

            var c = store.Certificates.Find(X509FindType.FindBySubjectName, "*.ppy.sh", true);
            if (c.Count >= 1)
            {
                X509Certificate2 lx509 = c[c.Count - 1];
                Regex regex = new Regex(@"O=(.+?),");
                MatchCollection matches = regex.Matches(lx509.Subject.ToString());
                return matches[0].Groups[1].ToString();
            }
            else {
                return "<UNKNOWN>";
            }
        }
    }
}
