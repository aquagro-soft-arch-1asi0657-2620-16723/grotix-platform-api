using GrotixBackend.Telemetry.Domain.Model.ValueObjects;

namespace GrotixBackend.Telemetry.Application.Internal;

/// <summary>
/// Declares the scale in which devices report humidity.
/// Devices not listed use <see cref="DefaultScale"/>.
/// </summary>
public sealed class HumidityScaleOptions
{
    public const string SectionName = "Telemetry:HumidityScales";

    public HumidityScale DefaultScale { get; set; } = HumidityScale.Percent;

    public List<DeviceHumidityScale> Devices { get; set; } = [];

    /// <summary>
    /// Resolves the scale for a device and sensor type. An entry with a matching
    /// sensor type wins over an entry without sensor type (which applies to both
    /// humidity sensors of the device).
    /// </summary>
    public HumidityScale Resolve(int deviceId, string sensorType)
    {
        var normalizedType = SensorTypes.Normalize(sensorType);
        var deviceEntries = Devices.Where(entry => entry.DeviceId == deviceId).ToList();

        var specific = deviceEntries.FirstOrDefault(entry =>
            !string.IsNullOrWhiteSpace(entry.SensorType)
            && SensorTypes.Normalize(entry.SensorType) == normalizedType);
        if (specific is not null)
            return specific.Scale;

        var general = deviceEntries.FirstOrDefault(entry => string.IsNullOrWhiteSpace(entry.SensorType));
        return general?.Scale ?? DefaultScale;
    }
}

public sealed class DeviceHumidityScale
{
    public int DeviceId { get; set; }

    /// <summary>Optional. Empty applies to both AIR_HUMIDITY and SOIL_MOISTURE.</summary>
    public string? SensorType { get; set; }

    public HumidityScale Scale { get; set; } = HumidityScale.Percent;
}
