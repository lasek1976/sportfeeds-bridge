using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Google.Protobuf;
using SportFeedsBridge.Configuration;
using SportFeedsBridge.Services;
using SportFeedsBridge.Phoenix.Models.Feeds.Diff;

namespace SportFeedsBridge;

class Program
{
    static async Task Main(string[] args)
    {
        Console.WriteLine("===========================================");
        Console.WriteLine("SportFeeds MongoDB to RabbitMQ Bridge");
        Console.WriteLine("===========================================");
        Console.WriteLine();

        var builder = WebApplication.CreateBuilder(args);

        // Load configuration
        builder.Configuration
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
            .AddEnvironmentVariables()
            .AddCommandLine(args);

        // Logging
        builder.Logging.ClearProviders();
        builder.Logging.AddConsole();
        builder.Logging.AddConfiguration(builder.Configuration.GetSection("Logging"));

        // Configuration sections
        builder.Services.Configure<MongoDbSettings>(
            builder.Configuration.GetSection("MongoDB"));
        builder.Services.Configure<RabbitMQSettings>(
            builder.Configuration.GetSection("RabbitMQ"));
        builder.Services.Configure<ProcessingSettings>(
            builder.Configuration.GetSection("Processing"));

        // Services
        builder.Services.AddSingleton<MongoDbReaderService>();
        builder.Services.AddSingleton<DebugProtoBufService>();
        builder.Services.AddSingleton<RabbitMQPublisherService>();
        builder.Services.AddSingleton<RabbitMQControlService>();

        // Background worker
        builder.Services.AddHostedService<BridgeWorkerService>();

        var app = builder.Build();

        // ── Read-on-demand HTTP endpoints ──────────────────────────────────────
        // Used by the Node.js admin page to fetch and display/download messages.
        // The same Phoenix → ProtobufConverter → Google.Protobuf pipeline used
        // for RabbitMQ publishing is reused here — output is JSON via JsonFormatter.

        /// GET /api/message/snapshot/{id}
        /// Fetch a FeedsMessage snapshot by integer MessageId, return as JSON.
        app.MapGet("/api/message/snapshot/{id:long}", async (long id, MongoDbReaderService mongo) =>
        {
            var message = await mongo.GetSnapshotByIdAsync(id);
            if (message?.Body is not DataFeedsDiff diff)
                return Results.NotFound($"Snapshot {id} not found or body is not DataFeedsDiff");

            var proto = ProtobufConverter.ToProtobuf(diff);
            return Results.Text(JsonFormatter.Default.Format(proto), "application/json");
        });

        /// GET /api/message/full/{id}
        /// Fetch a GridFS Full message by ObjectId hex string, return as JSON.
        app.MapGet("/api/message/full/{id}", async (string id, MongoDbReaderService mongo) =>
        {
            var (message, _) = await mongo.GetFullMessageByFileIdAsync(id);
            if (message?.Body is not DataFeedsDiff diff)
                return Results.NotFound($"GridFS file {id} not found");

            var proto = ProtobufConverter.ToProtobuf(diff);
            return Results.Text(JsonFormatter.Default.Format(proto), "application/json");
        });

        await app.RunAsync();
    }
}
