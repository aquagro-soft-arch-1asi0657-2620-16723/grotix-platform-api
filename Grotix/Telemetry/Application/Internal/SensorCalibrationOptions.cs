namespace GrotixBackend.Telemetry.Application.Internal;

/// <summary>
/// Optional calibration settings per device and sensor type.
/// Reference values use the normalized system scale.
/// </summary>
public sealed class SensorCalibrationOptions
{
    public const string SectionName = "Telemetry:SensorCalibrations";

    public List<SensorCalibrationPoint> Sensors { get; set; } = [];
}

public sealed class SensorCalibrationPoint
{
    public int DeviceId { get; set; }
    public string SensorType { get; set; } = string.Empty;
    public double ObservedReferenceValue { get; set; }
    public double ReferenceValue { get; set; }
}
