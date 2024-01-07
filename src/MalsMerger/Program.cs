using MalsMerger;
using MalsMerger.Core.Extensions;
using MalsMerger.Core.Helpers;
using System.Diagnostics;

Stopwatch watch = Stopwatch.StartNew();
PrintTriforce("TotK Mals Merger");

if (args.Length == 0 || args[0] is "-h" or "--help") {
    Print(Constants.Help);
    return;
}

if (args.Length < 3) {
    Print("Invalid arguments, use --help to get usage information.");
    return;
}

string command = args[0]
    .ToLower();
string[] inputs = args[1]
    .Replace("\"", string.Empty)
    .Split('|')
    .Select(x => x.TrimEnd('/').TrimEnd('\\'))
    .Where(Path.Exists)
    .ToArray();
string output = args[2];

if (inputs.Length <= 0) {
    throw new ArgumentException("No input could be found.");
}

// Parse optional flags
Flags flags = Flags.Parse(args[3..]);

// Enable verbose logging
Verbose = flags.Get(false, "v", "verbose");
Print("Verbose Logging enabled", LogLevel.OK);

// Enable log output
if (flags.TryGet(out string? logFile, "l", "log") && logFile is not null) {
    Print("Logging enabled", LogLevel.Info);
    CreateLogFile(logFile);
}

bool isMerge = command is "merge" or "merge-mods";

// Get target localization
string? localization = flags.Get<string?>(null, "t", "target", "l", "localization");
if (isMerge && localization?.TryParseLocalization(out _, out _) == false) {
    Print($"Could not parse localization target: '{localization}'", LogLevel.Error);
    return;
}

try {
    Commands commands = new(inputs, output, isMerge ? localization : null);

    if (isMerge) {
        commands.MergeMods();
    }
    else if (command is "gen" or "gen-chlgs" or "gen-changelogs") {
        commands.GenerateChangelogs(flags);
    }
    else {
        throw new ArgumentException($"Invalid command: '{command}'");
    }

    Print($"Operation completed in {watch.ElapsedMilliseconds / 100.0} seconds.", LogLevel.Info);
}
catch (Exception ex) {
    Print(ex, LogLevel.Error);

#if DEBUG
    throw;
#else
    Environment.Exit(ex.HResult);
#endif
}