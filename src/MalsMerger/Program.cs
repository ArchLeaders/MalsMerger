using MalsMerger;

Logger.Triforce("TotK Mals Merger");

if (args.Length < 2 || args[0] is "-h" or "--help") {
    Logger.WriteLine(Const.Help, LogLevel.None);
}

Flags flags = Flags.Parse(args[2..]);

Logger.Verbose = flags.Get(true, "v", "verbose");

if (flags.TryGet(out string? logFile, "l", "log") && logFile is not null) {
    Logger.CreateLogFile(logFile);
}