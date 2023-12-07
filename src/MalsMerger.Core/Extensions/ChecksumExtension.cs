using MalsMerger.Core.Models;
using MessageStudio.Formats.BinaryText;
using Standart.Hash.xxHash;

namespace MalsMerger.Core.Extensions;

public static class ChecksumExtension
{
    private static readonly Dictionary<string, ChecksumArchive> _archives = [];

    public static bool IsVanilla(this byte[] buffer, string sarcFile)
    {
        return IsVanillaCore(buffer, sarcFile, sarcFile.Replace('\\', '/'));
    }

    public static bool IsVanilla(this byte[] buffer, string msbtFile, string sarcFile)
    {
        return IsVanillaCore(buffer, sarcFile, Path.Combine(sarcFile, msbtFile).Replace('\\', '/'));
    }

    public static bool IsEntryVanilla(this MsbtEntry entry, string msbtFile, string sarcFile, string label)
    {
        return IsVanillaCore(System.Text.Encoding.UTF8.GetBytes(entry.Text + entry.Attribute ?? string.Empty), sarcFile, Path.Combine(sarcFile, msbtFile).Replace('\\', '/') + ':' + label);
    }

    private static bool IsVanillaCore(this byte[] buffer, string sarcFile, string key)
    {
        string? version = null;
        IEnumerable<string> segmented = sarcFile.Split('.').Reverse();
        foreach (string part in segmented) {
            if (int.TryParse(part, out _)) {
                version = part;
                break;
            }
        }

        if (version is null) {
            throw new InvalidOperationException($"Could not parse version from file name: '{sarcFile}'");
        }

        return xxHash64.ComputeHash(buffer) == GetArchive(version)
            .Fetch(key);
    }

    public static ChecksumArchive GetArchive(string version)
    {
        if (!_archives.TryGetValue(version, out ChecksumArchive? archive)) {
            archive = new(version);
        }

        return archive;
    }
}
