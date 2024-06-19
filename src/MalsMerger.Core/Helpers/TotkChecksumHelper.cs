using MalsMerger.Core.Models;
using TotkCommon.Components;
using TotkCommon.Extensions;

namespace MalsMerger.Core.Helpers;

public static class TotkChecksumHelper
{
    private static readonly string _path = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "totk", "checksums.bin");
    private static readonly TotkChecksums _checksums;

    static TotkChecksumHelper()
    {
        _checksums = TotkChecksums.FromFile(_path);
    }

    public static bool IsVanilla(this Span<byte> buffer, GameFile malsArchivePath)
    {
        string name = $"Mals/{malsArchivePath.Name.ToCanonical()}";
        return _checksums.IsFileVanilla(name, buffer, malsArchivePath.Version);
    }

    public static bool IsVanilla(this Span<byte> buffer, GameFile malsArchivePath, string msbtPath)
    {
        string name = $"Mals/{malsArchivePath.Name.ToCanonical()}/{msbtPath}";
        return _checksums.IsFileVanilla(name, buffer, malsArchivePath.Version);
    }
}
