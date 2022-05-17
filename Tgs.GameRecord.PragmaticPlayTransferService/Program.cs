using System.Text.Json.Serialization;
using Tgs.GameRecord.PragmaticPlayTransferService.ReceiveHandler;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Configuration
    .AddJsonFile("/config/appsettings.json", true)
    .AddEnvironmentVariables();

builder.Services
    .AddRouting(options => options.LowercaseUrls = true)
    .AddControllers()
    .AddJsonOptions(options => options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter()));

builder.Services
    .AddPragmaticPlayTransferGameRecordService(
        (options, sp) =>
        {
            var configuration = sp.GetRequiredService<IConfiguration>();
            options.BaseUrl = configuration.GetValue<Uri>("GameRecord:GameProvidersBaseUrl");
        },
        (options, sp) =>
        {
            var configuration = sp.GetRequiredService<IConfiguration>();
            options.BaseUrl = configuration.GetValue<Uri>("GameRecord:GameProvidersQueryServerBaseUrl");
        });

builder.Services
    .AddMessageQueue()
    .AddRabbitMessageQueue(config =>
    {
        _ = config.ConfigQueueOptions((options, sp) =>
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
        }).AddHandler<PragmaticPlayTransferReceiveHandler>("gp.gamerecord.pptf");
    });

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddHealthChecks();

var app = builder.Build();

// Configure the HTTP request pipeline.
app.MapControllers();

app.UseSwagger();
app.UseSwaggerUI();

app.UseHealthChecks("/healthz");

app.Run();
