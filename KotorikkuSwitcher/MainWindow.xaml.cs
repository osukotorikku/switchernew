using System;
using System.Globalization;
using System.Resources;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;

namespace KurikkuSwitcher
{
    public partial class MainWindow : Window
    {
        ServerSwitcher serverSwitcher;
        CertificateManager certificateManager;

        ResourceManager resourcesApp;
        CultureInfo cul;


        public MainWindow()
        {
            Dispatcher.UnhandledException += Dispatcher_UnhandledException;
            InitializeComponent();
            // base init
            resourcesApp = new ResourceManager("KurikkuSwitcher.Resources.Res", typeof(MainWindow).Assembly);
            string locale = "en";

            locale = GeneralHelper.GetSetting("locale");
            if (String.IsNullOrEmpty(locale)) {
                GeneralHelper.SetSetting("locale", "en");
            }
            switch (locale) {
                case "en":
                default:
                    cul = CultureInfo.CreateSpecificCulture("en");
                    switchLocaleButton.Content = resourcesApp.GetString("UiSwitchToRussian", cul);
                    break;
                case "ru":
                    cul = CultureInfo.CreateSpecificCulture("ru");
                    switchLocaleButton.Content = resourcesApp.GetString("UiSwitchToEnglish", cul); 
                    break;
            }

            certificateManager = new CertificateManager();
            switchButton.Content = resourcesApp.GetString("UiGettingIPs", cul);
            certButton.Content = resourcesApp.GetString("UiGettingCertificateStatus", cul);
            certButton.ToolTip = resourcesApp.GetString("UiCertToolTip", cul);
            statusLabel.Content = resourcesApp.GetString("UiUpdatingStatus", cul);
            DisableSwitching();
            InitSwitcher();
        }

        private async void InitSwitcher()
        {
            // certificate init
            await CheckSertificate();

            // load server ip
            var serverIps = await GeneralHelper.GetKurikkuAddressAsync();
            if (serverIps[0] == string.Empty || serverIps[1] == string.Empty)
            {
                MessageBox.Show(resourcesApp.GetString("UiErrorGettingIPs_1", cul) + Environment.NewLine +
                    resourcesApp.GetString("UiErrorGettingIPs_2", cul));
                serverIps = new string[]{ Constants.KurikkuHardcodedIp, Constants.KurikkuHardcodedBMIp };
            }
            serverSwitcher = new ServerSwitcher(serverIps[0], serverIps[1]);

            // switcher init
            await CheckServer();
        }

        private async Task CheckSertificate()
        {
            certButton.IsEnabled = false;
            var certificateStatus = await certificateManager.GetStatusAsync();
            certButton.Content = certificateStatus ? resourcesApp.GetString("UiUninstallCertificate", cul) : resourcesApp.GetString("UiInstallCertificate", cul);
            certButton.IsEnabled = true;

            var certificateStatusOrg = await certificateManager.GetOrganisationAsync();
            certStatus.Text = resourcesApp.GetString("UiInstalledCertificate", cul) + certificateStatusOrg;
        }

        private async Task CheckServer()
        {
            switchButton.IsEnabled = false;
            var currentServer = await serverSwitcher.GetCurrentServerAsync();
            statusLabel.Content = (currentServer == Server.Kurikku)
                ? resourcesApp.GetString("UiYouArePlayingOnKurikku", cul) : resourcesApp.GetString("UiYouArePlayingOnOfficial", cul);
            switchButton.Content = (currentServer == Server.Official)
                ? resourcesApp.GetString("UiSwitchToKurikku", cul) : resourcesApp.GetString("UiSwitchToOfficial", cul);
            switchButton.IsEnabled = true;
        }

        private void closeButton_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }

        private async void switchButton_Click(object sender, RoutedEventArgs e)
        {
            var serv = await serverSwitcher.GetCurrentServerAsync();

            try
            {
                if (serv == Server.Official)
                {
                    serverSwitcher.SwitchToKurikku();
                }
                else
                {
                    serverSwitcher.SwitchToOfficial();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(resourcesApp.GetString("UiErrorSwitching", cul)
                + string.Format("\r\n\r\n{0}\r\n{1}", resourcesApp.GetString("UiDetails", cul), ex.Message));
                Logger.Log(ex);
            }

            await CheckServer();
        }

        private async void sertButton_Click(object sender, RoutedEventArgs e)
        {
            var status = await certificateManager.GetStatusAsync();

            try
            {
                if (status)
                {
                    certificateManager.Uninstall();
                }
                else
                {
                    certificateManager.Install();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(resourcesApp.GetString("UiErrorCertSwitching", cul)
                    + string.Format("\r\n\r\n{0}\r\n{1}", resourcesApp.GetString("UiDetails", cul), ex.Message));
                Logger.Log(ex);
            }

            await CheckSertificate();
        }

        private void DisableSwitching()
        {
            switchButton.IsEnabled = false;
            certButton.IsEnabled = false;
        }

        private void websiteText_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            System.Diagnostics.Process.Start("https://kurikku.pw");
        }

        private void titleBar_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            base.OnMouseLeftButtonDown(e);
            this.DragMove();
        }

        void Dispatcher_UnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
        {
            Logger.Fatal(e.Exception);
        }

        void switchLocaleButton_Click(object sender, RoutedEventArgs e) {
            string locale = GeneralHelper.GetSetting("locale");
            string nextLocale = "";
            switch (locale) {
                case "en":
                default:
                    nextLocale = "ru";
                    break;
                case "ru":
                    nextLocale = "en";
                    break;
            }
            GeneralHelper.SetSetting("locale", nextLocale);
            // Restarting App
            System.Diagnostics.Process.Start(Application.ResourceAssembly.Location);
            Application.Current.Shutdown();
        }
    }
}
