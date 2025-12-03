using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System;
using System.Windows.Threading;
using Microsoft.Toolkit.Uwp.Notifications; // ToastContentBuilder, ToastNotificationManagerCompat
using Windows.UI.Notifications; // ToastNotification

namespace Notifier
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private DispatcherTimer _toastTimer;

        public MainWindow()
        {
            InitializeComponent();
            ConfigureToastTimer();
            ShowToast("Notifier started", "You will receive a reminder every 40 minutes.");
        }

        private void ConfigureToastTimer()
        {
            _toastTimer = new DispatcherTimer
            {
                Interval = TimeSpan.FromMinutes(40)
            };
            _toastTimer.Tick += (_, __) =>
            {
                ShowToast("Reminder", "站起来！ 走两步？！");
            };
            _toastTimer.Start();
        }

        private static void ShowToast(string title, string message)
        {
            // Build toast content
            var content = new ToastContentBuilder()
                .AddText(title)
                .AddText(message)
                .GetToastContent();

            // Show using ToastNotificationManagerCompat for desktop apps
            var toast = new ToastNotification(content.GetXml());
            ToastNotificationManagerCompat.CreateToastNotifier().Show(toast);
        }

        private void ResetButton_Click(object sender, RoutedEventArgs e)
        {
            if (_toastTimer is null) return;
            _toastTimer.Stop();
            _toastTimer.Start();
            ShowToast("Timer reset", "The 40-minute timer has been restarted.");
        }
    }
}