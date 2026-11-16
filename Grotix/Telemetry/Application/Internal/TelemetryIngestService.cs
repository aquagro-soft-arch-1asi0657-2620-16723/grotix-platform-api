using GrotixBackend.Contracts.Integration.Telemetry;
using GrotixBackend.Contracts.Integration.Hardware;
using GrotixBackend.Telemetry.Application.Internal.Alerting;
using GrotixBackend.Telemetry.Domain.Model.Entities;
using GrotixBackend.Telemetry.Domain.Model.ValueObjects;
using GrotixBackend.Telemetry.Domain.Repositories;
using GrotixBackend.Telemetry.Domain.Services;
using Microsoft.Extensions.Options;

namespace GrotixBackend.Telemetry.Application.Internal;

public sealed class TelemetryIngestService(
    ISensorRepository sensorRepository,
    ISensorReadingRepository sensorReadingRepository,
    IAlertEvaluationService alertEvaluationService,
    IDeviceHeartbeatPublisher deviceHeartbeatPublisher,
    IOptions<SensorCalibrationOptions>? calibrationOptions = null,
    IOptions<HumidityScaleOptions>? humidityScaleOptions = null) : ITelemetryIngestService
{
    private readonly SensorCalibrationOptions _calibrationOptions = calibrationOptions?.Value ?? new();
    private readonly HumidityScaleOptions _humidityScaleOptions = humidityScaleOptions?.Value ?? new();

    public async Task IngestAsync(TelemetryReceivedIntegrationEvent evt, CancellationToken cancellationToken = default)
    {
        var reading = new SensorReading
        {
            DeviceId = evt.DeviceId,
            ZoneId = evt.ZoneId,
            Temperature = NormalizeAndCalibrate(SensorTypes.AirTemperature, evt.Temperature, evt.DeviceId),
            HumidityAir = NormalizeAndCalibrate(SensorTypes.AirHumidity, evt.HumidityAir, evt.DeviceId),
            HumiditySoil = NormalizeAndCalibrate(SensorTypes.SoilMoisture, evt.HumiditySoil, evt.DeviceId),
            LightIntensity = NormalizeAndCalibrate(SensorTypes.LightIntensity, evt.LightIntensity, evt.DeviceId),
            Timestamp = evt.Timestamp.ToUniversalTime()
        };

        await sensorReadingRepository.AddAsync(reading, cancellationToken);

        var sensors = await sensorRepository.ListByZoneAsync(evt.ZoneId, cancellationToken);
        foreach (var sensor in sensors)
        {
            var value = ResolveFieldForSensor(sensor.Type, reading);
            if (value is null) continue;
            await alertEvaluationService.EvaluateAsync(sensor, value.Value, reading.Timestamp, cancellationToken);
        }

        deviceHeartbeatPublisher.Publish(new DeviceHeartbeatIntegrationEvent(evt.DeviceId, reading.Timestamp));
    }

    private double NormalizeAndCalibrate(string sensorType, double value, int deviceId)
    {
        var normalizedType = SensorTypes.Normalize(sensorType);
        var scale = _humidityScaleOptions.Resolve(deviceId, normalizedType);
        var normalizedValue = SensorMeasurementNormalizer.Normalize(normalizedType, value, scale);
        var calibration = _calibrationOptions.Sensors.FirstOrDefault(point =>
            point.DeviceId == deviceId
            && SensorTypes.Normalize(point.SensorType) == normalizedType);

        if (calibration is null)
            return normalizedValue;

        var calibratedValue = SensorReadingCalibration.ApplyOffset(
            normalizedValue,
            calibration.ObservedReferenceValue,
            calibration.ReferenceValue);

        return SensorMeasurementNormalizer.ClampCalibratedHumidity(normalizedType, normalizedValue, calibratedValue);
    }

    private static double? ResolveFieldForSensor(string sensorType, SensorReading reading) =>
        SensorTypes.Normalize(sensorType) switch
        {
            SensorTypes.AirTemperature => reading.Temperature,
            SensorTypes.AirHumidity    => reading.HumidityAir,
            SensorTypes.SoilMoisture   => reading.HumiditySoil,
            SensorTypes.LightIntensity => reading.LightIntensity,
            _ => null
        };
}
