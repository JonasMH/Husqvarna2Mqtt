# Husqvarna2Mqtt

Husqvarna2Mqtt is a .NET application that integrates Husqvarna Automower with MQTT, allowing you to control and monitor your Automower through MQTT messages.

## Features

- Control your Husqvarna Automower via MQTT
- Monitor Automower status and metrics
- Integration with Home Assistant

## Prerequisites

- .NET 9.0 SDK
- Docker (optional, for containerized deployment)
- MQTT broker (e.g., Mosquitto)

## Development Setup

### Clone the Repository

```sh
git clone https://github.com/yourusername/Husqvarna2Mqtt.git
cd Husqvarna2Mqtt
```

### Configure User Secrets

This project uses .NET user-secrets to store sensitive information such as the Husqvarna API credentials. Follow these steps to set up user-secrets:

Add your Husqvarna API credentials:

```sh
cd src
dotnet user-secrets set "HusqvarnaAutomoverClientOptions:ClientId" "your-client-id"
dotnet user-secrets set "HusqvarnaAutomoverClientOptions:ClientSecret" "your-client-secret"
```

## Usage

Once the application is running, it will connect to your MQTT broker and start publishing Automower status and metrics. You can control the Automower by sending MQTT messages to the appropriate topics.

## Contributing

Contributions are welcome! Please open an issue or submit a pull request.

## License

This project is licensed under the MIT License. See the [LICENSE](LICENSE.txt) file for details.