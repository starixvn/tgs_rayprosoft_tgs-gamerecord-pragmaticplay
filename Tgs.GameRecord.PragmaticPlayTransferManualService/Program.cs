using System.Text.Json.Serialization;
using Microsoft.Extensions.Options;
using Microsoft.OpenApi.Models;
using MongoDB.Messaging;
using OpenTelemetry.Exporter;
using OpenTelemetry.Instrumentation.AspNetCore;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using Tgs.GameRecord.PragmaticPlayTransferManualService;
using Tgs.GameRecord.PragmaticPlayTransferManualService.DependencyInjection;
using Tgs.GameRecord.PragmaticPlayTransferManualService.MessageHandler;
using Tgs.GameRecord.PragmaticPlayTransferService.Core;
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
    .AddGameRecordAwcService((options, sp) =>
    {
        var configuration = sp.GetRequiredService<IConfiguration>();
        options.ConnectionString = configuration.GetConnectionString("Redis")!;
    });

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
        },
        (sp) => sp.GetRequiredService<RedisConnectionManager>().Database);

var gameRecordInfo = builder.Configuration.GetSection("GameRecordInfo").Get<GameRecordInfoOptions>();

builder.Services
    .AddMessageQueue()
    .AddNatsMessageQueue(natsConfig =>
    {
        _ = natsConfig
            .ConfigQueueOptions((options, sp) =>
            {
                var configuration = sp.GetRequiredService<IConfiguration>();
                options.Url = configuration.GetConnectionString("Nats")!;
            })
            .AddProcessor<ManualMessageHandler>("gp.gamerecord.manual.pptf", "gp.gamerecord.manual");
    })
    .AddMongoMessageQueue(config =>
    {
        _ = config
            .DefineMessageQueue(
                gameRecordInfo.MongoDefine,
                sp => new MongoMessageQueueBuilderOptions
                {
                    ConnectionString =
                        sp.GetRequiredService<IConfiguration>().GetConnectionString("MongoMessaging")!,
                    MessageQueue = MessageQueue.Default
                });
    })
    .ClearAllExchanges()
    .AddMongoGlobPatternExchange(gameRecordInfo.MongoGlobPattern, gameRecordInfo.MongoDefine, gameRecordInfo.MongoGlobPattern);

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
                   && !httpContext.Request.Path.StartsWithSegments("/metrics")
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
