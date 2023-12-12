using MalsMerger.Core.Extensions;
using MalsMerger.Core.Helpers;
using MalsMerger.Core.Models;
using MessageStudio.Formats.BinaryText;
using SarcLibrary;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace MalsMerger.Core;

public class MalsChangelog : Dictionary<string, Dictionary<string, Msbt>>
{
    public MalsChangelog() { }

    /// <summary>
    /// Construct a <see cref="MalsChangelog"/> from JSON changelog data
    /// </summary>
    /// <param name="json">The raw JSON string.</param>
    public static MalsChangelog FromJson(string json)
    {
        return JsonSerializer.Deserialize<MalsChangelog>(json)
            ?? throw new InvalidOperationException(
                "Error parsing changelog, the JSON deserializer returned null");
    }

    public string ToJson() => JsonSerializer.Serialize(this, MalsChangelogSerializerContext.Default.MalsChangelog);
    public void WriteJson(Stream output) => JsonSerializer.Serialize(output, this, MalsChangelogSerializerContext.Default.MalsChangelog);

    public void AppendMals(GameFile file, byte[] buffer)
    {
        if (file.PreName is null) {
            throw new InvalidDataException($"Invalid input file: '{file.Name}'");
        }

        if (!TryGetValue(file.PreName, out Dictionary<string, Msbt>? logged)) {
            this[file.PreName] = logged = [];
        }

        if (buffer.IsVanilla(file.Name)) {
            Logger.WriteLine($"Found vanilla Mals archive '{file.Name}', skipping...", LogLevel.OK);
            return;
        }

        Span<byte> malsData = ZstdHelper.Decompress(buffer, file.Name);

        foreach ((var msbtFile, var sarcFileData) in SarcFile.FromBinary(malsData.ToArray())) {
            Span<byte> data = sarcFileData.AsSpan();
            if (sarcFileData.IsVanilla($"{file.Name}/{msbtFile}")) {
                Logger.WriteLine($"Found vanilla Mals file '{file.Name}/{msbtFile}', skipping...", LogLevel.OK);
                continue;
            }

            if (data[0..8].SequenceEqual("MsgStdBn"u8)) {
                Msbt msbt = Msbt.FromBinary(data);
                Msbt changes = logged.ContainsKey(msbtFile)
                    ? Merge(msbt, logged[msbtFile], msbtFile, file)
                    : GetChanges(msbt, msbtFile, file);

                if (changes.Count > 0) {
                    logged[msbtFile] = changes;
                    continue;
                }
            }

            Logger.WriteLine($"Invalid Mals file: '{msbtFile}'", LogLevel.Warning);
        }
    }

    private static Msbt Merge(Msbt msbtA, Msbt msbtB, string msbtFile, GameFile malsFile)
    {
        foreach ((var label, var aEntry) in msbtA) {
            if (aEntry.IsEntryVanilla(label, msbtFile, malsFile)) {
                Logger.WriteLine($"Found vanilla entry '{msbtFile}/{label}', skipping...", LogLevel.OK);
                continue;
            }

            Logger.WriteLine($"Conflicting changes in '{msbtFile}/{label}', using priority entry.", LogLevel.Info);
            msbtB[label] = aEntry;
        }

        return msbtB;
    }

    private Msbt GetChanges(Msbt msbt, string msbtFile, GameFile malsFile)
    {
        Msbt result = [];
        foreach ((var label, var entry) in msbt) {
            if (!entry.IsEntryVanilla(label, msbtFile, malsFile)) {
                Logger.WriteLine($"Logging changes in '{msbtFile}/{label}'", LogLevel.Info);
                result[label] = entry;
                continue;
            }

            Logger.WriteLine($"Found vanilla entry '{msbtFile}/{label}', skipping...", LogLevel.OK);
        }

        return result;
    }
}

[JsonSerializable(typeof(MalsChangelog))]
public partial class MalsChangelogSerializerContext : JsonSerializerContext
{

}