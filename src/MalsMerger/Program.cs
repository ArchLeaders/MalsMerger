using MalsMerger;
using MalsMerger.Core;
using MalsMerger.Core.Extensions;
using MalsMerger.Core.Helpers;
using MalsMerger.Core.Models;
using MessageStudio.Formats.BinaryText;
using SarcLibrary;

try {
    Logger.Triforce("TotK Mals Merger");

    if (args.Length < 2 || args[0] is "-h" or "--help") {
        Logger.WriteLine(Const.Help, LogLevel.None);
        return;
    }

    List<string> inputFolders = [];
    foreach (var inputFolder in args[0].Split('|')) {
        string folder = inputFolder;
        if (ValidatorExtension.IsValidMalsFolder(ref folder)) {
            inputFolders.Add(folder);
        }
    }

    if (inputFolders.Count <= 0) {
        throw new ArgumentException("Invalid input folders!");
    }

    if (inputFolders.Count <= 1) {
        Logger.WriteLine("No point merging one folder...", LogLevel.None);
        return;
    }

    Flags flags = Flags.Parse(args[2..]);

    Logger.Verbose = flags.Get(false, "v", "verbose");
    Logger.WriteLine("Verbose Logging Enabled", LogLevel.OK);

    if (flags.TryGet(out string? logFile, "l", "log") && logFile is not null) {
        Logger.WriteLine("Logging Enabled", LogLevel.Info);
        Logger.CreateLogFile(logFile);
    }

    bool merge = flags.Get(false, "m", "merge");
    if (merge) {
        Logger.WriteLine($"Merging Enabled", LogLevel.Info);
    }

    string outputFolder = args[1];
    Logger.WriteLine($"Registered output path: '{outputFolder}'", LogLevel.Info);

#if DEBUG
    if (Directory.Exists(outputFolder)) {
        Directory.Delete(outputFolder, true);
        Logger.WriteLine($"Cleared output: '{outputFolder}'", LogLevel.Info);
    }
#endif

    Logger.WriteLine($"Registered MalsCache with game version: " +
        $"{TotkConfig.Shared.GamePath.GetVersion()}", LogLevel.Info);

    foreach (var path in inputFolders) {
        Logger.WriteLine($"Located input path: '{path}'", LogLevel.Info);
    }

    MalsChangelog changelog = [];

    inputFolders.Reverse();
    foreach (var inputFolder in inputFolders) {
        IEnumerable<GameFile> malsFiles = Directory
            .EnumerateFiles(inputFolder)
            .Select(x => new GameFile(x, Path.GetDirectoryName(inputFolder)
                ?? throw new InvalidOperationException($"Invalid input folder: '{x}'")))
            .DistinctBy(x => x.PreName);

        foreach (var file in malsFiles) {
            string best = file.GetBestMatch(TotkConfig.Shared.Version)
                ?? throw new InvalidOperationException($"Could not find any version for: '{file.Name}'");
            byte[] buffer = File.ReadAllBytes(best);
            changelog.AppendMals(file, buffer);
        }
    }

    if (merge) {
        Logger.WriteLine("Merging, this may take a moment...", LogLevel.Info);

        foreach ((var mals, var changes) in changelog) {
            GameFile outputMals = new(mals, "sarc.zs", "Mals");
            string vanilla = outputMals.GetVanilla()
                ?? throw new InvalidOperationException($"Could not find vanilla file: '{outputMals.Name}'");

            Span<byte> malsData = ZstdHelper.Decompress(vanilla);
            SarcFile sarc = SarcFile.FromBinary(malsData.ToArray());
            foreach ((var msbtFile, var msbt) in changes) {
                Span<byte> msbtData = sarc[msbtFile].AsSpan();
                if (!msbtData[0..8].SequenceEqual("MsgStdBn"u8)) {
                    Logger.WriteLine($"Invalid Mals file: '{msbtFile}' (Your game dump is likely messed up)", LogLevel.Warning);
                    continue;
                }

                Msbt vanillaMsbt = Msbt.FromBinary(msbtData);
                foreach ((var label, var entry) in msbt) {
                    vanillaMsbt[label] = entry;
                }

                using MemoryStream ms = new();
                vanillaMsbt.ToBinary(ms);
                sarc[msbtFile] = ms.ToArray();
            }

            string output = Path.Combine(outputFolder, "Mals");
            Directory.CreateDirectory(output);

            string outputFile = Path.Combine(output, outputMals.Name);
            byte[] buffer = sarc.ToBinary();

            using FileStream fs = File.Create(outputFile);
            Span<byte> compressed = ZstdHelper.Compress(buffer, outputFile);
            fs.Write(compressed);
        }
    }
    else {
        Directory.CreateDirectory(outputFolder);
        using FileStream fs = File.Create(Path.Combine(outputFolder, "mals.json"));
        changelog.WriteJson(fs);
    }
}
catch (Exception ex) {
    Logger.WriteLine(ex, LogLevel.Error);
    Environment.Exit(ex.HResult);
}