namespace GrotixBackend.Telemetry.Domain.Services;

/// <summary>
/// Applies an offset correction obtained by comparing a sensor reading
/// with a known reference value.
/// </summary>
public static class SensorReadingCalibration
{
    public static double ApplyOffset(
        double value,
        double observedReferenceValue,
        double referenceValue)
    {
        if (!double.IsFinite(value))
            throw new ArgumentOutOfRangeException(nameof(value), "Reading must be finite.");
        if (!double.IsFinite(observedReferenceValue))
            throw new ArgumentOutOfRangeException(nameof(observedReferenceValue), "Observed reference must be finite.");
        if (!double.IsFinite(referenceValue))
            throw new ArgumentOutOfRangeException(nameof(referenceValue), "Known reference must be finite.");

        return value + (referenceValue - observedReferenceValue);
    }
}
