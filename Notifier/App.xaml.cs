using System.Configuration;
using System.Data;
using System.Windows;
using Forms = System.Windows.Forms; // alias

namespace Notifier
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : System.Windows.Application
    {
        private Forms.NotifyIcon? _trayIcon;

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            // Create tray icon
            _trayIcon = new Forms.NotifyIcon
            {
                Text = "Notifier",
                Icon = System.Drawing.SystemIcons.Information,
                Visible = true
            };

            // Context menu
            var menu = new Forms.ContextMenuStrip();
            menu.Items.Add("Show", null, (_, __) => ShowMainWindow());
            menu.Items.Add("Reset Timer", null, (_, __) =>
            {
                if (Current.MainWindow is MainWindow mw)
                {
                    mw.ResetTimerFromTray();
                }
            });
            menu.Items.Add("Exit", null, (_, __) =>
            {
                _trayIcon!.Visible = false;
                _trayIcon!.Dispose();
                Shutdown();
            });
            _trayIcon.ContextMenuStrip = menu;

            // Click to show
            _trayIcon.DoubleClick += (_, __) => ShowMainWindow();

            // Create but do not show main window
            var window = new MainWindow();
            window.Hide();
        }

        private void ShowMainWindow()
        {
            if (Current.MainWindow is null)
            {
                Current.MainWindow = new MainWindow();
            }
            Current.MainWindow.Show();
            Current.MainWindow.Activate();
        }
    }
}
