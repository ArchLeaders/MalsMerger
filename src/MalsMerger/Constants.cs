using System.Reflection;

namespace MalsMerger;

public static class Constants
{
    public static readonly string Help = $"""
        TotK Mals Merger [Version {Assembly.GetExecutingAssembly().GetName().Version?.ToString(3) ?? "Undefined"}]
        (c) Arch Leaders. MIT License

        Usage:
          <Command> [Options]

        Commands:
          
          Merge-Mods: (merge, merge-mods)
            <Input(s)> <Output-Mod-Folder>

            Merges the specified mods or changelogs and places the merged file(s) canonically in the output mod folder.

            Inputs: (Paths)
              Bar (|) seperated list of the input mod folders or changelogs. [Priority: highest to lowest <-> left to right]

              Note: This should be one argument surounded by quotes.
                    Quotation marks should NOT be around each path (see examples)
              
              Warning: Changelogs must be named using the pattern: "XXzz.Product.json"

            Output Mod Folder: (Path)
              The path to the output mod folder.

          Generate-Changelogs: (gen, gen-chlgs, gen-changelogs)
            <Input-Mod-Folders> <Output-Mod-Folder> [-f|--format]

            Generates changelogs for each specified mod and places the changelogs canonically in the output mod folder.

            Input Mod Folders: (Paths)
              Bar (|) seperated list of the input mod folders. [Priority: highest to lowest <-> left to right]

              Note: This should be one argument surounded by quotes.
                    Quotation marks should NOT be around each path (see examples)

            Output Mod Folder: (Path)
              The path to the output mod folder.

            Format JSON: (Flag)
              Format the output JSON changelog files.

        Options:
          Log File: (Path)
            Specify a path to write logs to (logging disabled by default)
          
          Verbose: (Boolean)
            Enable verbose logging

        Examples:
          gen "path/to/mod_a|path/to/mod_b" "path/to/output" --verbose
          merge "path/to/mod_a|path/to/mod_b" "path/to/output" -l "path/to/log.txt"
        """;
}
