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
    public string ZsDicPath => Path.Combine(GamePath, "Pack", "ZsDic.pack.zs");

    [JsonIgnore]
    public int Version => GamePath.GetVersion();

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
        ZstdExtension.LoadDictionaries(result.ZsDicPath);
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