namespace MalsMerger.Core;

public static class Validator
{
    public static bool IsValidMalsFolder(ref string path)
        => Path.GetFileName(path) == "Mals" || Directory.Exists(path = Path.Combine(path, "Mals"));
}
