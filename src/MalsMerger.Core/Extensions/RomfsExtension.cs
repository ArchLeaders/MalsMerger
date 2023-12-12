using MalsMerger.Core.Models;

namespace MalsMerger.Core.Extensions;

public static class RomfsExtension
{
    public static int GetVersion(this string romfsFolder, int @default = 100)
    {
        string regionLangMask = Path.Combine(romfsFolder, "System", "RegionLangMask.txt");
        if (File.Exists(regionLangMask)) {
            string[] lines = File.ReadAllLines(regionLangMask);
            if (lines.Length >= 3 && int.TryParse(lines[2], out int value)) {
                return value;
            }
        }

        return @default;
    }

    public static bool IsValidMalsFolder(this string romfsFolder)
    {
        return Directory.Exists(Path.Combine(romfsFolder, "Mals"));
    }

    public static IEnumerable<GameFile> GetMalsArchives(this string romfsFolder)
    {
        return Directory
            .EnumerateFiles(Path.Combine(romfsFolder, "Mals"))
            .Select(x => new GameFile(x, romfsFolder))
            .DistinctBy(x => x.NamePrefix);
    }

    public static bool IsMsbtFile(this byte[] data)
    {
        return data.AsSpan()[0..8].SequenceEqual("MsgStdBn"u8);
    }
}
