using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;

namespace KotorikkuSwitcher
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
            var serverIp = await GeneralHelper.GetKotorikkuAddressAsync();
            if (serverIp == string.Empty)
            {
                MessageBox.Show("Ошибка при получении IP-адреса kotorikku. Возможно, у вас проблемы с Интернетом?" + Environment.NewLine +
                    "Будет использоваться встроенный IP-адрес. Быть может, он уже устарел.");
                serverIp = Constants.KotorikkuHardcodedIp;
            }
            serverSwitcher = new ServerSwitcher(serverIp);

            // switcher init
            await CheckServer();
        }

        private async Task CheckSertificate()
        {
            certButton.IsEnabled = false;
            var certificateStatus = await certificateManager.GetStatusAsync();
            certButton.Content = certificateStatus ? Constants.UiUninstallCertificate : Constants.UiInstallCertificate;
            certButton.IsEnabled = true;
        }

        private async Task CheckServer()
        {
            switchButton.IsEnabled = false;
            var currentServer = await serverSwitcher.GetCurrentServerAsync();
            statusLabel.Content = (currentServer == Server.Kotorikku)
                ? Constants.UiYouArePlayingOnKotorikku : Constants.UiYouArePlayingOnOfficial;
            switchButton.Content = (currentServer == Server.Official)
                ? Constants.UiSwitchToKotorikku : Constants.UiSwitchToOfficial;
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
                    serverSwitcher.SwitchToKotorikku();
                }
                else
                {
                    serverSwitcher.SwitchToOfficial();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Произошла ошибка при переключении сервера. Если вы уверены, что её не должно быть и у вас отключен антивирус, обратитесь за помощью! support@kotorikku.ru"
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
            System.Diagnostics.Process.Start("https://kotorikku.ru");
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
