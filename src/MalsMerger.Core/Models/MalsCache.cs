using MalsMerger.Core.Extensions;
using MessageStudio.Formats.BinaryText;
using SarcLibrary;
using System.Runtime.CompilerServices;

namespace MalsMerger.Core.Models;

public class MalsCache(string romfs)
{
    private readonly string _romfs = romfs;
    private readonly Dictionary<string, SarcFile> _malsFiles = [];
    private readonly Dictionary<string, Msbt> _msbtFiles = [];

    public bool CheckEntry(in MsbtEntry entry, string label, string msbtFile, string malsFile)
    {
        MsbtEntry vanilla = GetEntry(label, msbtFile, malsFile);
        return vanilla.Text == entry.Text
            && vanilla.Attribute == entry.Attribute;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private MsbtEntry GetEntry(string label, string msbtFile, string malsFile)
    {
        if (!_malsFiles.TryGetValue(malsFile, out SarcFile? mals)) {
            string malsName = Path.GetFileName(malsFile);
            byte[] buffer = ZstdExtension.Shared.TryDecompress(Path.Combine(_romfs, "Mals", malsName)).ToArray();
            mals = _malsFiles[malsFile] = SarcFile.FromBinary(buffer);
        }
        
        if (!_msbtFiles.TryGetValue(Path.Combine(malsFile, msbtFile), out Msbt? msbt)) {
            msbt = _msbtFiles[Path.Combine(malsFile, msbtFile)] = Msbt.FromBinary(mals[msbtFile]);
        }

        return msbt[label];
    }
}
