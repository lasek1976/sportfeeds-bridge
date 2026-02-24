# SportFeeds MongoDB to RabbitMQ Bridge

This .NET 10 console application bridges MongoDB SportFeeds data to RabbitMQ for consumption by Node.js clients.

## Features

- ✅ Reads `FeedsMessages` collection from MongoDB
- ✅ Reads GridFS `fs.files` collection for Full messages
- ✅ Deserializes binary data using `ZipBinaryBsonSerializer` from Phoenix project
- ✅ Publishes messages to RabbitMQ using standard ProtoBuf serialization
- ✅ Background polling service for continuous processing
- ✅ Configurable via appsettings.json

## Architecture

```
MongoDB (SportFeeds DB)
    ├── FeedsMessages collection (Snapshot messages)
    └── GridFS fs.files (Full messages)
           ↓
    [ZipBinaryBsonSerializer]
           ↓
    Deserialize to DataFeedsDiff objects
           ↓
    [Standard ProtoBuf Serializer]
           ↓
    RabbitMQ (sportfeeds exchange)
           ↓
    Node.js Consumer (protobufjs)
```

## Configuration

Edit `appsettings.json`:

```json
{
  "MongoDB": {
    "ConnectionString": "mongodb://localhost:27017",
    "DatabaseName": "SportFeeds",
    "FeedsMessagesCollection": "FeedsMessages",
    "GridFSBucket": "fs"
  },
  "RabbitMQ": {
    "HostName": "localhost",
    "Port": 5672,
    "UserName": "guest",
    "Password": "guest",
    "VirtualHost": "/",
    "ExchangeName": "sportfeeds",
    "QueueName": "sportfeeds.messages",
    "RoutingKey": "feeds.snapshot"
  },
  "Processing": {
    "PublishFormat": "ProtoBuf",
    "PollingIntervalSeconds": 5,
    "BatchSize": 10
  }
}
```

## Build

```bash
cd c:\sviluppo\claude-code\sportfeeds-app\sportfeeds-bridge
dotnet restore
dotnet build
```

## Run

```bash
dotnet run
```

Or publish as self-contained:

```bash
dotnet publish -c Release -r win-x64 --self-contained
.\bin\Release\net10.0\win-x64\publish\SportFeedsBridge.exe
```

## How It Works

### 1. MongoDB Reader
- Polls MongoDB every N seconds (configurable)
- Reads latest Full messages from GridFS
- Reads Snapshot messages from FeedsMessages collection
- Uses existing `ZipBinaryBsonSerializer` to deserialize binary Body field

### 2. ProtoBuf Serializer
- Re-serializes `DataFeedsDiff` objects using **standard ProtoBuf** (not protobuf-net BCL)
- Output is compatible with `protobufjs` in Node.js
- No GZip compression (RabbitMQ handles compression if needed)
- No type metadata prefix (not needed for ProtoBuf)

### 3. RabbitMQ Publisher
- Publishes to configured exchange/queue
- Adds metadata in message headers:
  - MessageId
  - DiffType
  - Format
  - CreatedTime
  - MessageType
- Persistent delivery mode
- Topic exchange for flexible routing

## Message Format

### RabbitMQ Message Structure

```
Headers:
  - MessageId: 123456789
  - DiffType: "Snapshot"
  - Format: "ProtoBuf"
  - CreatedTime: "2026-02-11T10:30:00Z"
  - MessageType: "DataFeedsDiff"
  - ContentType: "application/protobuf"

Body:
  - Pure ProtoBuf bytes (DataFeedsDiff)
  - No GZip compression
  - No type metadata prefix
  - Standard proto3 wire format
```

## Node.js Consumer Changes

The Node.js app should:

1. **Remove MongoDB connection** - no longer needed
2. **Add RabbitMQ consumer** - connect to `sportfeeds.messages` queue
3. **Update deserializer** - receive pure ProtoBuf bytes (no GZip, no type prefix)
4. **Use existing proto schema** - `sportfeeds.proto` should work as-is

Example Node.js consumer:

```javascript
import amqp from 'amqplib';
import protobuf from 'protobufjs';

const connection = await amqp.connect('amqp://localhost');
const channel = await connection.createChannel();

await channel.assertQueue('sportfeeds.messages');

const root = await protobuf.load('proto/sportfeeds.proto');
const DataFeedsDiff = root.lookupType('sportfeeds.DataFeedsDiff');

channel.consume('sportfeeds.messages', (msg) => {
  if (msg) {
    // Deserialize pure ProtoBuf (no GZip, no type prefix)
    const dataFeedsDiff = DataFeedsDiff.decode(msg.content);

    console.log('Received message:', {
      messageId: msg.properties.headers.MessageId,
      diffType: msg.properties.headers.DiffType,
      events: dataFeedsDiff.Events.length
    });

    channel.ack(msg);
  }
});
```

## Dependencies

- MongoDB.Driver 3.3.0
- RabbitMQ.Client 7.0.0
- protobuf-net 2.4.8
- Microsoft.Extensions.Hosting 10.0.1
- Linked Phoenix.SportFeeds.Application classes

## Troubleshooting

### Build Errors
- Ensure .NET 10 SDK is installed
- Check that Phoenix project paths are correct in .csproj
- Verify all linked source files exist

### Runtime Errors
- Check MongoDB connection string
- Verify RabbitMQ is running and accessible
- Ensure collections exist in MongoDB
- Check ZipBinaryBsonSerializer compatibility with MongoDB data

### ProtoBuf Deserialization Issues
- Verify proto schema matches .NET classes
- Check that Node.js is receiving pure ProtoBuf (use message headers to verify)
- Test with a ProtoBuf viewer/inspector tool

## Next Steps

1. ✅ Build and run the bridge
2. ✅ Install RabbitMQ locally (or use Docker)
3. ✅ Update Node.js app to consume from RabbitMQ
4. ✅ Test end-to-end message flow
5. ⬜ Deploy to production environment
