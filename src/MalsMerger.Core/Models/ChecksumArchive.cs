using Standart.Hash.xxHash;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace MalsMerger.Core.Models;

public class ChecksumArchive
{
    private readonly Memory<byte> _buffer;

    public ChecksumArchive(string version)
    {
        using Stream? stream = Assembly
            .GetExecutingAssembly()
            .GetManifestResourceStream(
                $"MalsMerger.Resources.{version.Replace(".", string.Empty)}")
                    ?? throw new NotSupportedException(
                        $"The game version {version} is not supported");

        byte[] buffer = new byte[stream.Length];
        stream.Read(buffer);
        _buffer = buffer;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public ulong Fetch(string key)
        => Fetch(xxHash64.ComputeHash(key));

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public ulong Fetch(ulong key)
    {
        Span<ulong> cast = MemoryMarshal.Cast<byte, ulong>(_buffer.Span);
        int half = cast.Length / 2;
        return cast[half..][cast[..half].BinarySearch(key, Comparer<ulong>.Default)];
    }
}
