namespace DsBatteryOsd;

internal static class DiagnosticLog
{
    private static readonly string? Path = Environment.GetEnvironmentVariable("DS_BATTERY_OSD_LOG");
    private static readonly object Gate = new();

    public static void Write(string message)
    {
        if (string.IsNullOrWhiteSpace(Path)) return;
        try
        {
            lock (Gate)
                File.AppendAllText(Path, $"{DateTime.Now:O} {message}{Environment.NewLine}");
        }
        catch
        {
            // Diagnostics must never interfere with the overlay.
        }
    }
}
