using MalsMerger;

try {
    Logger.Triforce("TotK Mals Merger");

    if (args.Length < 2 || args[0] is "-h" or "--help") {
        Logger.WriteLine(Const.Help, LogLevel.None);
    }

    string[]? inputFolders = null;

    if (args[0].Contains('|')) {
        if ((inputFolders = args[0].Split('|')).Any(x => !Directory.Exists(x))) {
            throw new ArgumentException("Invalid input folders!");
        }
    }
    else if (!Directory.Exists(args[0])) {
        throw new ArgumentException("Invalid input folder!");
    }

    inputFolders ??= [args[0]];

    Flags flags = Flags.Parse(args[2..]);

    Logger.Verbose = flags.Get(true, "v", "verbose");
    Logger.WriteLine("Verbose Logging Enabled", LogLevel.OK);

    if (flags.TryGet(out string? logFile, "l", "log") && logFile is not null) {
        Logger.WriteLine("Logging Enabled", LogLevel.OK);
        Logger.CreateLogFile(logFile);
    }

    string outputPath = args[1];
    Logger.WriteLine($"Registered output path: '{outputPath}'", LogLevel.OK);

    foreach (var path in inputFolders) {
        Logger.WriteLine($"Located input path: '{path}'", LogLevel.OK);
    }
}
catch (Exception ex) {
    Logger.WriteLine(ex, LogLevel.Error);
    Environment.Exit(ex.HResult);
}