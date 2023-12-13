using MalsMerger.Core.Extensions;
using MalsMerger.Core.Helpers;
using MessageStudio.Formats.BinaryText;
using Revrs;
using SarcLibrary;
using System.Text.Json.Serialization;

namespace MalsMerger.Core.Models;

public class MalsChangelog : Dictionary<string, Msbt>
{
    /// <summary>
    /// Append a Mals archive and merge it with the current <see cref="MalsChangelog"/>.
    /// </summary>
    /// <param name="malsArchiveFile"></param>
    /// <param name="malsArchiveData"></param>
    public void Append(GameFile malsArchiveFile, byte[] malsArchiveData)
    {
        RevrsReader reader = new(ZstdHelper.Decompress(malsArchiveData, malsArchiveFile.Name));
        ImmutableSarc malsArchive = new(ref reader);

        foreach ((var msbtFile, var msbtData) in malsArchive) {
            if (!msbtData.IsMsbtFile() || msbtData.IsVanilla(malsArchiveFile, msbtFile)) {
                continue;
            }

            Msbt msbt = Msbt.FromBinary(msbtData);

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
        Sarc vanillaMalsArchive = Sarc.FromBinary(
            ZstdHelper.Decompress(
                File.ReadAllBytes(vanillaMalsArchivePath), vanillaMalsArchivePath));

        foreach ((var msbtPath, var msbt) in this) {
            Msbt vanillaMsbt = Msbt.FromBinary(vanillaMalsArchive[msbtPath]);
            foreach ((var label, var entry) in msbt) {
                vanillaMsbt[label] = entry;
            }

            using MemoryStream msbtBinaryStream = new();
            vanillaMsbt.ToBinary(msbtBinaryStream, vanillaMsbt.Encoding, vanillaMsbt.Endian);
            vanillaMalsArchive[msbtPath] = msbtBinaryStream.ToArray();
        }

        using MemoryStream malsBinaryStream = new();
        vanillaMalsArchive.Write(malsBinaryStream);

        Span<byte> mergedMalsArchiveData = ZstdHelper.Compress(
            malsBinaryStream.ToArray(), vanillaMalsArchivePath);
        output.Write(mergedMalsArchiveData);
    }
}

[JsonSerializable(typeof(MalsChangelog))]
public partial class MalsChangelogSerializerContext : JsonSerializerContext
{

}