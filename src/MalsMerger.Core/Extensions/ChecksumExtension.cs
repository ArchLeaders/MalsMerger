using MalsMerger.Core.Models;
using Standart.Hash.xxHash;

namespace MalsMerger.Core.Extensions;

public static class ChecksumExtension
{
    private static readonly Dictionary<string, ChecksumArchive> _archives = [];

    public static bool IsVanilla(this byte[] buffer, string file, string archive)
    {
        string? version = null;
        IEnumerable<string> segmented = archive.Split('.').Reverse();
        foreach (string part in segmented) {
            if (int.TryParse(part, out _)) {
                version = part;
                break;
            }
        }

        if (version is null) {
            throw new InvalidOperationException($"Could not parse version from file name: '{archive}'");
        }

        return xxHash64.ComputeHash(buffer) == GetArchive(version)
            .Fetch(Path.Combine(archive, file).Replace('\\', '/'));
    }

    public static ChecksumArchive GetArchive(string version)
    {
        if (!_archives.TryGetValue(version, out ChecksumArchive? archive)) {
            archive = new(version);
        }

        return archive;
    }
}
