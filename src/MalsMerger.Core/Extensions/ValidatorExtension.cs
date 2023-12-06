namespace MalsMerger.Core.Extensions;

public static class ValidatorExtension
{
    public static bool IsValidMalsFolder(ref string path)
        => Path.GetFileName(path) == "Mals" || Directory.Exists(path = Path.Combine(path, "Mals"));
}
