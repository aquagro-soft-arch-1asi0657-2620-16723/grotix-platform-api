using GrotixBackend.Telemetry.Domain.Model.ValueObjects;

namespace GrotixBackend.Telemetry.Domain.Services;

/// <summary>
/// Converts humidity values expressed as a fraction (0-1) to percentage (0-100),
/// the scale used by the application.
/// </summary>
public static class SensorMeasurementNormalizer
{
    public static double Normalize(string sensorType, double value)
    {
        var normalizedType = SensorTypes.Normalize(sensorType);
        if (normalizedType is SensorTypes.AirHumidity or SensorTypes.SoilMoisture
            && value >= 0d && value <= 1d)
        {
            return value * 100d;
        }

        return value;
    }
}
