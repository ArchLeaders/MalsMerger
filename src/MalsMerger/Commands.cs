using MalsMerger.Core;
using MalsMerger.Core.Extensions;
using MalsMerger.Core.Helpers;
using MalsMerger.Core.Models;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json;

namespace MalsMerger;

public class Commands
{
    private readonly Dictionary<string, MalsChangelog> _changelogs = [];
    private readonly string _output;

    public Commands(string[] inputs, string output, string? localization)
    {
        Directory.CreateDirectory(_output = output);

        // Reverse the inputs to match
        // left > right priority
        Array.Reverse(inputs);

        foreach (var inputFile in inputs.SelectMany(x => x.GetMalsArchives(localization))) {
            string key = localization is not null
                ? $"{localization}.Product" : inputFile.NamePrefix;

            if (!_changelogs.TryGetValue(key, out MalsChangelog? changelog)) {
                _changelogs[key] = changelog = [];
            }

            if (inputFile.NamePostfix is "json") {
                Print($"Found Changelog '{inputFile.Name}'...", LogLevel.Info);

                using FileStream readJsonChangelog = File.OpenRead(inputFile.GetPath());
                changelog.Append(
                    JsonSerializer.Deserialize(readJsonChangelog, MalsChangelogSerializerContext.Default.MalsChangelog)
                        ?? throw new InvalidOperationException("Could not parse changelog, the deserializer returned null.")
                );
            }
            else {
                Print($"Found Mals Archive '{inputFile.Name}'...", LogLevel.Info);

                changelog.Append(inputFile, File.ReadAllBytes(
                    inputFile.GetBestMatch(TotkConfig.Shared.Version))
                );
            }
        }
    }

    public void MergeMods()
    {
        foreach ((var malsArchiveFile, var changelog) in _changelogs.Select(x => (new GameFile(x.Key, "sarc.zs", "Mals"), x.Value))) {
            string malsArchivePath = malsArchiveFile.BuildOutput(_output);
            Console.WriteLine($"@{malsArchivePath}");
            using FileStream fs = File.Create(malsArchivePath);
            changelog.Build(malsArchiveFile, fs);
        }
    }

    public void GenerateChangelogs(Flags flags)
    {
        JsonSerializerOptions options = new() {
            TypeInfoResolver = MalsChangelogSerializerContext.Default,
            WriteIndented = flags.Get(false, "f", "format")
        };

        foreach ((var malsArchiveFile, var changelog) in _changelogs.Select(x => (new GameFile(x.Key, "sarc.zs", "Mals"), x.Value))) {
            string outputFolder = Path.Combine(_output, "Mals");
            Directory.CreateDirectory(outputFolder);
            using FileStream fs = File.Create(Path.Combine(outputFolder, $"{malsArchiveFile.NamePrefix}.0.json"));

#pragma warning disable IL2026 // This is safe because context
#pragma warning disable IL3050 // is provided to the options
            JsonSerializer.Serialize(fs, changelog, options);
        }
    }
}
