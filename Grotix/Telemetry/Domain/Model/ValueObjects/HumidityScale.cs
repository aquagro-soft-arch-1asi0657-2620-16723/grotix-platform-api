namespace GrotixBackend.Telemetry.Domain.Model.ValueObjects;

/// <summary>
/// Scale in which a device reports humidity values.
/// </summary>
public enum HumidityScale
{
    /// <summary>Values from 0 to 100 (the system scale).</summary>
    Percent,

    /// <summary>Values from 0 to 1.</summary>
    Fraction
}
