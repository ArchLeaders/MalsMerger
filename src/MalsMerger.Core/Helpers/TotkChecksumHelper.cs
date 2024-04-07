using CommunityToolkit.HighPerformance.Buffers;
using MalsMerger.Core.Models;
using Standart.Hash.xxHash;
using System.Collections.Frozen;
using System.Runtime.InteropServices;

namespace MalsMerger.Core.Helpers;

public static class TotkChecksumHelper
{
    private static readonly string _path = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Totk", "checksums.bin");
    private static readonly FrozenDictionary<ulong, ulong> _lookup;

    static TotkChecksumHelper()
    {
        using FileStream fs = File.OpenRead(_path);
        int size = Convert.ToInt32(fs.Length);
        using SpanOwner<byte> buffer = SpanOwner<byte>.Allocate(size);
        fs.Read(buffer.Span);

        Span<ulong> values = MemoryMarshal.Cast<byte, ulong>(buffer.Span);
        int count = values.Length / 2;
        Span<ulong> half = values[..count];

        Dictionary<ulong, ulong> lookup = new(count);
        for (int i = 0; i < count; i++) {
            lookup[values[i]] = half[i];
        }

        _lookup = FrozenDictionary.ToFrozenDictionary(lookup);
    }

    public static bool IsVanilla(this Span<byte> buffer, GameFile malsArchivePath)
    {
        return buffer.IsVanillaCore(malsArchivePath, Path.Combine("Mals", malsArchivePath.Name)
            .Replace('\\', '/'));
    }

    public static bool IsVanilla(this Span<byte> buffer, GameFile malsArchivePath, string msbtPath)
    {
        return buffer.IsVanillaCore(malsArchivePath, Path.Combine("Mals", malsArchivePath.Name, msbtPath)
            .Replace('\\', '/'));
    }

    private static bool IsVanillaCore(this Span<byte> buffer, GameFile malsArchivePath, string key)
    {
        return xxHash64.ComputeHash(buffer, buffer.Length) == GetChecksum(key, malsArchivePath.Version);
    }

    public static ulong GetChecksum(string key, int version)
    {
        return _lookup.TryGetValue(xxHash64.ComputeHash($"{key}#{version}"), out ulong hash) switch {
            true => hash,
            false => _lookup.TryGetValue(xxHash64.ComputeHash($"{key}#{version}"), out hash) switch {
                true => hash,
                false => ulong.MinValue
            }
        };
    }
}
