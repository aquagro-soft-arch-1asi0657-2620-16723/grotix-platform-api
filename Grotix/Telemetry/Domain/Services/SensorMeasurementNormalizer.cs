using GrotixBackend.Telemetry.Domain.Model.ValueObjects;

namespace GrotixBackend.Telemetry.Domain.Services;

/// <summary>
/// Converts humidity values to percentage (0-100), the scale used by the application.
/// The source scale is declared per device; it is never inferred from the value,
/// because a reading such as 1 is valid both as a fraction (100%) and as a percentage (1%).
/// </summary>
public static class SensorMeasurementNormalizer
{
    public const double MinHumidityPercent = 0d;
    public const double MaxHumidityPercent = 100d;

    public static double Normalize(string sensorType, double value, HumidityScale scale)
    {
        if (IsHumidity(sensorType) && scale == HumidityScale.Fraction)
            return value * 100d;

        return value;
    }

    /// <summary>
    /// Keeps a calibrated humidity reading inside 0-100 when the reading before calibration
    /// was already in range, so the calibration offset cannot push it out of bounds.
    /// Readings that were already out of range are returned unchanged so range validation
    /// can still reject them.
    /// </summary>
    public static double ClampCalibratedHumidity(string sensorType, double uncalibratedValue, double calibratedValue)
    {
        if (!IsHumidity(sensorType) || !IsWithinHumidityRange(uncalibratedValue))
            return calibratedValue;

        return Math.Clamp(calibratedValue, MinHumidityPercent, MaxHumidityPercent);
    }

    public static bool IsHumidity(string sensorType) =>
        SensorTypes.Normalize(sensorType) is SensorTypes.AirHumidity or SensorTypes.SoilMoisture;

    private static bool IsWithinHumidityRange(double value) =>
        value is >= MinHumidityPercent and <= MaxHumidityPercent;
}
