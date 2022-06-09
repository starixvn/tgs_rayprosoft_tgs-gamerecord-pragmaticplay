using System.Text.Json.Serialization;
using Microsoft.Extensions.Options;
using Microsoft.OpenApi.Models;
using MongoDB.Messaging;
using OpenTelemetry.Exporter;
using OpenTelemetry.Instrumentation.AspNetCore;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using Tgs.GameRecord.PragmaticPlayTransferService.MessageHandler;
using Tgs.MessageQueue.MongoDB.Configuration;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Configuration
    .AddJsonFile("/config/appsettings.json", true)
    .AddEnvironmentVariables();

builder.Logging
    .AddOpenTelemetry(options =>
    {
        options.IncludeFormattedMessage = true;
        options.ParseStateValues = true;
        _ = options.AddFluentdForwardExporter(fluentOptions =>
        {
            var nodeIp = builder.Configuration.GetValue<string>("NODE_IP");

            fluentOptions.Host = nodeIp;
            fluentOptions.Tag = builder.Configuration.GetValue<string>("OTEL_SERVICE_NAME");
            fluentOptions.UseMessagePack();
        });
    });

builder.Services
    .AddRouting(options => options.LowercaseUrls = true)
    .AddControllers()
    .AddJsonOptions(options => options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter()));

builder.Services
    .AddMemoryCache();

builder.Services
    .AddGameRecordServiceCore(
        (options, sp) =>
        {
            var configuration = sp.GetRequiredService<IConfiguration>();
            configuration.GetSection("GameRecordInfo").Bind(options);
        },
        (options, sp) =>
        {
            var configuration = sp.GetRequiredService<IConfiguration>();
            options.BaseUrl = configuration.GetValue<Uri>("GameRecord:GameProvidersBaseUrl");
        },
        (options, sp) =>
        {
            var configuration = sp.GetRequiredService<IConfiguration>();
            options.BaseUrl = configuration.GetValue<Uri>("GameRecord:GameProvidersQueryServerBaseUrl");
        },
        (options, sp) =>
        {
            var configuration = sp.GetRequiredService<IConfiguration>();
            options.BaseUrl = configuration.GetValue<Uri>("GameRecord:GameRecordSchedulesBaseUrl");
        });

builder.Services
    .AddMessageQueue()
    .AddRabbitMessageQueue(config =>
    {
        _ = config
            .ConfigQueueOptions((options, sp) =>
            {
                var configuration = sp.GetRequiredService<IConfiguration>();
                configuration.GetSection("RabbitMQ").Bind(options);

                options.OnSetupQueueAndExchange = channel =>
                {
                    _ = channel.QueueDeclare(
                        "gp.gamerecord.pptf",
                        true,
                        false,
                        false,
                        new Dictionary<string, object>());

                    channel.QueueBind(
                        "gp.gamerecord.pptf",
                        "amq.direct",
                        "tgs.gp.gamerecord.pptf",
                        new Dictionary<string, object>());
                };
            })
            .AddHandler<ScheduleMessageHandler>("gp.gamerecord.pptf", false);
    })
    .AddNatsMessageQueue(natsConfig =>
    {
        _ = natsConfig
            .ConfigQueueOptions((options, sp) =>
            {
                var configuration = sp.GetRequiredService<IConfiguration>();
                options.Url = configuration.GetConnectionString("Nats");
            })
            .AddProcessor<ManualMessageHandler>(
                "gp.gamerecord.manual.pptf",
                "gp.gamerecord.manual");
    })
    .AddMongoMessageQueue(config =>
    {
        _ = config
            .DefineMessageQueue(
                "gp.gamerecord.database",
                sp => new MongoMessageQueueBuilderOptions
                {
                    ConnectionString =
                        sp.GetRequiredService<IConfiguration>().GetConnectionString("MongoMessaging"),
                    MessageQueue = MessageQueue.Default
                })
            .DeclareQueue("tgs.gp.gamerecord.database.round.pptf");
    })
    .ClearAllExchanges()
    .AddMongoGlobPatternExchange(
        "tgs.gp.gamerecord.database.round.pptf",
        "gp.gamerecord.database",
        "tgs.gp.gamerecord.database.round.pptf");

builder.Services
    .AddSingleton<IConfigureOptions<OtlpExporterOptions>>(
        sp => new ConfigureNamedOptions<OtlpExporterOptions>(
            Options.DefaultName,
            exporterOptions =>
            {
                var config = sp.GetRequiredService<IConfiguration>();
                var nodeIp = config.GetValue<string>("NODE_IP");

                if (!string.IsNullOrWhiteSpace(nodeIp))
                    exporterOptions.Endpoint = new Uri($"http://{nodeIp}:4317");

                config.GetSection("Opentelemetry:Tracing").Bind(exporterOptions);
            }))
    .AddOpenTelemetryTracing(traceBuilder => traceBuilder
        .SetResourceBuilder(ResourceBuilder.CreateDefault().AddEnvironmentVariableDetector())
        .AddSource("Tgs.*")
        .AddSource("TGS.*")
        .AddAspNetCoreInstrumentation()
        .AddHttpClientInstrumentation()
        .AddProfileViewExporter()
        .AddOtlpExporter())
    .AddProfileViewer()
    .Configure<AspNetCoreInstrumentationOptions>(options =>
    {
        options.Filter = httpContext =>
        {
            return !httpContext.Request.Path.HasValue
                   || !httpContext.Request.Path.StartsWithSegments("/swagger")
                   && !httpContext.Request.Path.StartsWithSegments("/healthz")
                   && !httpContext.Request.Path.StartsWithSegments("/profiler/")
                   && !new[] { "html", "ico", "png", "jpg" }.Any(
                       ext => httpContext.Request.Path.Value.EndsWith($".{ext}"));
        };
    });

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddSwaggerGen(options =>
{
    var xmlFileNames = Directory.GetFiles(AppDomain.CurrentDomain.BaseDirectory, "*.xml");
    foreach (var xmlFileName in xmlFileNames)
    {
        options.IncludeXmlComments(xmlFileName);
    }

    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "PragmaticPlayTransfer GameRecord Service",
        Version = "v1"
    });
});

builder.Services.AddHealthChecks();

var app = builder.Build();

// Configure the HTTP request pipeline.
app.MapControllers();
app.MapProfileViewer();

app.UseSwagger();
app.UseSwaggerUI();

app.UseHealthChecks("/healthz");

app.Run();
