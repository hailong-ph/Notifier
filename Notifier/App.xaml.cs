using System;
using System.Runtime.InteropServices;
using System.Windows.Interop;
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
        private const int HOTKEY_ID = 9000;
        private const uint MOD_WIN = 0x0008;
        private const uint MOD_CONTROL = 0x0002;
        private const uint VK_R = 0x52;
        private HwndSource? _source;

        [DllImport("user32.dll")]
        private static extern bool RegisterHotKey(IntPtr hWnd, int id, uint fsModifiers, uint vk);
        [DllImport("user32.dll")]
        private static extern bool UnregisterHotKey(IntPtr hWnd, int id);

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

            var helper = new WindowInteropHelper(window);
            _source = HwndSource.FromHwnd(helper.Handle);
            _source.AddHook(HwndHook);
            RegisterHotKey(helper.Handle, HOTKEY_ID, MOD_WIN | MOD_CONTROL, VK_R);
        }

        protected override void OnExit(ExitEventArgs e)
        {
            if (_source != null)
            {
                var handle = _source.Handle;
                UnregisterHotKey(handle, HOTKEY_ID);
                _source.RemoveHook(HwndHook);
            }
            _trayIcon?.Dispose();
            base.OnExit(e);
        }

        private IntPtr HwndHook(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
        {
            const int WM_HOTKEY = 0x0312;
            if (msg == WM_HOTKEY && wParam.ToInt32() == HOTKEY_ID)
            {
                if (Current.MainWindow is MainWindow mw)
                {
                    mw.ResetTimerFromTray();
                }
                handled = true;
            }
            return IntPtr.Zero;
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
