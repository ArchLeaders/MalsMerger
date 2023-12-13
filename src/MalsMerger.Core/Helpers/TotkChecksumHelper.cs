using MalsMerger.Core.Models;
using Standart.Hash.xxHash;
using System.Runtime.InteropServices;

namespace MalsMerger.Core.Helpers;

public static class TotkChecksumHelper
{
    private static readonly string _path = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Totk", "checksums.bin");
    private static readonly byte[] _buffer = File.ReadAllBytes(_path);

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
        Span<ulong> cast = MemoryMarshal.Cast<byte, ulong>(_buffer);
        int half = cast.Length / 2;

        int index;
        if ((index = cast[..half].BinarySearch(xxHash64.ComputeHash($"{key}#{version}"))) > -1)
        {
            return cast[half..][index];
        }
        else if ((index = cast[..half].BinarySearch(xxHash64.ComputeHash(key))) > -1)
        {
            return cast[half..][index];
        }

        return ulong.MinValue;
    }
}
