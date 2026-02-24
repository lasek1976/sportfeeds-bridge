namespace SportFeedsBridge.Phoenix.Serializers;

using System.Diagnostics;
using System.IO.Compression;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using Microsoft.Extensions.Logging;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using ProtoBuf;
using ProtoBuf.Meta;

public class ZipBinaryBsonSerializer
    : IBsonSerializer
{
  private readonly ILogger<ZipBinaryBsonSerializer> _logger;
    private static readonly Dictionary<string, Type> _cachedTypes = new Dictionary<string, Type>();
    private static readonly Dictionary<Type, Func<Stream, object>> _cachedSerializer = new Dictionary<Type, Func<Stream, object>>();
    public static readonly Lazy<ZipBinaryBsonSerializer> _instance = new Lazy<ZipBinaryBsonSerializer>(() => new ZipBinaryBsonSerializer());
    private const int _typeLength = 500;
    private static bool _protoBufConfigured = false;

    private ZipBinaryBsonSerializer()
    {
        // Configure ProtoBuf type resolver (only once)
        if (!_protoBufConfigured)
        {
            ConfigureProtoBufTypeMapping();
            _protoBufConfigured = true;
        }
    }

    public static ZipBinaryBsonSerializer Instance
    {
        get { return _instance.Value; }
    }

    private static void ConfigureProtoBufTypeMapping()
    {
        // Hook into ProtoBuf's dynamic type formatting
        RuntimeTypeModel.Default.DynamicTypeFormatting += (sender, args) =>
                                                          {
                                                              if (args.FormattedName != null)
                                                              {
                                                                  // Map old GoldbetFeeds types to new Phoenix types
                                                                  var mappedType = MapProtoBufType(args.FormattedName);
                                                                  if (mappedType != args.FormattedName)
                                                                  {
                                                                      args.Type = Type.GetType(mappedType, AssemblyResolver, null);
                                                                  }
                                                              }
                                                          };
    }

    private object DeserializeBody(Type objectType, Stream sourceStream)
    {
        if (!_cachedSerializer.TryGetValue(objectType, out Func<Stream, object> deserializerFunc))
        {
            var resultParam = Expression.Parameter(objectType, "result");
            var streamVar = Expression.Parameter(typeof(Stream), "stream");
            var returnTarget = Expression.Label(objectType, "return");

            var block = Expression.Block(
                new[] { resultParam },
                Expression.Assign(resultParam,
                    Expression.Call(typeof(Serializer), "Deserialize", new Type[] { objectType }, streamVar)),
                Expression.Label(returnTarget, resultParam)
            );
            deserializerFunc = Expression.Lambda<Func<Stream, object>>(block, streamVar).Compile();
            _cachedSerializer[objectType] = deserializerFunc;
        }

        return deserializerFunc(sourceStream);
    }

    private static void WriteTypeBytes(Stream stream, object value)
    {
        var objectType = value.GetType();
        var type = new byte[_typeLength];
        Encoding.UTF8.GetBytes(objectType.AssemblyQualifiedName).CopyTo(type, 0);
        stream.Write(type, 0, type.Length);
    }

    private static Type ReadType(byte[] bytes)
    {
        var typeString = MapFeedsTypeToPhoenixType(Encoding.UTF8.GetString(bytes, 0, _typeLength));
        if (!_cachedTypes.TryGetValue(typeString, out Type type))
        {
            type = Type.GetType(typeString, AssemblyResolver, null);
            _cachedTypes[typeString] = type;
        }
#if DEBUG
        if (type == null)
        {
            Debugger.Break();
        }
#endif
        return type;
    }

    public object Deserialize(BsonDeserializationContext context, BsonDeserializationArgs args)
    {
        if (context.Reader.CurrentBsonType == BsonType.Null)
            return null;

        if (context.Reader.CurrentBsonType != BsonType.Binary)
            return BsonSerializer.Deserialize(context.Reader, args.NominalType);

        var compressedBytes = context.Reader.ReadBytes();

        if (compressedBytes.Length < _typeLength)
            return null;

        var objectType = ReadType(compressedBytes);
        var dataBytes = compressedBytes.Skip(_typeLength).ToArray();

        using var memeoryStream = new MemoryStream(dataBytes);
        using var memeoryStreamInput = new MemoryStream();
        using var cStream = new GZipStream(memeoryStream, CompressionMode.Decompress);

        var buffer = new byte[dataBytes.Length];
        var bytesRead = 0;

        while ((bytesRead = cStream.Read(buffer, 0, buffer.Length)) > 0)
        {
            memeoryStreamInput.Write(buffer, 0, bytesRead);
            buffer = new byte[dataBytes.Length];
        }

        memeoryStreamInput.Position = 0;
        // Special case for string
        if (objectType == typeof(string))
            return Encoding.UTF8.GetString(memeoryStreamInput.ToArray());
        else // General case
            return DeserializeBody(objectType, memeoryStreamInput);
    }

    public void Serialize(BsonSerializationContext context, BsonSerializationArgs args, object value)
    {
        if (value == null)
            return;

        using var memoryStream = new MemoryStream();

        //metadata
        WriteTypeBytes(memoryStream, value);
        using (var cStream = new GZipStream(memoryStream, CompressionMode.Compress))
        {
            // Special case for string optimization
            if (value is string)
            {
                var originalBytes = Encoding.UTF8.GetBytes(value as string);
                cStream.Write(originalBytes, 0, originalBytes.Length);
            }
            else // General case
            {
                Serializer.Serialize(cStream, value);
            }
        }

        context.Writer.WriteBytes(memoryStream.ToArray());
    }

    public Type ValueType
    {
        get { return typeof(object); }
    }

    private static Assembly AssemblyResolver(AssemblyName assemblyName)
    {
        assemblyName.Version = null;
        return Assembly.Load(assemblyName);
    }

    private static string MapFeedsTypeToPhoenixType(string typeString)
    {
        typeString = typeString.TrimEnd('\0');

        var parts = typeString.Split(new[] { ',' }, 2);
        if (parts.Length < 2)
            return typeString;

        var typeName = parts[0].Trim();
        var assemblyInfo = parts[1].Trim();

        // Map from Phoenix.SportFeeds.Application to SportFeedsBridge
        if (typeName.StartsWith("Phoenix.SportFeeds.Application") || assemblyInfo.Contains("Phoenix.SportFeeds.Application"))
        {
            typeName = MapTypeNamespace(typeName);

            var assemblyParts = assemblyInfo.Split(',');
            if (assemblyParts[0].Trim() == "Phoenix.SportFeeds.Application")
            {
                assemblyParts[0] = "SportFeedsBridge";
            }

            assemblyInfo = string.Join(",", assemblyParts);
            return $"{typeName}, {assemblyInfo}";
        }
        // Only map if it's from the old GoldbetFeeds.Model assembly
        else if (typeName.StartsWith("GoldbetFeeds.Model") || assemblyInfo.Contains("GoldbetFeeds.Model"))
        {
            // Use shared mapping logic
            typeName = MapTypeNamespace(typeName);

            // Fix assembly name
            var assemblyParts = assemblyInfo.Split(',');
            if (assemblyParts[0]
                        .Trim() == "GoldbetFeeds.Model")
            {
                assemblyParts[0] = "SportFeedsBridge";
            }

            assemblyInfo = string.Join(",", assemblyParts);

            return $"{typeName}, {assemblyInfo}";
        }
        else
        {
            return typeString;
        }
    }

    private static string MapProtoBufType(string typeName)
    {
        var parts = typeName.Split(',');
        var baseTypeName = parts[0].Trim();

        // Map from Phoenix.SportFeeds.Application to SportFeedsBridge
        if (baseTypeName.StartsWith("Phoenix.SportFeeds.Application") || (parts.Length > 1 && parts[1].Trim().Contains("Phoenix.SportFeeds.Application")))
        {
            baseTypeName = MapTypeNamespace(baseTypeName);

            if (parts.Length > 1 && parts[1].Trim().StartsWith("Phoenix.SportFeeds.Application"))
            {
                return $"{baseTypeName}, SportFeedsBridge";
            }
        }
        // Only map if it's from the old GoldbetFeeds.Model
        else if (baseTypeName.StartsWith("GoldbetFeeds.Model") || (parts.Length > 1 && parts[1]
                                                                                  .Trim()
                                                                                  .Contains("GoldbetFeeds.Model")))
        {
            // Use shared mapping logic
            baseTypeName = MapTypeNamespace(baseTypeName);

            // If it has assembly info, fix it
            if (parts.Length > 1 && parts[1]
                                    .Trim()
                                    .StartsWith("GoldbetFeeds.Model"))
            {
                return $"{baseTypeName}, SportFeedsBridge";
            }
        }
        else
        {
            return typeName;
        }

        return baseTypeName;
    }

    // Shared logic for mapping type namespaces
    private static string MapTypeNamespace(string typeName)
    {
        if (typeName.StartsWith("Phoenix.SportFeeds.Application.Models.Comparer"))
        {
            return typeName.Replace("Phoenix.SportFeeds.Application.Models.Comparer", "SportFeedsBridge.Phoenix.Models.Comparer");
        }
        else if (typeName.StartsWith("Phoenix.SportFeeds.Application.Models.Feeds.Diff"))
        {
            return typeName.Replace("Phoenix.SportFeeds.Application.Models.Feeds.Diff", "SportFeedsBridge.Phoenix.Models.Feeds.Diff");
        }
        else if (typeName.StartsWith("Phoenix.SportFeeds.Application.Models"))
        {
            return typeName.Replace("Phoenix.SportFeeds.Application.Models", "SportFeedsBridge.Phoenix.Models");
        }
        else if (typeName.StartsWith("GoldbetFeeds.Model.Comparer"))
        {
            return typeName.Replace("GoldbetFeeds.Model.Comparer", "SportFeedsBridge.Phoenix.Models.Comparer");
        }
        else if (typeName.Contains("Diff", StringComparison.OrdinalIgnoreCase))
        {
            return typeName.Replace("GoldbetFeeds.Model.Feeds", "SportFeedsBridge.Phoenix.Models.Feeds.Diff");
        }
        else if (typeName.StartsWith("GoldbetFeeds.Model"))
        {
            return typeName.Replace("GoldbetFeeds.Model", "SportFeedsBridge.Phoenix.Models");
        }

        return typeName;
    }
}
