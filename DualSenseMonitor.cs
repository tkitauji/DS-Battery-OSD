using Microsoft.Win32.SafeHandles;
using System.Runtime.InteropServices;

namespace DsBatteryOsd;

internal sealed class DualSenseMonitor : IDisposable
{
    private static readonly TimeSpan ReportTimeout = TimeSpan.FromSeconds(10);
    private readonly CancellationTokenSource _stop = new();
    private BatteryState? _lastState;
    private bool _started;

    public event Action<BatteryState?>? StateChanged;

    public void Start()
    {
        if (_started) return;
        _started = true;
        _ = Task.Run(() => MonitorLoopAsync(_stop.Token));
    }

    private async Task MonitorLoopAsync(CancellationToken token)
    {
        while (!token.IsCancellationRequested)
        {
            var path = HidNative.FindDualSensePath();
            if (path is null)
            {
                DiagnosticLog.Write("DualSense not found");
                Publish(null);
                await Task.Delay(1500, token).ConfigureAwait(false);
                continue;
            }

            try
            {
                await ReadDeviceAsync(path, token).ConfigureAwait(false);
            }
            catch (OperationCanceledException) when (token.IsCancellationRequested) { }
            catch (Exception error)
            {
                DiagnosticLog.Write($"Read failed: {error}");
                Publish(null);
                await Task.Delay(1000, token).ConfigureAwait(false);
            }
        }
    }

    private async Task ReadDeviceAsync(string path, CancellationToken token)
    {
        using var handle = HidNative.OpenDevice(path);
        if (handle.IsInvalid) return;

        var inputLength = HidNative.GetInputReportLength(handle);
        DiagnosticLog.Write($"Opened {path}; input report length={inputLength}");
        if (inputLength < 54) return;

        // Bluetooth starts with the short compatibility report. Reading feature
        // report 05 asks the controller for the full 0x31 input report.
        HidNative.EnableFullBluetoothReports(handle);

        using var stream = new FileStream(handle, FileAccess.Read, inputLength, true);
        var report = new byte[inputLength];
        while (!token.IsCancellationRequested)
        {
            var read = 0;
            while (read < report.Length)
            {
                using var readTimeout = CancellationTokenSource.CreateLinkedTokenSource(token);
                readTimeout.CancelAfter(ReportTimeout);

                int count;
                try
                {
                    count = await stream.ReadAsync(
                        report.AsMemory(read, report.Length - read),
                        readTimeout.Token).ConfigureAwait(false);
                }
                catch (OperationCanceledException) when (!token.IsCancellationRequested)
                {
                    throw new TimeoutException("DualSense stopped sending input reports.");
                }

                if (count == 0) throw new EndOfStreamException();
                read += count;
            }

            var state = ParseReport(report, read);
            if (state is not null)
                Publish(state);
        }
    }

    internal static BatteryState? ParseReport(byte[] report, int length)
    {
        int statusOffset = report[0] switch
        {
            0x01 when length >= 54 => 53, // USB full input report
            0x31 when length >= 55 => 54, // Bluetooth full input report
            _ => -1
        };
        if (statusOffset < 0) return null;

        var status = report[statusOffset];
        var rawLevel = status & 0x0F;
        var rawCharge = status >> 4;
        var chargeState = rawCharge switch
        {
            0x0 => ChargeState.Discharging,
            0x1 => ChargeState.Charging,
            0x2 => ChargeState.Full,
            _ => ChargeState.Unknown
        };

        // Sony's driver treats charge state 2 as authoritative. The capacity
        // nibble may retain an older bucket after charging has completed.
        var percent = rawCharge == 0x2
            ? 100
            : Math.Clamp(rawLevel * 10, 0, 100);
        return new BatteryState(percent, chargeState);
    }

    private void Publish(BatteryState? state)
    {
        if (_lastState == state) return;
        _lastState = state;
        DiagnosticLog.Write(state is null
            ? "State changed: disconnected"
            : $"State changed: {state.Value.Percent}%; {state.Value.ChargeState}");
        StateChanged?.Invoke(state);
    }

    public void Dispose()
    {
        _stop.Cancel();
        _stop.Dispose();
    }
}

internal static class HidNative
{
    private const uint DigcfPresent = 0x2;
    private const uint DigcfDeviceInterface = 0x10;
    private const uint GenericRead = 0x80000000;
    private const uint FileShareRead = 0x1;
    private const uint FileShareWrite = 0x2;
    private const uint OpenExisting = 3;
    private const uint FileFlagOverlapped = 0x40000000;
    private static readonly string[] UsbProductIds = ["pid_0ce6", "pid_0df2"];
    private static readonly string[] BluetoothProductIds = ["pid&0ce6", "pid&0df2"];

