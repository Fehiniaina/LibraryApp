using OpenTelemetry.Resources;
using OpenTelemetry.Trace;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddReverseProxy()
    .LoadFromConfig(builder.Configuration.GetSection("ReverseProxy"));

builder.Services.AddOpenTelemetry()
    .ConfigureResource(resource => resource.AddService("Library.Gateway"))
    .WithTracing(tracing =>
    {
        tracing
            .AddAspNetCoreInstrumentation()
            .AddHttpClientInstrumentation() // YARP utilise HttpClient en interne pour forwarder les requêtes
            .AddOtlpExporter(options => options.Endpoint = new Uri("http://jaeger:4317"));
    });

var app = builder.Build();

app.MapReverseProxy();

app.Run();