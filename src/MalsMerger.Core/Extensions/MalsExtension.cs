using MalsMerger.Core.Helpers;
using MalsMerger.Core.Models;
using MessageStudio.Formats.BinaryText;
using SarcLibrary;
using Standart.Hash.xxHash;
using System.Runtime.CompilerServices;

namespace MalsMerger.Core.Extensions;

public static class MalsExtension
{
    private static readonly Dictionary<string, SarcFile> _malsFiles = [];
    private static readonly Dictionary<string, Msbt> _msbtFiles = [];

    public static bool IsEntryVanilla(this MsbtEntry entry, string label, string msbtFile, GameFile malsFile)
    {
        MsbtEntry? vanilla = GetEntry(label, msbtFile, malsFile, out ulong checksum);
        return checksum == ulong.MinValue
            ? vanilla?.Text == entry.Text && vanilla?.Attribute == entry.Attribute
            : xxHash64.ComputeHash(entry.Text + entry.Attribute) == checksum;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static MsbtEntry? GetEntry(string label, string msbtFile, GameFile malsFile, out ulong checksum)
    {
        bool isNotGameVersion = malsFile.Version == TotkConfig.Shared.Version;

        if (!_malsFiles.TryGetValue(malsFile.Name, out SarcFile? mals)) {
            byte[] buffer = ZstdHelper.Decompress(malsFile.GetVanilla()
                ?? throw new FileNotFoundException($"Could not find any version of the Mals file: '{malsFile.Name}:{malsFile.GetVanilla()}'")).ToArray();

            mals = _malsFiles[malsFile.Name] = SarcFile.FromBinary(buffer);
        }

        if (isNotGameVersion && malsFile.Version is int version) {
            string key = Path.Combine(malsFile.PreName ?? malsFile.Name, msbtFile, label).Replace('\\', '/');
            if ((checksum = MalsChecksumHelper.Shared.GetEntry(xxHash64.ComputeHash(key), (ushort)version)) != ulong.MinValue) {
                return null;
            }
        }

        if (!_msbtFiles.TryGetValue(Path.Combine(malsFile.Name, msbtFile), out Msbt? msbt)) {
            msbt = _msbtFiles[Path.Combine(malsFile.Name, msbtFile)] = Msbt.FromBinary(mals[msbtFile]);
        }

        checksum = ulong.MinValue;
        return msbt[label];
    }
}
