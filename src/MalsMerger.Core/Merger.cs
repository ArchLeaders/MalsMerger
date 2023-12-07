using MalsMerger.Core.Extensions;
using MalsMerger.Core.Mergers;
using SarcLibrary;
using System.Runtime.CompilerServices;
using RootFilePair = System.Collections.Generic.KeyValuePair<string, string>;

namespace MalsMerger.Core;

public class Merger
{
    private readonly IEnumerable<string> _inputMods;
    private readonly string _output;
    private readonly Dictionary<string, List<RootFilePair>> _conflicts = [];
    private readonly Dictionary<string, List<string>> _unmatched = [];
    private readonly Dictionary<string, SarcFile> _merged = [];

    public Merger(IEnumerable<string> inputMods, string output)
    {
        _inputMods = inputMods;
        _output = output;
        var groups = inputMods
            .Select(x => (root: x, files: Directory.GetFiles(x)))
            .ToDictionary(x => x.root, x => x.files
                .Select(y => Path.GetRelativePath(x.root, y))
                .ToList());

        foreach (var group in groups.Keys) {
            List<RootFilePair> matches = _conflicts[group] = [];
            List<string> unmatchedValues = _unmatched[group] = [.. groups[group]];
            foreach (var value in groups[group]) {
                ProcessValueMatches(groups, ref matches, ref unmatchedValues, group, _conflicts.Keys, value);
            }
        }

        if (Logger.Verbose) {
            LogResults();
        }
    }

    public void Merge()
    {
        foreach ((var root, var files) in _unmatched) {
            foreach (var file in files) {
                CopyToOutput(root, file);
            }
        }

        foreach ((var root, var conflictingFiles) in _conflicts.Reverse()) {
            if (conflictingFiles.Count <= 0) {
                continue;
            }

            foreach ((var folder, var file) in conflictingFiles) {
                byte[] bufferA = ZstdExtension.Shared.TryDecompress(Path.Combine(root, file)).ToArray();
                byte[] bufferB = ZstdExtension.Shared.TryDecompress(Path.Combine(folder, file)).ToArray();

                if (bufferA.IsVanilla(file)) {
                    Logger.WriteLine($"'{Path.Combine(root, file)}' (source) is a vanilla file. (Skipping, conflict takes priority...)", LogLevel.Info);
                    _merged[file] = GetSarc(bufferB, file);
                    continue;
                }

                if (bufferB.IsVanilla(file)) {
                    Logger.WriteLine($"'{Path.Combine(folder, file)}' (conflict) is a vanilla file. (Skipping, source takes priority...)", LogLevel.Info);
                    _merged[file] = GetSarc(bufferA, file);
                    continue;
                }

                Logger.WriteLine($"Merging ['{Path.Combine(root, file)}' > '{Path.Combine(folder, file)}']", LogLevel.OK);
                _merged[file] = SarcMerger.Merge(
                    file, GetSarc(bufferA, file), GetSarc(bufferB, file)
                );
            }
        }

        foreach ((var file, var sarc) in _merged) {
            byte[] buffer = sarc.ToBinary();
            string output = Path.Combine(_output, "Mals", file);
            using FileStream fs = File.Create(output);
            fs.Write(ZstdExtension.Shared.TryCompress(file, buffer));
        }
    }

    private SarcFile GetSarc(byte[] buffer, string file)
    {
        if (!_merged.TryGetValue(file, out SarcFile? sarc)) {
            sarc = SarcFile.FromBinary(buffer);
        }

        return sarc;
    }

    private void LogResults()
    {
        Console.WriteLine();

        foreach ((var group, var matches) in _conflicts) {
            if (matches.Count <= 0) {
                continue;
            }

            Logger.WriteLine($"""
                Found Conflicts in: '{group}'
                   - {string.Join("\n   - ", matches)}
                """, LogLevel.Info);
        }

        Console.WriteLine();

        foreach ((var group, var matches) in _unmatched) {
            if (matches.Count <= 0) {
                continue;
            }

            Logger.WriteLine($"""
                Found Unmatched Files in: '{group}'
                   - [{string.Join("]\n   - [", matches)}]
                """, LogLevel.Info);
        }

        Console.WriteLine();
    }

    private void CopyToOutput(string root, string name)
    {
        string src = Path.Combine(root, name);
        string output = Path.Combine(_output, "Mals", name);
        Directory.CreateDirectory(Path.GetDirectoryName(output) ?? string.Empty);
        File.Copy(src, output);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static void ProcessValueMatches(
        in Dictionary<string, List<string>> groups,
        ref List<RootFilePair> result,
        ref List<string> unmatchedValues,
        string srcGroup,
        Dictionary<string, List<RootFilePair>>.KeyCollection skip,
        string value)
    {
        foreach ((var group, var values) in groups.Where(x => x.Key != srcGroup)) {
            if (values.Contains(value) && !result.Any(x => x.Value == value)) {
                if (!skip.Contains(group)) {
                    result.Add(new(group, value));
                }

                unmatchedValues.Remove(value);
            }
        }
    }
}
