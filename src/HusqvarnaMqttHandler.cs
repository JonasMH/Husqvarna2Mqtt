using HomeAssistantDiscoveryNet;
using System.Globalization;
using System.Text.Json;
using ToMqttNet;

namespace Husqvarna2Mqtt;

public class HusqvarnaMqttHandler(ILogger<HusqvarnaMqttHandler> logger, HusqvarnaAutomoverClient client, IMqttConnectionService mqttConnection) : BackgroundService
{
    private HashSet<string> _publishedDiscoveryConfigs = [];

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {

            try
            {
                await PublishUpdates();
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Failed to publish updates");

            }

            await Task.Delay(TimeSpan.FromMinutes(5), stoppingToken);
        }
    }

    private async Task PublishUpdates()
    {
        var mowers = await client.GetMowersAsync();

        foreach (var mower in mowers)
        {
            if(!_publishedDiscoveryConfigs.Contains(mower.Id))
            {
                await PublishDiscoveryDocumentAsync(mower);
                _publishedDiscoveryConfigs.Add(mower.Id);
            }

            await mqttConnection.PublishAsync(new MQTTnet.MqttApplicationMessageBuilder()
                .WithTopic(StatusTopic(mower))
                .WithRetainFlag()
                .WithPayload(JsonSerializer.SerializeToUtf8Bytes(mower.Attributes, HusqvarnaJsonContext.Default.HusqvarnaDataEntityMower))
                .Build());
        }
    }

    private string StatusTopic(HusqvarnaDataEntity<Mower> mower) => $"{mqttConnection.MqttOptions.NodeId}/status/{mower.Id}/status";
    private string HeadlightCommandTopic(HusqvarnaDataEntity<Mower> mower) => $"{mqttConnection.MqttOptions.NodeId}/{mower.Id}/headlights";

    private async Task PublishDiscoveryDocumentAsync(HusqvarnaDataEntity<Mower> mower)
    {
        var device = new MqttDiscoveryDevice
        {
            Name = mower.Attributes.System.Name,
            Manufacturer = "Husqvarna",
            Model = mower.Attributes.System.Model,
            SuggestedArea = "Yard",
            Identifiers = [mower.Attributes.System.SerialNumber.ToString()]
        };

        await mqttConnection.PublishDiscoveryDocument(new MqttSensorDiscoveryConfig()
        {
            Device = device,
            Name = $"Battery State of Charge",
            ValueTemplate = "{{ value_json.battery.batteryPercent }}",
            StateTopic = StatusTopic(mower),
            StateClass = MqttDiscoveryStateClass.Measurement,
            UniqueId = $"{mqttConnection.MqttOptions.NodeId}-{mower.Id}-battery-soc",
            UnitOfMeasurement = HomeAssistantUnits.PERCENTAGE.Value,
            DeviceClass = HomeAssistantDeviceClass.BATTERY.Value,
        });


        await mqttConnection.PublishDiscoveryDocument(new MqttLawnMowerDiscoveryConfig()
        {
            Device = device,
            Name = mower.Attributes.System.Name,
            UniqueId = $"{mqttConnection.MqttOptions.NodeId}-{mower.Id}-mower",
            ActivityStateTopic = StatusTopic(mower),
            ActivityValueTemplate = """
{% if value_json.mower.state == "ERROR" %}
error
{% elif value_json.mower.activity == "MOWING" %}
mowing
{% elif value_json.mower.activity == "STOPPED_IN_GARDEN" %}
paused
{% elif value_json.mower.activity == "CHARGING" or value_json.mower.activity == "PARKED_IN_CS" %}
docked
{% else %}
none
{% endif %}
"""
        });

        if (mower.Attributes.Capabilities.Headlights)
        {
            await mqttConnection.PublishDiscoveryDocument(new MqttSelectDiscoveryConfig()
            {
                Device = device,
                Name = "Headlights",
                UniqueId = $"{mqttConnection.MqttOptions.NodeId}-{mower.Id}-headlights",
                StateTopic = StatusTopic(mower),
                ValueTemplate = "{{ value_json.settings.headlight.mode }}",
                Options = ["ALWAYS_ON", "ALWAYS_OFF", "EVENING_ONLY", "EVENING_AND_NIGHT"],
                CommandTopic = HeadlightCommandTopic(mower)
            });
        }

        if(mower.Attributes.Capabilities.Position)
        {
            await mqttConnection.PublishDiscoveryDocument(new MqttDeviceTrackerDiscoveryConfig()
            {
                Device = device,
                Name = "Position",
                UniqueId = $"{mqttConnection.MqttOptions.NodeId}-{mower.Id}-position",
                JsonAttributesTopic = StatusTopic(mower),
                JsonAttributesTemplate = "{{ value_json.positions[0] | to_json }}",
            });
        }

    }
}
