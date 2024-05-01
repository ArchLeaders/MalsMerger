using CommunityToolkit.HighPerformance.Buffers;
using MalsMerger.Core.Extensions;
using MalsMerger.Core.Helpers;
using MessageStudio.Formats.BinaryText;
using Revrs;
using Revrs.Buffers;
using SarcLibrary;
using System.Text.Json.Serialization;
using TotkCommon;

namespace MalsMerger.Core.Models;

public class MalsChangelog : Dictionary<string, Msbt>
{
    private static readonly MsbtOptions _msbtOptions = new() {
        DuplicateKeyMode = MsbtDuplicateKeyMode.UseLastOccurrence
    };

    /// <summary>
    /// Append a Mals archive and merge it with the current <see cref="MalsChangelog"/>.
    /// </summary>
    /// <param name="malsArchiveFile"></param>
    /// <param name="malsArchiveData"></param>
    public void Append(GameFile malsArchiveFile, byte[] malsArchiveData)
    {
        RevrsReader reader = new(Totk.Zstd.Decompress(malsArchiveData));
        ImmutableSarc malsArchive = new(ref reader);

        foreach ((var msbtFile, var msbtData) in malsArchive) {
            if (!msbtData.IsMsbtFile() || msbtData.IsVanilla(malsArchiveFile, msbtFile)) {
                continue;
            }

            Msbt msbt = Msbt.FromBinary(msbtData, _msbtOptions);

            TryGetValue(msbtFile, out Msbt? currentMsbt);
            this[msbtFile] = Merge(msbt, currentMsbt ?? [], msbtFile);
        }

        Msbt Merge(Msbt msbtA, Msbt msbtB, string msbtPath)
        {
            foreach ((var label, var entry) in msbtA) {
                if (!entry.IsVanilla(malsArchiveFile, msbtPath, label)) {
                    msbtB[label] = entry;
                }
            }

            return msbtB;
        }
    }

    /// <summary>
    /// Append a Mals changelog and merge it with the current <see cref="MalsChangelog"/>.
    /// </summary>
    /// <param name="changelog">The <see cref="MalsChangelog"/> to merge with the current instance.</param>
    public void Append(MalsChangelog changelog)
    {
        foreach ((var msbtPath, var msbt) in changelog) {
            if (!TryGetValue(msbtPath, out Msbt? currentMsbt)) {
                this[msbtPath] = msbt;
                continue;
            }

            foreach ((var label, var entry) in msbt) {
                currentMsbt[label] = entry;
            }
        }
    }

    /// <summary>
    /// Build a <see cref="GameFile"/> from the current <see cref="MalsChangelog"/> and write it to the <paramref name="output"/>.
    /// </summary>
    /// <param name="output">The output stream to write the Mals archive.</param>
    public void Build(GameFile malsArchive, Stream output)
    {
        string vanillaMalsArchivePath = malsArchive.GetVanilla();

        using FileStream fs = File.OpenRead(vanillaMalsArchivePath);
        using SpanOwner<byte> vanillaMalsArchiveBuffer =
            SpanOwner<byte>.Allocate(Convert.ToInt32(fs.Length));
        fs.Read(vanillaMalsArchiveBuffer.Span);

        using ArraySegmentOwner<byte> vanillaMalsArchiveDecompressedBuffer =
            ArraySegmentOwner<byte>.Allocate(Zstd.GetDecompressedSize(vanillaMalsArchiveBuffer.Span));
        Totk.Zstd.Decompress(vanillaMalsArchiveBuffer.Span, vanillaMalsArchiveDecompressedBuffer.Segment, out int dictionaryId);

        Sarc vanillaMalsArchive = Sarc.FromBinary(
            vanillaMalsArchiveDecompressedBuffer.Segment
        );

        foreach ((var msbtPath, var msbt) in this) {
            if (!vanillaMalsArchive.TryGetValue(msbtPath, out ArraySegment<byte> msbtData)) {
                vanillaMalsArchive[msbtPath]
                    = msbt.ToBinary(msbt.Encoding, msbt.Endianness);
                continue;
            }

            Msbt vanillaMsbt = Msbt.FromBinary(msbtData, _msbtOptions);
            foreach ((var label, var entry) in msbt) {
                vanillaMsbt[label] = entry;
            }

            vanillaMalsArchive[msbtPath]
                = msbt.ToBinary(vanillaMsbt.Encoding, vanillaMsbt.Endianness);
        }

        using MemoryStream malsBinaryStream = new();
        vanillaMalsArchive.Write(malsBinaryStream);
        ReadOnlySpan<byte> malsBuffer = malsBinaryStream.ToArray();

        using SpanOwner<byte> compressedBuffer = SpanOwner<byte>.Allocate(malsBuffer.Length);
        int compressedSize = Totk.Zstd.Compress(malsBuffer, compressedBuffer.Span, dictionaryId);
        output.Write(compressedBuffer.Span[..compressedSize]);
    }
}

[JsonSerializable(typeof(MalsChangelog))]
public partial class MalsChangelogSerializerContext : JsonSerializerContext
{

}