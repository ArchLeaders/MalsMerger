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

    public static IEnumerable<GameFile> GetMalsArchives(this string romfsFolder, string? localization)
    {
        if (localization is null) {
            // Return all of the Mals file to be merged
            return Directory
                .EnumerateFiles(Path.Combine(romfsFolder, "Mals"))
                .Select(x => new GameFile(x, romfsFolder))
                .OrderByDescending(x => x.Version)
                .DistinctBy(x => x.NamePrefix);
        }

        string[] malsPaths = Directory
            .GetFiles(Path.Combine(romfsFolder, "Mals"));

        if (malsPaths.Length == 0) {
            return [];
        }

        if (malsPaths.Length == 1) {
            return [
                new(malsPaths[0], romfsFolder)
            ];
        }

        string match =
            malsPaths.FirstOrDefault(x => MatchesRegion(x, localization) && MatchesLang(x, localization)) ??
            malsPaths.FirstOrDefault(x => MatchesLang(x, localization)) ??
            malsPaths.FirstOrDefault(x => MatchesRegion(x, localization)) ??
            malsPaths.First();

        return [
            new(match, romfsFolder)
        ];
    }

    public static bool IsMsbtFile(this Span<byte> data)
    {
        return data[0..8].SequenceEqual("MsgStdBn"u8);
    }

    public static bool TryParseLocalization(this string localization, out string region, out string lang)
    {
        if (localization.Length < 4) {
            region = string.Empty;
            lang = string.Empty;
            return false;
        }

        region = localization[..2];
        lang = localization[2..4];
        return true;
    }

    private static bool MatchesLang(string path, string localization)
    {
        string name = Path.GetFileName(path);
        if (!(name.TryParseLocalization(out _, out string foundLang) && localization.TryParseLocalization(out _, out string targetLang))) {
            return false;
        }

        return foundLang == targetLang;
    }

    private static bool MatchesRegion(string path, string localization)
    {
        string name = Path.GetFileName(path);
        if (!(name.TryParseLocalization(out string foundRegion, out _) && localization.TryParseLocalization(out string targetRegion, out _))) {
            return false;
        }

        return foundRegion == targetRegion;
    }
}
