using System.Windows;

namespace DsBatteryOsd;

public sealed class App : Application
{
    [STAThread]
    public static void Main()
    {
        var app = new App { ShutdownMode = ShutdownMode.OnMainWindowClose };
        app.Run(new OverlayWindow());
    }
}
