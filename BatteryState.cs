namespace DsBatteryOsd;

internal readonly struct BatteryState : IEquatable<BatteryState>
{
    public BatteryState(int percent, ChargeState chargeState)
    {
        Percent = percent;
        ChargeState = chargeState;
    }

    public int Percent { get; }
    public ChargeState ChargeState { get; }
    public bool IsCharging => ChargeState is ChargeState.Charging or ChargeState.Full;

    public bool Equals(BatteryState other) => Percent == other.Percent && ChargeState == other.ChargeState;
    public override bool Equals(object? obj) => obj is BatteryState other && Equals(other);
    public override int GetHashCode() => HashCode.Combine(Percent, ChargeState);
    public static bool operator ==(BatteryState left, BatteryState right) => left.Equals(right);
    public static bool operator !=(BatteryState left, BatteryState right) => !left.Equals(right);
}

internal enum ChargeState
{
    Discharging,
    Charging,
    Full,
    Unknown
}
