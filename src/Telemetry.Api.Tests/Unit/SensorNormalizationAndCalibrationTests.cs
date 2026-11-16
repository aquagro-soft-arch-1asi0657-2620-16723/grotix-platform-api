using FluentAssertions;
using GrotixBackend.Telemetry.Domain.Model.ValueObjects;
using GrotixBackend.Telemetry.Domain.Services;
using Xunit;

namespace Telemetry.Api.Tests.Unit;

public class SensorNormalizationAndCalibrationTests
{
    // ==========================================
    // SensorMeasurementNormalizer
    // ==========================================
    [Theory]
    [InlineData(SensorTypes.AirHumidity, 0.45, 45)]
    [InlineData(SensorTypes.SoilMoisture, 0.8, 80)]
    public void Normalize_HumidityAsFraction_ReturnsPercentage(string sensorType, double value, double expected)
    {
        var result = SensorMeasurementNormalizer.Normalize(sensorType, value);
        result.Should().BeApproximately(expected, 1e-9);
    }

    [Fact]
    public void Normalize_HumidityAlreadyInPercentage_ReturnsSameValue()
    {
        var result = SensorMeasurementNormalizer.Normalize(SensorTypes.AirHumidity, 62.5);
        result.Should().Be(62.5);
    }

    [Fact]
    public void Normalize_NonHumiditySensor_ReturnsSameValue()
    {
        var result = SensorMeasurementNormalizer.Normalize(SensorTypes.AirTemperature, 0.5);
        result.Should().Be(0.5);
    }

    // ==========================================
    // SensorReadingCalibration
    // ==========================================
    [Fact]
    public void ApplyOffset_ShiftsReadingByReferenceDifference()
    {
        var result = SensorReadingCalibration.ApplyOffset(
            value: 50, observedReferenceValue: 48, referenceValue: 50);
        result.Should().Be(52);
    }

    [Theory]
    [InlineData(double.NaN, 0, 0)]
    [InlineData(0, double.PositiveInfinity, 0)]
    [InlineData(0, 0, double.NegativeInfinity)]
    public void ApplyOffset_NonFiniteInput_Throws(double value, double observed, double reference)
    {
        var act = () => SensorReadingCalibration.ApplyOffset(value, observed, reference);
        act.Should().Throw<ArgumentOutOfRangeException>();
    }
}
