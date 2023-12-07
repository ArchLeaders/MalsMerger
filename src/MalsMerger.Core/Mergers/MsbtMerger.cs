using MalsMerger.Core.Extensions;
using MessageStudio.Formats.BinaryText;

namespace MalsMerger.Core.Mergers;

public class MsbtMerger
{
    public static byte[] Merge(Span<byte> aBuffer, Span<byte> bBuffer, string msbtFile, string sarcFile)
    {
        Msbt msbtA = Msbt.FromBinary(aBuffer);
        Msbt msbtB = Msbt.FromBinary(bBuffer);

        foreach ((var label, var aEntry) in msbtA) {
            if (!msbtB.ContainsKey(label)) {
                Logger.WriteLine($"'{sarcFile}/{msbtFile}[{label}]' (source) was not found in the conflicting MSBT (Skipping...)", LogLevel.OK);
                msbtB[label] = aEntry;
                continue;
            }

            if (!aEntry.IsEntryVanilla(msbtFile, sarcFile, label)) {
                Logger.WriteLine($"'{sarcFile}/{msbtFile}[{label}]' conflict found, using priority entry.", LogLevel.Info);
                msbtB[label] = aEntry;
                continue;
            }

            Logger.WriteLine($"'{sarcFile}/{msbtFile}[{label}]' (source) is a vanilla entry. (Skipping, conflict takes priority...)", LogLevel.OK);
        }

        using MemoryStream ms = new();
        msbtB.ToBinary(ms, msbtB.Encoding, msbtB.Endian);
        return ms.ToArray();
    }
}
