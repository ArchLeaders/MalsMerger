using MalsMerger.Core.Models;
using MessageStudio.Formats.BinaryText;

namespace MalsMerger.Core.Extensions;

public static class MalsCacheExtension
{
    public static MalsCache? MalsCache { get; set; }

    public static bool IsEntryVanilla(this MsbtEntry entry, string msbtFile, string malsFile, string label)
    {
        if (MalsCache is null) {
            throw new InvalidOperationException("Uninitialized MalsCache");
        }

        return MalsCache.CheckEntry(entry, label, msbtFile, malsFile);
    }
}
