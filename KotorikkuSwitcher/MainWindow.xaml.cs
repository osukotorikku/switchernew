using System;
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

        public MainWindow()
        {
            Dispatcher.UnhandledException += Dispatcher_UnhandledException;
            InitializeComponent();
            // base init
            certificateManager = new CertificateManager();
            switchButton.Content = "Получение IP адреса...";
            certButton.Content = "Получение статуса сертификата...";
            statusLabel.Content = Constants.UiUpdatingStatus;
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
                MessageBox.Show("Ошибка при получении IP-адреса kurikku. Возможно, у вас проблемы с Интернетом?" + Environment.NewLine +
                    "Будет использоваться встроенный IP-адрес. Быть может, он уже устарел.");
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
            certButton.Content = certificateStatus ? Constants.UiUninstallCertificate : Constants.UiInstallCertificate;
            certButton.IsEnabled = true;

            var certificateStatusOrg = await certificateManager.GetOrganisationAsync();
            certStatus.Text = "Установлен сертификат "+certificateStatusOrg;
        }

        private async Task CheckServer()
        {
            switchButton.IsEnabled = false;
            var currentServer = await serverSwitcher.GetCurrentServerAsync();
            statusLabel.Content = (currentServer == Server.Kurikku)
                ? Constants.UiYouArePlayingOnKurikku : Constants.UiYouArePlayingOnOfficial;
            switchButton.Content = (currentServer == Server.Official)
                ? Constants.UiSwitchToKurikku : Constants.UiSwitchToOfficial;
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
                MessageBox.Show("Произошла ошибка при переключении сервера. Если вы уверены, что её не должно быть и у вас отключен антивирус, обратитесь за помощью! support@kurikku.pw"
                + string.Format("\r\n\r\nДетали:\r\n{0}", ex.Message));
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
                MessageBox.Show("Произошла ошибка при установке/удалении сертификата."
                    + string.Format("\r\n\r\nДетали:\r\n{0}", ex.Message));
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
    }
}
