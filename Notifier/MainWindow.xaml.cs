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
using Microsoft.Extensions.Configuration;
using System.IO;

namespace Notifier
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private DispatcherTimer _toastTimer;
        private DispatcherTimer _uiTimer;
        private DateTime _nextReminderUtc;
        private IConfiguration _config;
        private int _intervalMinutes;
        private string _reminderMessage;
        private string _startupMessage;
        private string _title;

        public MainWindow()
        {
            InitializeComponent();
            LoadConfiguration();
            ConfigureToastTimer();
            ConfigureUiTimer();
            ShowToast(_title, _startupMessage);
        }

        private void LoadConfiguration()
        {
            // Defaults in case configuration is missing or invalid
            const int defaultInterval = 40;
            const string defaultReminderMessage = "站起来！走两步？！";
            const string defaultTitle = "Reminder";

            // Determine app base path and load appsettings.json (optional)
            var basePath = AppContext.BaseDirectory;

            try
            {
                var builder = new ConfigurationBuilder()
                    .SetBasePath(basePath)
                    .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true);

                _config = builder.Build();
            }
            catch
            {
                // If configuration building fails, use an empty configuration
                _config = new ConfigurationBuilder().Build();
            }

            var reminderSection = _config.GetSection("Reminder");

            // Read values with safe fallbacks
            int configuredInterval = reminderSection.GetValue<int>("IntervalMinutes", defaultInterval);
            _intervalMinutes = configuredInterval > 0 ? configuredInterval : defaultInterval;

            _reminderMessage = reminderSection.GetValue<string>("Message") ?? defaultReminderMessage;

            _title = reminderSection.GetValue<string>("Title") ?? defaultTitle;

            // Startup message can reference the resolved interval
            var configuredStartupMessage = reminderSection.GetValue<string>("StartupMessage");
            _startupMessage = string.IsNullOrWhiteSpace(configuredStartupMessage)
                ? $"You will receive a reminder every {_intervalMinutes} minutes."
                : configuredStartupMessage;
        }

        private void ConfigureToastTimer()
        {
            _toastTimer = new DispatcherTimer
            {
                Interval = TimeSpan.FromMinutes(_intervalMinutes)
            };
            _toastTimer.Tick += (_, __) =>
            {
                ShowToast(_title, _reminderMessage);
                ScheduleNextReminder();
            };
            _toastTimer.Start();
            ScheduleNextReminder();
        }

        private void ConfigureUiTimer()
        {
            _uiTimer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(1) };
            _uiTimer.Tick += (_, __) => UpdateRemainingText();
            _uiTimer.Start();
            UpdateRemainingText();
        }

        private void ScheduleNextReminder()
        {
            _nextReminderUtc = DateTime.UtcNow.AddMinutes(_intervalMinutes);
        }

        private void UpdateRemainingText()
        {
            var remaining = _nextReminderUtc - DateTime.UtcNow;
            if (remaining <= TimeSpan.Zero)
            {
                RemainingText.Text = "00:00";
                return;
            }
            RemainingText.Text = $"{(int)remaining.TotalMinutes:00}:{remaining.Seconds:00}";
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
            _toastTimer.Interval = TimeSpan.FromMinutes(_intervalMinutes);
            _toastTimer.Start();
            ScheduleNextReminder();
            //ShowToast(_title, $"The {_intervalMinutes}-minute timer has been restarted.");
        }

        public void ResetTimerFromTray()
        {
            if (_toastTimer is null) return;
            _toastTimer.Stop();
            _toastTimer.Interval = TimeSpan.FromMinutes(_intervalMinutes);
            _toastTimer.Start();
            ScheduleNextReminder();
            ShowToast(_title, $"The {_intervalMinutes}-minute timer has been restarted.");
        }
    }
}