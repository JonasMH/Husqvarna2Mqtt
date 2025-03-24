using Husqvarna2Mqtt;
using OpenTelemetry;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using ToMqttNet;


var builder = WebApplication.CreateBuilder(args);

builder.Logging.AddOpenTelemetry();
builder.Services.AddOpenTelemetry()
    .UseOtlpExporter()
    .ConfigureResource(resource =>
    {
        resource.AddService(serviceName: builder.Environment.ApplicationName);
    })
    .WithMetrics(metrics =>
    {
        metrics.AddPrometheusExporter();
    })
    .WithTracing(tracing =>
    {
        tracing.AddAspNetCoreInstrumentation();
        tracing.AddHttpClientInstrumentation();
    });

builder.Services.AddHealthChecks();
builder.Services.AddHttpClient();
builder.Services.AddOptions<HusqvarnaAutomoverClientOptions>().BindConfiguration(nameof(HusqvarnaAutomoverClientOptions));
builder.Services.AddSingleton<HusqvarnaAutomoverClient>();
builder.Services.AddHostedService<HusqvarnaMqttHandler>();
builder.Services.AddMqttConnection();

var app = builder.Build();

app.MapPrometheusScrapingEndpoint("/metrics");
app.MapHealthChecks("/health");

await app.RunAsync();