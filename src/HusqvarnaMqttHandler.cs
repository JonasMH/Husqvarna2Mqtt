using HomeAssistantDiscoveryNet;
using Husqvarna2Mqtt.Models;
using MQTTnet;
using System.Text.Json;
using ToMqttNet;

namespace Husqvarna2Mqtt;

public class HusqvarnaMqttHandler(ILogger<HusqvarnaMqttHandler> logger, HusqvarnaAutomoverClient client, IMqttConnectionService mqttConnection) : BackgroundService
{
    private HashSet<Guid> _publishedDiscoveryConfigs = [];

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {

        mqttConnection.OnApplicationMessageReceived += async (sender, args) => await OnApplicationMessageReceivedAsync(sender, args);
        await mqttConnection.SubscribeAsync(new MqttTopicFilterBuilder().WithTopic($"{mqttConnection.MqttOptions.NodeId}/write/#").Build());

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

    private async Task OnApplicationMessageReceivedAsync(object? sender, MQTTnet.Client.MqttApplicationMessageReceivedEventArgs args)
    {
        try
        {
            var topic = args.ApplicationMessage.Topic;
            var topicSplit = topic.Split('/');

            var mower = Guid.Parse(topicSplit[2]);
            var command = topicSplit[3];

            switch (command)
            {
                case "action":
                    var action = topicSplit[4];
                    await HandleActionCommandAsync(mower, action, args.ApplicationMessage.ConvertPayloadToString());
                    break;
                default:
                    logger.LogWarning("Received unknown command {command} to {mower}", command, mower);
                    break;
            }

        } catch(Exception e)
        {
            logger.LogError(e, "Failed to handle message {msg} to {topic}", args.ApplicationMessage.ConvertPayloadToString(), args.ApplicationMessage.Topic);
        }

    }

    private async Task HandleActionCommandAsync(Guid mower, string action, string payload)
    {
        switch (action)
        {
            case "start":
                await client.ActionAsync(mower, new Start()
                {
                    Type = "Start",
                    Attributes = {
                        Duration = 90
                    }
                });
                break;
            case "start-in-area":
                await client.ActionAsync(mower, new StartInWorkArea()
                {
                    Type = "StartInWorkArea",
                    Attributes = {
                        Duration = 90,
                        WorkAreaId = long.Parse(payload)
                    }
                });
                break;
            case "pause":
                await client.ActionAsync(mower, new Pause()
                {
                    Type = "Pause",
                });
                break;
            case "park":
                await client.ActionAsync(mower, new Park()
                {
                    Type = "Park",
                });
                break;
            case "park-next-schedule":
                await client.ActionAsync(mower, new ParkUntilNextSchedule()
                {
                    Type = "ParkUntilNextSchedule",
                });
                break;
            case "park-further-notice":
                await client.ActionAsync(mower, new ParkUntilFurtherNotice()
                {
                    Type = "ParkUntilFurtherNotice",
                });
                break;
            case "resume-schedule":
                await client.ActionAsync(mower, new Pause()
                {
                    Type = "ResumeSchedule",
                });
                break;
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

            await client.ActionAsync(mower.Id, new Start()
            {
                Type = "Start",
                Attributes = {
                        Duration = 90
                    }
            });

            await mqttConnection.PublishAsync(new MqttApplicationMessageBuilder()
                .WithTopic(StatusTopic(mower))
                .WithRetainFlag()
                .WithPayload(JsonSerializer.SerializeToUtf8Bytes(mower.Attributes, HusqvarnaJsonContext.Default.MowerData))
                .Build());
        }
    }

    private string StatusTopic(Mower mower) => $"{mqttConnection.MqttOptions.NodeId}/status/{mower.Id}/status";
    private string HeadlightCommandTopic(Mower mower) => $"{mqttConnection.MqttOptions.NodeId}/write/{mower.Id}/headlights";
    private string StartActionCommandTopic(Mower mower) => $"{mqttConnection.MqttOptions.NodeId}/write/{mower.Id}/action/start";
    private string PauseActionCommandTopic(Mower mower) => $"{mqttConnection.MqttOptions.NodeId}/write/{mower.Id}/action/pause";
    private string ParkActionCommandTopic(Mower mower) => $"{mqttConnection.MqttOptions.NodeId}/write/{mower.Id}/action/park";
    private string ParkUntilNextScheduleActionCommandTopic(Mower mower) => $"{mqttConnection.MqttOptions.NodeId}/write/{mower.Id}/action/park-next-schedule";
    private string ParkUntilFurtherNoticeActionCommandTopic(Mower mower) => $"{mqttConnection.MqttOptions.NodeId}/write/{mower.Id}/action/park-further-notice";
    private string ResumeScheduleActionCommandTopic(Mower mower) => $"{mqttConnection.MqttOptions.NodeId}/write/{mower.Id}/action/resume-schedule";

    private async Task PublishDiscoveryDocumentAsync(Mower mower)
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

        await mqttConnection.PublishDiscoveryDocument(new MqttSensorDiscoveryConfig()
        {
            Device = device,
            Name = $"Cutting Blade Usage Time",
            ValueTemplate = "{{ value_json.statistics.cuttingBladeUsageTime }}",
            StateTopic = StatusTopic(mower),
            StateClass = MqttDiscoveryStateClass.TotalIncreasing,
            UniqueId = $"{mqttConnection.MqttOptions.NodeId}-{mower.Id}-cutting-blade-usage",
            UnitOfMeasurement = HomeAssistantUnits.TIME_SECONDS.Value,
        });

        await mqttConnection.PublishDiscoveryDocument(new MqttSensorDiscoveryConfig()
        {
            Device = device,
            Name = $"Charging Cycles",
            ValueTemplate = "{{ value_json.statistics.numberOfChargingCycles }}",
            StateTopic = StatusTopic(mower),
            StateClass = MqttDiscoveryStateClass.Total,
            UniqueId = $"{mqttConnection.MqttOptions.NodeId}-{mower.Id}-charging-cycles"
        });

        await mqttConnection.PublishDiscoveryDocument(new MqttSensorDiscoveryConfig()
        {
            Device = device,
            Name = $"Collisions",
            ValueTemplate = "{{ value_json.statistics.numberOfCollisions }}",
            StateTopic = StatusTopic(mower),
            StateClass = MqttDiscoveryStateClass.Total,
            UniqueId = $"{mqttConnection.MqttOptions.NodeId}-{mower.Id}-collisions",
        });

        await mqttConnection.PublishDiscoveryDocument(new MqttSensorDiscoveryConfig()
        {
            Device = device,
            Name = $"Charging Time",
            ValueTemplate = "{{ value_json.statistics.totalChargingTime }}",
            StateTopic = StatusTopic(mower),
            StateClass = MqttDiscoveryStateClass.Total,
            UniqueId = $"{mqttConnection.MqttOptions.NodeId}-{mower.Id}-charging-time",
            UnitOfMeasurement = HomeAssistantUnits.TIME_SECONDS.Value,
        });

        await mqttConnection.PublishDiscoveryDocument(new MqttSensorDiscoveryConfig()
        {
            Device = device,
            Name = $"Cutting Time",
            ValueTemplate = "{{ value_json.statistics.totalCuttingTime }}",
            StateTopic = StatusTopic(mower),
            StateClass = MqttDiscoveryStateClass.Total,
            UniqueId = $"{mqttConnection.MqttOptions.NodeId}-{mower.Id}-cutting-time",
            UnitOfMeasurement = HomeAssistantUnits.TIME_SECONDS.Value,
        });

        await mqttConnection.PublishDiscoveryDocument(new MqttSensorDiscoveryConfig()
        {
            Device = device,
            Name = $"Distance Driven",
            ValueTemplate = "{{ value_json.statistics.totalDrivenDistance }}",
            StateTopic = StatusTopic(mower),
            StateClass = MqttDiscoveryStateClass.Total,
            UniqueId = $"{mqttConnection.MqttOptions.NodeId}-{mower.Id}-distance-driven",
            UnitOfMeasurement = HomeAssistantUnits.LENGTH_METERS.Value,
        });

        await mqttConnection.PublishDiscoveryDocument(new MqttSensorDiscoveryConfig()
        {
            Device = device,
            Name = $"Running Time",
            ValueTemplate = "{{ value_json.statistics.totalRunningTime }}",
            StateTopic = StatusTopic(mower),
            StateClass = MqttDiscoveryStateClass.Total,
            UniqueId = $"{mqttConnection.MqttOptions.NodeId}-{mower.Id}-running-time",
            UnitOfMeasurement = HomeAssistantUnits.TIME_SECONDS.Value,
        });

        await mqttConnection.PublishDiscoveryDocument(new MqttSensorDiscoveryConfig()
        {
            Device = device,
            Name = $"Searching Time",
            ValueTemplate = "{{ value_json.statistics.totalSearchingTime }}",
            StateTopic = StatusTopic(mower),
            StateClass = MqttDiscoveryStateClass.Total,
            UniqueId = $"{mqttConnection.MqttOptions.NodeId}-{mower.Id}-searching-time",
            UnitOfMeasurement = HomeAssistantUnits.TIME_SECONDS.Value,
        });

        await mqttConnection.PublishDiscoveryDocument(new MqttSensorDiscoveryConfig()
        {
            Device = device,
            Name = $"Error Code",
            ValueTemplate = "{{ value_json.mower.errorCode }}",
            StateTopic = StatusTopic(mower),
            StateClass = MqttDiscoveryStateClass.Total,
            UniqueId = $"{mqttConnection.MqttOptions.NodeId}-{mower.Id}-error-code"
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
""",
            StartMowingCommandTopic = StartActionCommandTopic(mower),
            DockCommandTopic = ParkActionCommandTopic(mower),
            PauseCommandTopic = PauseActionCommandTopic(mower),
        });

        await mqttConnection.PublishDiscoveryDocument(new MqttButtonDiscoveryConfig()
        {
            Device = device,
            Name = $"Start",
            UniqueId = $"{mqttConnection.MqttOptions.NodeId}-{mower.Id}-start",
            CommandTopic = StartActionCommandTopic(mower),
        });

        await mqttConnection.PublishDiscoveryDocument(new MqttButtonDiscoveryConfig()
        {
            Device = device,
            Name = $"Pause",
            UniqueId = $"{mqttConnection.MqttOptions.NodeId}-{mower.Id}-pause",
            CommandTopic = PauseActionCommandTopic(mower),
        });

        await mqttConnection.PublishDiscoveryDocument(new MqttButtonDiscoveryConfig()
        {
            Device = device,
            Name = $"Park",
            UniqueId = $"{mqttConnection.MqttOptions.NodeId}-{mower.Id}-park",
            CommandTopic = ParkActionCommandTopic(mower),
        });

        await mqttConnection.PublishDiscoveryDocument(new MqttButtonDiscoveryConfig()
        {
            Device = device,
            Name = $"Park until next schedule",
            UniqueId = $"{mqttConnection.MqttOptions.NodeId}-{mower.Id}-park-next-schedule",
            CommandTopic = ParkUntilNextScheduleActionCommandTopic(mower),
        });

        await mqttConnection.PublishDiscoveryDocument(new MqttButtonDiscoveryConfig()
        {
            Device = device,
            Name = $"Park until further notice",
            UniqueId = $"{mqttConnection.MqttOptions.NodeId}-{mower.Id}-park-further-notice",
            CommandTopic = ParkUntilFurtherNoticeActionCommandTopic(mower),
        });

        await mqttConnection.PublishDiscoveryDocument(new MqttButtonDiscoveryConfig()
        {
            Device = device,
            Name = $"Resume schedule",
            UniqueId = $"{mqttConnection.MqttOptions.NodeId}-{mower.Id}-resume-schedule",
            CommandTopic = ResumeScheduleActionCommandTopic(mower),
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
