using MalsMerger.Core.Models;
using MessageStudio.Formats.BinaryText;
using SarcLibrary;
using Standart.Hash.xxHash;
using System.Buffers.Binary;
using System.Reflection;
using System.Runtime.InteropServices;

namespace MalsMerger.Core.Helpers;

public static class MalsChecksumHelper
{
    private static readonly Dictionary<string, Sarc> _malsFiles = [];
    private static readonly Dictionary<string, Msbt> _msbtFiles = [];

    private static readonly byte[] _keys;
    private static readonly byte[] _versionGroupMap;
    private static readonly byte[] _versionMap;
    private static readonly byte[] _checksums;

    static MalsChecksumHelper()
    {
        using Stream stream = Assembly
            .GetExecutingAssembly()
            .GetManifestResourceStream($"MalsMerger.Core.Resources.MalsOverflow.bin")
                ?? throw new InvalidDataException(
                    "The embedded mals-overflow binary was not found.");

        Span<byte> dword = stackalloc byte[sizeof(uint)];

        stream.Read(dword);
        int versionGroupMapOffset = BinaryPrimitives.ReadInt32LittleEndian(dword);

        stream.Read(dword);
        int checksumsOffset = BinaryPrimitives.ReadInt32LittleEndian(dword);

        stream.Read(_keys = new byte[versionGroupMapOffset - sizeof(ulong)]);
        stream.Read(_versionGroupMap = new byte[(versionGroupMapOffset - sizeof(ulong)) / sizeof(ulong) * sizeof(ushort)]);
        stream.Read(_versionMap = new byte[checksumsOffset - stream.Position]);
        stream.Read(_checksums = new byte[stream.Length - checksumsOffset]);
    }

    public static bool IsVanilla(this MsbtEntry entry, GameFile malsArchiveFile, string msbtFile, string label)
    {
        MsbtEntry? vanilla = GetEntry(malsArchiveFile, msbtFile, label, out ulong checksum);
        return checksum == ulong.MinValue
            ? vanilla?.Text == entry.Text && vanilla?.Attribute == entry.Attribute
            : xxHash64.ComputeHash(entry.Text + entry.Attribute) == checksum;
    }

    private static MsbtEntry? GetEntry(GameFile malsArchiveFile, string msbtFile, string label, out ulong checksum)
    {
        checksum = ulong.MinValue;
        bool isNotGameVersion = malsArchiveFile.Version == TotkConfig.Shared.Version;

        if (!_malsFiles.TryGetValue(malsArchiveFile.Name, out Sarc? mals)) {
            Span<byte> buffer = ZstdHelper.Decompress(malsArchiveFile.GetVanilla());
            mals = _malsFiles[malsArchiveFile.Name] = Sarc.FromBinary(buffer);
        }

        if (isNotGameVersion && malsArchiveFile.Version is int version) {
            string key = Path.Combine(malsArchiveFile.NamePrefix ?? malsArchiveFile.Name, msbtFile, label).Replace('\\', '/');
            if ((checksum = GetChecksum(xxHash64.ComputeHash(key), (ushort)version)) != ulong.MinValue) {
                return null;
            }
        }

        if (!mals.TryGetValue(msbtFile, out _)) {
            return null;
        }

        string msbtPath = Path.Combine(malsArchiveFile.Name, msbtFile);
        if (!_msbtFiles.TryGetValue(msbtPath, out Msbt? msbt)) {
            msbt = _msbtFiles[msbtPath] = Msbt.FromBinary(mals[msbtFile]);
        }

        if (!msbt.TryGetValue(label, out MsbtEntry? entry)) {
            return null;
        }

        return entry;
    }

    private static ulong GetChecksum(ulong key, ushort version)
    {
        Span<ulong> keys = MemoryMarshal.Cast<byte, ulong>(_keys);
        int versionGroupIndex = keys.BinarySearch(key);

        if (versionGroupIndex <= 0) {
            return ulong.MinValue;
        }

        Span<ushort> versionGroupMap = MemoryMarshal.Cast<byte, ushort>(_versionGroupMap);
        int index = versionGroupMap[versionGroupIndex];

        if (index > 0) {
            index /= 2;
        }

        Span<ushort> versionMap = MemoryMarshal.Cast<byte, ushort>(_versionMap);
        ushort lastVersion;

        while ((lastVersion = versionMap[index]) != version) {
            int nextIndex = index + 1;
            if (versionMap.Length == nextIndex || versionMap[nextIndex] <= lastVersion) {
                if (lastVersion > version) {
                    --index;
                }

                break;
            }

            index++;
        }

        Span<ulong> checksums = MemoryMarshal.Cast<byte, ulong>(_checksums);
        return checksums[index];
    }
}
