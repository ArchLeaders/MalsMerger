using SarcLibrary;
using ZstdSharp;

namespace MalsMerger.Core.Extensions;

public class ZstdExtension
{
    private readonly Decompressor _defaultDecompressor = new();
    private readonly Compressor _defaultCompressor = new();
    private readonly Dictionary<string, Decompressor> _decompressors = [];
    private readonly Dictionary<string, Compressor> _compressors = [];

    private static readonly Lazy<ZstdExtension> _shared = new(() => new(TotkConfig.Shared.ZsDicPath));
    public static ZstdExtension Shared => _shared.Value;

    public ZstdExtension(string zsDicPath)
    {
        if (File.Exists(zsDicPath)) {
            Span<byte> data = _defaultDecompressor.Unwrap(File.ReadAllBytes(zsDicPath));
            SarcFile sarc = SarcFile.FromBinary(data.ToArray());

            foreach ((var file, var fileData) in sarc) {
                Decompressor decompressor = new();
                decompressor.LoadDictionary(fileData);
                _decompressors[file[..file.LastIndexOf('.')]] = decompressor;

                Compressor compressor = new();
                compressor.LoadDictionary(fileData);
                _compressors[file[..file.LastIndexOf('.')]] = compressor;
            }
        }
    }

    public Span<byte> TryDecompress(string file)
    {
        Span<byte> src = File.ReadAllBytes(file);

        if (!file.EndsWith(".zs")) {
            return src;
        }

        try {
            foreach ((var key, var decompressor) in _decompressors) {
                if (file.EndsWith($"{key}.zs")) {
                    return decompressor.Unwrap(src);
                }
            }

            if (_decompressors.TryGetValue("zs", out Decompressor? common)) {
                return common.Unwrap(src);
            }

            return _defaultDecompressor.Unwrap(src);
        }
        catch (Exception ex) {
            throw new Exception($"Could not decompress '{file}'", ex);
        }
    }

    public Span<byte> TryCompress(string file, Span<byte> buffer)
    {
        if (!file.EndsWith(".zs")) {
            return buffer;
        }

        try {
            foreach ((var key, var compressor) in _compressors) {
                if (file.EndsWith($"{key}.zs")) {
                    return compressor.Wrap(buffer);
                }
            }

            if (_compressors.TryGetValue("zs", out Compressor? common)) {
                return common.Wrap(buffer);
            }

            return _defaultCompressor.Wrap(buffer);
        }
        catch (Exception ex) {
            throw new Exception($"Could not decompress '{file}'", ex);
        }
    }
}