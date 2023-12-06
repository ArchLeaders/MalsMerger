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
                continue;
            }

            if (aBuffer.IsVanilla(file, sarcFileName)) {
                // aBuffer is vanilla, so keep bBuffer as is
                continue;
            }

            if (sarcB.TryGetValue(file, out byte[]? bBuffer)) {
                if (bBuffer.IsVanilla(file, sarcFileName)) {
                    sarcB[file] = aBuffer;
                }
                else {
                    // both aBuffer and bBuffer are changed
                    // attempt to merge the files
                }
            }
        }

        return sarcB;
    }
}
