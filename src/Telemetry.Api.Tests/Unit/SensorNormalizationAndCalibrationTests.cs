using FluentAssertions;
using GrotixBackend.Contracts.Integration.Telemetry;
using GrotixBackend.Telemetry.Application.Internal;
using GrotixBackend.Telemetry.Application.Internal.Alerting;
using GrotixBackend.Telemetry.Domain.Model.Entities;
using GrotixBackend.Telemetry.Domain.Model.ValueObjects;
using GrotixBackend.Telemetry.Domain.Repositories;
using GrotixBackend.Telemetry.Domain.Services;
using Microsoft.Extensions.Options;
using Moq;
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
    [InlineData(SensorTypes.SoilMoisture, 1, 100)]
    public void Normalize_FractionScale_ReturnsPercentage(string sensorType, double value, double expected)
    {
        var result = SensorMeasurementNormalizer.Normalize(sensorType, value, HumidityScale.Fraction);
        result.Should().BeApproximately(expected, 1e-9);
    }

    [Theory]
    [InlineData(62.5)]
    [InlineData(1)]
    [InlineData(0.8)]
    public void Normalize_PercentScale_ReturnsSameValueEvenBelowOne(double value)
    {
        var result = SensorMeasurementNormalizer.Normalize(SensorTypes.SoilMoisture, value, HumidityScale.Percent);
        result.Should().Be(value);
    }

    [Fact]
    public void Normalize_NonHumiditySensor_IgnoresScale()
    {
        var result = SensorMeasurementNormalizer.Normalize(SensorTypes.AirTemperature, 0.5, HumidityScale.Fraction);
        result.Should().Be(0.5);
    }

    [Theory]
    [InlineData(98, 102, 100)]
    [InlineData(1, -2, 0)]
    [InlineData(50, 53, 53)]
    public void ClampCalibratedHumidity_InRangeReading_StaysWithinZeroAndHundred(
        double uncalibrated, double calibrated, double expected)
    {
        var result = SensorMeasurementNormalizer.ClampCalibratedHumidity(
            SensorTypes.AirHumidity, uncalibrated, calibrated);
        result.Should().Be(expected);
    }

    [Fact]
    public void ClampCalibratedHumidity_ReadingAlreadyOutOfRange_IsNotHidden()
    {
        var result = SensorMeasurementNormalizer.ClampCalibratedHumidity(
            SensorTypes.SoilMoisture, uncalibratedValue: 150, calibratedValue: 153);
        result.Should().Be(153);
    }

    [Fact]
    public void ClampCalibratedHumidity_NonHumiditySensor_ReturnsCalibratedValue()
    {
        var result = SensorMeasurementNormalizer.ClampCalibratedHumidity(
            SensorTypes.AirTemperature, uncalibratedValue: 98, calibratedValue: 102);
        result.Should().Be(102);
    }

    // ==========================================
    // HumidityScaleOptions
    // ==========================================
    [Fact]
    public void Resolve_UnlistedDevice_UsesDefaultScale()
    {
        var options = new HumidityScaleOptions { DefaultScale = HumidityScale.Percent };
        options.Resolve(7, SensorTypes.SoilMoisture).Should().Be(HumidityScale.Percent);
    }

    [Fact]
    public void Resolve_DeviceEntryWithoutSensorType_AppliesToBothHumiditySensors()
    {
        var options = new HumidityScaleOptions
        {
            Devices = [new DeviceHumidityScale { DeviceId = 7, Scale = HumidityScale.Fraction }]
        };

        options.Resolve(7, SensorTypes.AirHumidity).Should().Be(HumidityScale.Fraction);
        options.Resolve(7, SensorTypes.SoilMoisture).Should().Be(HumidityScale.Fraction);
    }

    [Fact]
    public void Resolve_SensorSpecificEntry_WinsOverDeviceEntry()
    {
        var options = new HumidityScaleOptions
        {
            Devices =
            [
                new DeviceHumidityScale { DeviceId = 7, Scale = HumidityScale.Fraction },
                new DeviceHumidityScale { DeviceId = 7, SensorType = "soil_moisture", Scale = HumidityScale.Percent }
            ]
        };

        options.Resolve(7, SensorTypes.SoilMoisture).Should().Be(HumidityScale.Percent);
        options.Resolve(7, SensorTypes.AirHumidity).Should().Be(HumidityScale.Fraction);
    }

    // ==========================================
    // TelemetryIngestService: scale + calibration
    // ==========================================
    [Fact]
    public async Task IngestAsync_PercentDeviceWithLowSoilMoisture_IsNotScaledUp()
    {
        var (service, readingRepo) = CreateIngestService(new HumidityScaleOptions());

        await service.IngestAsync(Event(humidityAir: 40, humiditySoil: 0.8));

        readingRepo.Verify(r => r.AddAsync(
            It.Is<SensorReading>(s => s.HumiditySoil == 0.8 && s.HumidityAir == 40), default), Times.Once);
    }

    [Fact]
    public async Task IngestAsync_FractionDevice_ConvertsHumidityToPercentage()
    {
        var scales = new HumidityScaleOptions
        {
            Devices = [new DeviceHumidityScale { DeviceId = 1, Scale = HumidityScale.Fraction }]
        };
        var (service, readingRepo) = CreateIngestService(scales);

        await service.IngestAsync(Event(humidityAir: 0.4, humiditySoil: 0.25));

        readingRepo.Verify(r => r.AddAsync(
            It.Is<SensorReading>(s =>
                Math.Abs(s.HumidityAir - 40) < 1e-9 && Math.Abs(s.HumiditySoil - 25) < 1e-9), default), Times.Once);
    }

    [Fact]
    public async Task IngestAsync_CalibrationOffset_DoesNotPushHumidityAboveHundred()
    {
        var calibrations = new SensorCalibrationOptions
        {
            Sensors =
            [
                new SensorCalibrationPoint
                {
                    DeviceId = 1, SensorType = SensorTypes.AirHumidity,
                    ObservedReferenceValue = 50, ReferenceValue = 54
                }
            ]
        };
        var (service, readingRepo) = CreateIngestService(new HumidityScaleOptions(), calibrations);

        await service.IngestAsync(Event(humidityAir: 98, humiditySoil: 30));

        readingRepo.Verify(r => r.AddAsync(
            It.Is<SensorReading>(s => s.HumidityAir == 100), default), Times.Once);
    }

    private static TelemetryReceivedIntegrationEvent Event(double humidityAir, double humiditySoil) =>
        new(DeviceId: 1, ZoneId: 2, Temperature: 22, HumidityAir: humidityAir,
            HumiditySoil: humiditySoil, LightIntensity: 500, Timestamp: DateTime.UtcNow);

    private static (TelemetryIngestService Service, Mock<ISensorReadingRepository> ReadingRepo) CreateIngestService(
        HumidityScaleOptions scales,
        SensorCalibrationOptions? calibrations = null)
    {
        var sensorRepo = new Mock<ISensorRepository>();
        sensorRepo.Setup(r => r.ListByZoneAsync(It.IsAny<int>(), default))
                  .ReturnsAsync(new List<Sensor>());
        var readingRepo = new Mock<ISensorReadingRepository>();

        var service = new TelemetryIngestService(
            sensorRepo.Object,
            readingRepo.Object,
            new Mock<IAlertEvaluationService>().Object,
            new Mock<IDeviceHeartbeatPublisher>().Object,
            Options.Create(calibrations ?? new SensorCalibrationOptions()),
            Options.Create(scales));

        return (service, readingRepo);
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
