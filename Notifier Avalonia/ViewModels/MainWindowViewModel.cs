namespace Notifier_Avalonia.ViewModels;

using System;
using System.Timers;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.Configuration;
using Microsoft.Toolkit.Uwp.Notifications;
using Windows.UI.Notifications;

public partial class MainWindowViewModel : ViewModelBase
{
    private readonly Timer _toastTimer;
    private readonly Timer _uiTimer;
    private DateTime _nextReminderUtc;
    private readonly IConfiguration _config;

    private int _intervalMinutes;
    private string _reminderMessage = string.Empty;
    private string _startupMessage = string.Empty;
    private string _title = string.Empty;

    private string _remainingText = "00:00";
    public string RemainingText
    {
        get => _remainingText;
        private set => SetProperty(ref _remainingText, value);
    }

    public IRelayCommand ResetCommand { get; }

    public MainWindowViewModel()
    {
        _config = LoadConfiguration();
        ReadSettings();

        _toastTimer = new Timer(TimeSpan.FromMinutes(_intervalMinutes).TotalMilliseconds)
        {
            AutoReset = true,
            Enabled = true
        };
        _toastTimer.Elapsed += (_, __) =>
        {
            ShowToast(_title, _reminderMessage);
            ScheduleNextReminder();
        };
        ScheduleNextReminder();

        _uiTimer = new Timer(TimeSpan.FromSeconds(1).TotalMilliseconds)
        {
            AutoReset = true,
            Enabled = true
        };
        _uiTimer.Elapsed += (_, __) => UpdateRemainingText();
        UpdateRemainingText();

        // Show startup toast
        ShowToast(_title, _startupMessage);

        ResetCommand = new RelayCommand(ResetTimer);
    }

    private IConfiguration LoadConfiguration()
    {
        var basePath = AppContext.BaseDirectory;
        try
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath(basePath)
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true);
            return builder.Build();
        }
        catch
        {
            return new ConfigurationBuilder().Build();
        }
    }

    private void ReadSettings()
    {
        const int defaultInterval = 40;
        const string defaultReminderMessage = "站起来！走两步？！";
        const string defaultTitle = "Reminder";

        var reminderSection = _config.GetSection("Reminder");

        int configuredInterval = reminderSection.GetValue<int>("IntervalMinutes", defaultInterval);
        _intervalMinutes = configuredInterval > 0 ? configuredInterval : defaultInterval;

        _reminderMessage = reminderSection.GetValue<string>("Message") ?? defaultReminderMessage;
        _title = reminderSection.GetValue<string>("Title") ?? defaultTitle;

        var configuredStartupMessage = reminderSection.GetValue<string>("StartupMessage");
        _startupMessage = string.IsNullOrWhiteSpace(configuredStartupMessage)
            ? $"You will receive a reminder every {_intervalMinutes} minutes."
            : configuredStartupMessage!;
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
            RemainingText = "00:00";
            return;
        }
        RemainingText = $"{(int)remaining.TotalMinutes:00}:{remaining.Seconds:00}";
    }

    private static void ShowToast(string title, string message)
    {
        try
        {
            var content = new ToastContentBuilder()
                .AddText(title)
                .AddText(message)
                .GetToastContent();

            var toast = new ToastNotification(content.GetXml());
            Microsoft.Toolkit.Uwp.Notifications.ToastNotificationManagerCompat.CreateToastNotifier().Show(toast);
        }
        catch
        {
            // Swallow exceptions to avoid crashing startup on unsupported environments
        }
    }

    private void ResetTimer()
    {
        _toastTimer.Stop();
        _toastTimer.Interval = TimeSpan.FromMinutes(_intervalMinutes).TotalMilliseconds;
        _toastTimer.Start();
        ScheduleNextReminder();
    }
}
