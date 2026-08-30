using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Interop;
using System.Windows.Media;

namespace DsBatteryOsd;

internal sealed class OverlayWindow : Window
{
    private const int GwlExStyle = -20;
    private const int WsExTransparent = 0x20;
    private const int WsExToolWindow = 0x80;
    private const int WsExNoActivate = 0x08000000;

    private readonly TextBlock _text;
    private readonly DualSenseMonitor _monitor = new();

    public OverlayWindow()
    {
        Title = "DS Battery OSD";
        Width = 150;
        Height = 48;
        WindowStyle = WindowStyle.None;
        ResizeMode = ResizeMode.NoResize;
        AllowsTransparency = true;
        Background = Brushes.Transparent;
        Topmost = true;
        ShowInTaskbar = false;
        ShowActivated = false;
        Visibility = Visibility.Hidden;

        _text = new TextBlock
        {
            Foreground = Brushes.White,
            FontFamily = new FontFamily("Segoe UI Semibold"),
            FontSize = 22,
            VerticalAlignment = VerticalAlignment.Center,
            HorizontalAlignment = HorizontalAlignment.Center
        };

        TextOptions.SetTextFormattingMode(_text, TextFormattingMode.Display);

        Content = new Border
        {
            Background = Brushes.Transparent,
            Padding = new Thickness(12, 5, 12, 6),
            Child = _text
        };

        SourceInitialized += (_, _) => MakeClickThrough();
        Loaded += (_, _) => PlaceAtTopRight();
        SystemParameters.StaticPropertyChanged += (_, _) => PlaceAtTopRight();
        _monitor.StateChanged += OnStateChanged;
        _monitor.Start();
        Closed += (_, _) => _monitor.Dispose();
    }

    private void OnStateChanged(BatteryState? state) => Dispatcher.Invoke(() =>
    {
        if (state is null)
        {
            Hide();
            return;
        }

        var battery = state.Value;
        _text.Text = battery.ChargeState switch
        {
            ChargeState.Full => "⚡ DS 100%",
            ChargeState.Charging => $"⚡ DS {battery.Percent}%",
            _ => $"🎮 DS {battery.Percent}%"
        };
        _text.Foreground = battery.Percent <= 10 && !battery.IsCharging
            ? Brushes.OrangeRed
            : Brushes.White;
        PlaceAtTopRight();
        Show();
    });

    private void PlaceAtTopRight()
    {
        var area = SystemParameters.WorkArea;
        Left = area.Right - Width - 20;
        Top = area.Top + 20;
    }

    private void MakeClickThrough()
    {
        var handle = new WindowInteropHelper(this).Handle;
        var style = GetWindowLongPtr(handle, GwlExStyle).ToInt64();
        SetWindowLongPtr(handle, GwlExStyle,
            new IntPtr(style | WsExTransparent | WsExToolWindow | WsExNoActivate));
    }

    [DllImport("user32.dll", EntryPoint = "GetWindowLongPtrW")]
    private static extern IntPtr GetWindowLongPtr(IntPtr window, int index);

    [DllImport("user32.dll", EntryPoint = "SetWindowLongPtrW")]
    private static extern IntPtr SetWindowLongPtr(IntPtr window, int index, IntPtr value);
}
