using System.Runtime.InteropServices;
using System.Text.Json;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;

namespace DsBatteryOsd;

internal sealed class OverlayWindow : Window
{
    private const int GwlExStyle = -20;
    private const int WsExToolWindow = 0x80;
    private const int WsExNoActivate = 0x08000000;
    private const double MinimumVisibleWidth = 40;
    private const double MinimumVisibleHeight = 24;

    private static readonly string PositionFilePath = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
        "DS Battery OSD",
        "window-position.json");

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
        Opacity = 0;
        IsHitTestVisible = false;

        _text = new TextBlock
        {
            Foreground = Brushes.White,
            FontFamily = new FontFamily(new Uri("pack://application:,,,/"), "./Assets/#Nunito"),
            FontWeight = FontWeights.Bold,
            FontSize = 22,
            VerticalAlignment = VerticalAlignment.Center,
            HorizontalAlignment = HorizontalAlignment.Center,
            Margin = new Thickness(12, 5, 12, 6),
            Cursor = Cursors.SizeAll
        };

        TextOptions.SetTextFormattingMode(_text, TextFormattingMode.Ideal);
        TextOptions.SetTextRenderingMode(_text, TextRenderingMode.Grayscale);
        TextOptions.SetTextHintingMode(_text, TextHintingMode.Animated);

        Content = new Grid
        {
            Background = Brushes.Transparent,
            Children = { _text }
        };
        MouseLeftButtonDown += (_, eventArgs) =>
        {
            if (eventArgs.ButtonState == MouseButtonState.Pressed)
            {
                DragMove();
                SavePosition();
            }
        };

        SourceInitialized += (_, _) => MakeClickThrough();
        Loaded += (_, _) =>
        {
            RestorePosition();
            _monitor.Start();
        };
        SystemParameters.StaticPropertyChanged += OnSystemParametersChanged;
        _monitor.StateChanged += OnStateChanged;
        Closed += (_, _) =>
        {
            SystemParameters.StaticPropertyChanged -= OnSystemParametersChanged;
            _monitor.Dispose();
        };
    }

    private void OnStateChanged(BatteryState? state) => Dispatcher.Invoke(() =>
    {
        if (state is null)
        {
            Opacity = 0;
            IsHitTestVisible = false;
            Visibility = Visibility.Hidden;
            return;
        }

        var battery = state.Value;
        _text.Text = battery.ChargeState switch
        {
            ChargeState.Full => "🎮 ⚡ FULL",
            ChargeState.Charging => $"🎮 ⚡ {battery.Percent}%",
            _ => $"🎮 {battery.Percent}%"
        };
        _text.Foreground = battery.Percent < 20
            ? Brushes.Red
            : Brushes.White;
        Visibility = Visibility.Visible;
        IsHitTestVisible = true;
        Opacity = 1;
    });

    private void PlaceAtTopRight()
    {
        var area = SystemParameters.WorkArea;
        Left = area.Right - Width - 20;
        Top = area.Top + 20;
    }

    private void RestorePosition()
    {
        try
        {
            if (File.Exists(PositionFilePath))
            {
                var saved = JsonSerializer.Deserialize<WindowPosition>(
                    File.ReadAllText(PositionFilePath));
                if (saved is not null && IsPositionVisible(saved.Left, saved.Top))
                {
                    Left = saved.Left;
                    Top = saved.Top;
                    return;
                }
            }
        }
        catch (IOException) { }
        catch (UnauthorizedAccessException) { }
        catch (JsonException) { }

        PlaceAtTopRight();
    }

    private void SavePosition()
    {
        try
        {
            var directory = Path.GetDirectoryName(PositionFilePath)!;
            Directory.CreateDirectory(directory);
            File.WriteAllText(PositionFilePath,
                JsonSerializer.Serialize(new WindowPosition(Left, Top)));
        }
        catch (IOException) { }
        catch (UnauthorizedAccessException) { }
    }

    private bool IsPositionVisible(double left, double top)
    {
        if (!double.IsFinite(left) || !double.IsFinite(top))
            return false;

        var savedBounds = new Rect(left, top, Width, Height);
        var virtualScreen = new Rect(
            SystemParameters.VirtualScreenLeft,
            SystemParameters.VirtualScreenTop,
            SystemParameters.VirtualScreenWidth,
            SystemParameters.VirtualScreenHeight);
        savedBounds.Intersect(virtualScreen);
        return savedBounds.Width >= MinimumVisibleWidth
            && savedBounds.Height >= MinimumVisibleHeight;
    }

    private void OnSystemParametersChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs eventArgs)
    {
        if (!IsLoaded || IsPositionVisible(Left, Top))
            return;

        PlaceAtTopRight();
        SavePosition();
    }

    private void MakeClickThrough()
    {
        var handle = new WindowInteropHelper(this).Handle;
        var style = GetWindowLongPtr(handle, GwlExStyle).ToInt64();
        SetWindowLongPtr(handle, GwlExStyle,
            new IntPtr(style | WsExToolWindow | WsExNoActivate));
    }

    [DllImport("user32.dll", EntryPoint = "GetWindowLongPtrW")]
    private static extern IntPtr GetWindowLongPtr(IntPtr window, int index);

    [DllImport("user32.dll", EntryPoint = "SetWindowLongPtrW")]
    private static extern IntPtr SetWindowLongPtr(IntPtr window, int index, IntPtr value);

    private sealed record WindowPosition(double Left, double Top);
}
