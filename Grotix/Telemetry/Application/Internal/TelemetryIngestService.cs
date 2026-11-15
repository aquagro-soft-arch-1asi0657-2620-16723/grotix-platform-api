using GrotixBackend.Contracts.Integration.Telemetry;
using GrotixBackend.Contracts.Integration.Hardware;
using GrotixBackend.Telemetry.Application.Internal.Alerting;
using GrotixBackend.Telemetry.Domain.Model.Entities;
using GrotixBackend.Telemetry.Domain.Model.ValueObjects;
using GrotixBackend.Telemetry.Domain.Repositories;
using GrotixBackend.Telemetry.Domain.Services;

namespace GrotixBackend.Telemetry.Application.Internal;

public sealed class TelemetryIngestService(
    ISensorRepository sensorRepository,
    ISensorReadingRepository sensorReadingRepository,
    IAlertEvaluationService alertEvaluationService,
    IDeviceHeartbeatPublisher deviceHeartbeatPublisher) : ITelemetryIngestService
{
    public async Task IngestAsync(TelemetryReceivedIntegrationEvent evt, CancellationToken cancellationToken = default)
    {
        var reading = new SensorReading
        {
            DeviceId = evt.DeviceId,
            ZoneId = evt.ZoneId,
            Temperature = NormalizeValue(SensorTypes.AirTemperature, evt.Temperature),
            HumidityAir = NormalizeValue(SensorTypes.AirHumidity, evt.HumidityAir),
            HumiditySoil = NormalizeValue(SensorTypes.SoilMoisture, evt.HumiditySoil),
            LightIntensity = NormalizeValue(SensorTypes.LightIntensity, evt.LightIntensity),
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

    private static double NormalizeValue(string sensorType, double value) =>
        SensorMeasurementNormalizer.Normalize(sensorType, value);

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