    public static string? FindDualSensePath()
    {
        HidD_GetHidGuid(out var hidGuid);
        var infoSet = SetupDiGetClassDevs(ref hidGuid, null, IntPtr.Zero, DigcfPresent | DigcfDeviceInterface);
        if (infoSet == new IntPtr(-1)) return null;
        try
        {
            for (uint index = 0; ; index++)
            {
                var data = new DeviceInterfaceData { Size = Marshal.SizeOf<DeviceInterfaceData>() };
                if (!SetupDiEnumDeviceInterfaces(infoSet, IntPtr.Zero, ref hidGuid, index, ref data)) break;
                SetupDiGetDeviceInterfaceDetail(infoSet, ref data, IntPtr.Zero, 0, out var required, IntPtr.Zero);
                var detail = Marshal.AllocHGlobal((int)required);
                try
                {
                    Marshal.WriteInt32(detail, IntPtr.Size == 8 ? 8 : 6);
                    if (!SetupDiGetDeviceInterfaceDetail(infoSet, ref data, detail, required, out _, IntPtr.Zero)) continue;
                    var path = Marshal.PtrToStringUni(detail + 4);
                    if (path is not null) DiagnosticLog.Write($"HID path: {path}");
                    if (path is not null && IsDualSensePath(path))
                        return path;
                }
                finally { Marshal.FreeHGlobal(detail); }
            }
        }
        finally { SetupDiDestroyDeviceInfoList(infoSet); }
        return null;
    }

    internal static bool IsDualSensePath(string path)
    {
        var isUsb = path.Contains("vid_054c", StringComparison.OrdinalIgnoreCase) &&
                    UsbProductIds.Any(pid => path.Contains(pid, StringComparison.OrdinalIgnoreCase));

        // Bluetooth HID paths encode Sony's vendor ID as VID&0002054C.
        var isBluetooth = path.Contains("vid&0002054c", StringComparison.OrdinalIgnoreCase) &&
                          BluetoothProductIds.Any(pid => path.Contains(pid, StringComparison.OrdinalIgnoreCase));

        return isUsb || isBluetooth;
    }

    public static SafeFileHandle OpenDevice(string path) => CreateFile(path, GenericRead,
        FileShareRead | FileShareWrite, IntPtr.Zero, OpenExisting, FileFlagOverlapped, IntPtr.Zero);

    public static ushort GetInputReportLength(SafeFileHandle handle)
    {
        if (!HidD_GetPreparsedData(handle, out var data)) return 0;
        try { return HidP_GetCaps(data, out var caps) == 0x110000 ? caps.InputReportByteLength : (ushort)0; }
        finally { HidD_FreePreparsedData(data); }
    }

    public static void EnableFullBluetoothReports(SafeFileHandle handle)
    {
        var feature = new byte[41];
        feature[0] = 0x05;
        HidD_GetFeature(handle, feature, feature.Length);
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct DeviceInterfaceData { public int Size; public Guid InterfaceClassGuid; public int Flags; public IntPtr Reserved; }

    [StructLayout(LayoutKind.Sequential)]
    private struct HidCaps
    {
        public ushort Usage, UsagePage, InputReportByteLength, OutputReportByteLength, FeatureReportByteLength;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 17)] public ushort[] Reserved;
        public ushort NumberLinkCollectionNodes, NumberInputButtonCaps, NumberInputValueCaps, NumberInputDataIndices;
        public ushort NumberOutputButtonCaps, NumberOutputValueCaps, NumberOutputDataIndices;
        public ushort NumberFeatureButtonCaps, NumberFeatureValueCaps, NumberFeatureDataIndices;
    }

    [DllImport("hid.dll")] private static extern void HidD_GetHidGuid(out Guid guid);
    [DllImport("hid.dll")] private static extern bool HidD_GetPreparsedData(SafeFileHandle device, out IntPtr data);
    [DllImport("hid.dll")] private static extern bool HidD_FreePreparsedData(IntPtr data);
    [DllImport("hid.dll")] private static extern int HidP_GetCaps(IntPtr data, out HidCaps caps);
    [DllImport("hid.dll", SetLastError = true)] private static extern bool HidD_GetFeature(SafeFileHandle device, byte[] report, int length);
    [DllImport("setupapi.dll", CharSet = CharSet.Unicode, SetLastError = true)] private static extern IntPtr SetupDiGetClassDevs(ref Guid guid, string? enumerator, IntPtr parent, uint flags);
    [DllImport("setupapi.dll", SetLastError = true)] private static extern bool SetupDiEnumDeviceInterfaces(IntPtr set, IntPtr device, ref Guid guid, uint index, ref DeviceInterfaceData data);
    [DllImport("setupapi.dll", CharSet = CharSet.Unicode, SetLastError = true)] private static extern bool SetupDiGetDeviceInterfaceDetail(IntPtr set, ref DeviceInterfaceData data, IntPtr detail, uint size, out uint required, IntPtr deviceInfo);
    [DllImport("setupapi.dll")] private static extern bool SetupDiDestroyDeviceInfoList(IntPtr set);
    [DllImport("kernel32.dll", CharSet = CharSet.Unicode, SetLastError = true)] private static extern SafeFileHandle CreateFile(string name, uint access, uint share, IntPtr security, uint creation, uint flags, IntPtr template);
}
