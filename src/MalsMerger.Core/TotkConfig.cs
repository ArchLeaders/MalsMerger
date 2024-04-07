using MalsMerger.Core.Extensions;
using System.Text.Json;
using System.Text.Json.Serialization;
using Totk.Common.Extensions;

namespace MalsMerger.Core;

public class TotkConfig
{
    private static readonly string _path = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Totk", "config.json");

    private static readonly Lazy<TotkConfig> _shared = new(Load);
    public static TotkConfig Shared => _shared.Value;

    public required string GamePath { get; set; }

    [JsonIgnore]
    public string ZsDicPath { get; private set; } = string.Empty;

    [JsonIgnore]
    public int Version { get; private set; }

    public static TotkConfig Load()
    {
        if (!File.Exists(_path)) {
            throw new FileNotFoundException($"""
                TotK config file not found: '{_path}'
                """);
        }

        using FileStream fs = File.OpenRead(_path);
        TotkConfig result = JsonSerializer.Deserialize(fs, TotkConfigSerializerContext.Default.TotkConfig)
            ?? throw new InvalidOperationException("""
                Error parsing TotK config: the deserialized value was null
                """);

        result.ZsDicPath = Path.Combine(result.GamePath, "Pack", "ZsDic.pack.zs");
        result.Version = result.GamePath.GetVersion();
        ZstdExtension.LoadDictionaries(result.ZsDicPath);
        ZstdExtension.CompressionLevel = 3;
        return result;
    }

    public void Save()
    {
        Directory.CreateDirectory(Path.GetDirectoryName(_path)!);
        using FileStream fs = File.Create(_path);
        JsonSerializer.Serialize(fs, this, TotkConfigSerializerContext.Default.TotkConfig);
    }
}

[JsonSerializable(typeof(TotkConfig))]
public partial class TotkConfigSerializerContext : JsonSerializerContext
{

}