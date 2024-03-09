using System.Text.RegularExpressions;

namespace MalsMerger.Core.Models;

public partial class GameFile
{
    /// <summary>
    /// The root folder (romfs) the file belongs too.
    /// </summary>
    private readonly string _romfs;

    /// <summary>
    /// The game version if it's found in the file name.
    /// </summary>
    public int Version { get; set; }

    /// <summary>
    /// The name prefix of the <see cref="GameFile"/>.
    /// </summary>
    public string NamePrefix { get; }

    /// <summary>
    /// The name postfix of the <see cref="GameFile"/>.
    /// </summary>
    public string NamePostfix { get; }

    /// <summary>
    /// The name of the file with the default version.
    /// </summary>
    public string Name => $"{NamePrefix}.{Version}.{NamePostfix}";

    /// <summary>
    /// The folder path relative to <see cref="_romfs"/>
    /// </summary>
    public string Folder { get; }

    public GameFile(string file, string romfs)
    {
        NamePostfix = Path.GetFileName(file);
        _romfs = romfs;

        Folder = string.IsNullOrEmpty(romfs) ? string.Empty
            : Path.GetDirectoryName(Path.GetRelativePath(romfs, file)) ?? string.Empty;

        string[] trackedFileParts = VersionPattern().Split(NamePostfix);
        if (trackedFileParts.Length != 3 | !int.TryParse(trackedFileParts[1], out int version)) {

        }

        Version = version;
        NamePrefix = trackedFileParts[0];
        NamePostfix = trackedFileParts[2];
    }

    public GameFile(string name, string extension, string folder)
    {
        NamePrefix = name;
        NamePostfix = extension;
        _romfs = string.Empty;

        Folder = folder;
        Version = TotkConfig.Shared.Version;
    }

    public string BuildOutput(string outputFolder)
    {
        string folder = Path.Combine(outputFolder, Folder);
        Directory.CreateDirectory(folder);

        return Path.Combine(folder, Name);
    }

    public string GetPath()
    {
        return Path.Combine(_romfs, Folder, Name);
    }

    public string GetVanilla()
    {
        return GetVanilla(Version);
    }

    public string GetVanilla(int targetVersion)
    {
        return GetBestMatch(targetVersion, TotkConfig.Shared.GamePath);
    }

    public string GetBestMatch(int targetVersion)
    {
        return GetBestMatch(targetVersion, _romfs);
    }

    private string GetBestMatch(int targetVersion, string romfs)
    {
        Version = targetVersion;
        string defaultFile = Path.Combine(romfs, Folder, Name);

        if (File.Exists(defaultFile)) {
            return defaultFile;
        }

        string folder = Path.Combine(romfs, Folder);

        GameFile match = Directory
            .EnumerateFiles(folder, $"{NamePrefix}*{NamePostfix}")
            .Select(x => new GameFile(x, string.Empty))
            .OrderBy(x => x.Version)
            .LastOrDefault() ?? throw new FileNotFoundException(
                $"Could not find game file: '{defaultFile}'");

        Version = match.Version;
        return Path.Combine(folder, match.Name);
    }

    [GeneratedRegex("\\.([0-9]+)\\.")]
    private static partial Regex VersionPattern();
}
