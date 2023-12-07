using MalsMerger.Core.Extensions;
using SarcLibrary;

namespace MalsMerger.Core.Mergers;

public class SarcMerger
{
    public static SarcFile Merge(string sarcFileName, SarcFile sarcA, SarcFile sarcB)
    {
        foreach ((var file, var aBuffer) in sarcA) {
            if (!sarcB.ContainsKey(file)) {
                sarcB.Add(file, aBuffer);
                Logger.WriteLine($"'{sarcFileName}/{file}' (source) was not found in the conflicting archive (Skipping...)", LogLevel.OK);
                continue;
            }

            if (aBuffer.IsVanilla(file, sarcFileName)) {
                Logger.WriteLine($"'{sarcFileName}/{file}' (source) is a vanilla file. (Skipping, conflict takes priority...)", LogLevel.OK);
                continue;
            }

            if (sarcB.TryGetValue(file, out byte[]? bBuffer)) {
                if (bBuffer.IsVanilla(file, sarcFileName)) {
                    Logger.WriteLine($"'{sarcFileName}/{file}' (conflict) is a vanilla file. (Skipping, source takes priority...)", LogLevel.OK);
                    sarcB[file] = aBuffer;
                }
                else {
                    if (aBuffer.AsSpan()[..8].SequenceEqual("MsgStdBn"u8) && bBuffer.AsSpan()[..8].SequenceEqual("MsgStdBn"u8)) {
                        Logger.WriteLine($"Merging '{sarcFileName}/{file}'", LogLevel.Info);
                        sarcB[file] = MsbtMerger.Merge(aBuffer, bBuffer, file, sarcFileName);
                    }
                    else {
                        // If they aren't both MSBT files
                        // aBuffer takes priority
                        Logger.WriteLine($"Unknown file '{sarcFileName}/{file}', defaulting to priority file...", LogLevel.Warning);
                        sarcB[file] = aBuffer;
                    }
                }
            }
        }

        return sarcB;
    }
}
