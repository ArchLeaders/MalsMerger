using MalsMerger.Core.Extensions;
using SarcLibrary;
using Standart.Hash.xxHash;
using System.Buffers.Binary;

string src = args[0];
string[] versions = args[1].Split('|');
string output = args[2];

Span<byte> qword = stackalloc byte[sizeof(ulong)];

foreach (string version in versions) {
    string zsDicPath = Path.Combine(src, version, "Pack", "ZsDic.pack.zs");
    ZstdExtension zstd = new(zsDicPath);

    string mals = Path.Combine(src, version, "Mals");
    List<ulong> keys = [];
    List<ulong> hashes = [];
    foreach (string file in Directory.EnumerateFiles(mals)) {
        string relativePath = Path.GetRelativePath(mals, file);
        Span<byte> decompressed = zstd.TryDecompress(file);
        SarcFile sarc = SarcFile.FromBinary(decompressed.ToArray());
        foreach ((var name, var buffer) in sarc) {
            keys.Add(xxHash64.ComputeHash(Path.Combine(relativePath, name).Replace('\\', '/')));
            hashes.Add(xxHash64.ComputeHash(buffer));
        }
    }

    using FileStream fs = File.Create(Path.Combine(output, version.Replace(".", string.Empty)));

    foreach (var key in keys) {
        BinaryPrimitives.WriteUInt64LittleEndian(qword, key);
        fs.Write(qword);
    }

    foreach (var hash in hashes) {
        BinaryPrimitives.WriteUInt64LittleEndian(qword, hash);
        fs.Write(qword);
    }
}

