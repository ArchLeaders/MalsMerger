using MalsMerger;
using MalsMerger.Core;
using MalsMerger.Core.Extensions;

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

    Logger.Verbose = flags.Get(true, "v", "verbose");
    Logger.WriteLine("Verbose Logging Enabled", LogLevel.OK);

    if (flags.TryGet(out string? logFile, "l", "log") && logFile is not null) {
        Logger.WriteLine("Logging Enabled", LogLevel.Info);
        Logger.CreateLogFile(logFile);
    }

    string outputFolder = args[1];
    Logger.WriteLine($"Registered output path: '{outputFolder}'", LogLevel.Info);

#if DEBUG
    if (Directory.Exists(outputFolder)) {
        Directory.Delete(outputFolder, true);
        Logger.WriteLine($"Cleared output: '{outputFolder}'", LogLevel.Info);
    }
#endif

    foreach (var path in inputFolders) {
        Logger.WriteLine($"Located input path: '{path}'", LogLevel.Info);
    }

    Merger merger = new(inputFolders, outputFolder);
    merger.Merge();
}
catch (Exception ex) {
    Logger.WriteLine(ex, LogLevel.Error);
    Environment.Exit(ex.HResult);
}