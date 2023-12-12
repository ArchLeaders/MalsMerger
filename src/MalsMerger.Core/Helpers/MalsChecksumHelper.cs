using System.Buffers.Binary;
using System.Reflection;
using System.Runtime.InteropServices;

namespace MalsMerger.Core.Helpers;

public class MalsChecksumHelper
{
    private static readonly Lazy<MalsChecksumHelper> _shared = new(new MalsChecksumHelper());
    public static MalsChecksumHelper Shared => _shared.Value;

    private readonly byte[] _keys;
    private readonly byte[] _versionGroupMap;
    private readonly byte[] _versionMap;
    private readonly byte[] _checksums;

    public MalsChecksumHelper()
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

    public ulong GetEntry(ulong key, ushort version)
    {
        Span<ulong> keys = MemoryMarshal.Cast<byte, ulong>(_keys);
        int versionGroupIndex = keys.BinarySearch(key);

        if (versionGroupIndex <= 0)
        {
            return ulong.MinValue;
        }

        Span<ushort> versionGroupMap = MemoryMarshal.Cast<byte, ushort>(_versionGroupMap);
        int index = versionGroupMap[versionGroupIndex];

        if (index > 0) {
            index /= 2;
        }

        Span<ushort> versionMap = MemoryMarshal.Cast<byte, ushort>(_versionMap);
        ushort lastVersion;

        while ((lastVersion = versionMap[index]) != version)
        {
            int nextIndex = index + 1;
            if (versionMap.Length == nextIndex || versionMap[nextIndex] <= lastVersion)
            {
                if (lastVersion > version)
                {
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
