using Revrs;
using SarcLibrary;
using ZstdSharp;

namespace MalsMerger.Core.Helpers;

public static class ZstdHelper
{
    private static readonly Decompressor _decompressor = new();
    private static readonly Compressor _compressor = new();

    static ZstdHelper()
    {
        Span<byte> data = _decompressor.Unwrap(File.ReadAllBytes(TotkConfig.Shared.ZsDicPath));
        RevrsReader reader = new(data);
        ImmutableSarc sarc = new(ref reader);
        _decompressor.LoadDictionary(sarc["zs.zsdic"].Data);
        _compressor.LoadDictionary(sarc["zs.zsdic"].Data);
    }

    public static Span<byte> Decompress(string file)
    {
        byte[] src = File.ReadAllBytes(file);
        return Decompress(src, file);
    }

    public static Span<byte> Decompress(byte[] src, string file)
    {
        if (!file.EndsWith(".zs")) {
            return src;
        }

        return _decompressor.Unwrap(src);
    }

    public static Span<byte> Compress(Span<byte> buffer, string file)
    {
        if (!file.EndsWith(".zs")) {
            return buffer;
        }

        return _compressor.Wrap(buffer);
    }
}
