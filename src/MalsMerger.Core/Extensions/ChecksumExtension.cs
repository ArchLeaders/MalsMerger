using Standart.Hash.xxHash;
using System.Runtime.InteropServices;

namespace MalsMerger.Core.Extensions;

public static class ChecksumExtension
{
    private static readonly string _path = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Totk", "checksums.bin");
    private static readonly byte[] _buffer = File.ReadAllBytes(_path);

    public static bool IsVanilla(this byte[] buffer, string sarcFile)
    {
        return IsVanillaCore(buffer, sarcFile, Path.Combine("Mals", sarcFile).Replace('\\', '/'));
    }

    public static bool IsVanilla(this byte[] buffer, string msbtFile, string sarcFile)
    {
        return IsVanillaCore(buffer, sarcFile, Path.Combine("Mals", sarcFile, msbtFile).Replace('\\', '/'));
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

        return xxHash64.ComputeHash(buffer) == GetChecksum(key, version);
    }

    public static ulong GetChecksum(string key, string version)
    {
        Span<ulong> cast = MemoryMarshal.Cast<byte, ulong>(_buffer);
        int half = cast.Length / 2;

        int index;
        if ((index = cast[..half].BinarySearch(xxHash64.ComputeHash($"{key}#{version}"))) > -1) {
            return cast[half..][index];
        }
        else if ((index = cast[..half].BinarySearch(xxHash64.ComputeHash(key))) > -1) {
            return cast[half..][index];
        }

        return ulong.MinValue;
    }
}
