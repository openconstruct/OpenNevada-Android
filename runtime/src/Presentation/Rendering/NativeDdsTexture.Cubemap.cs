using System.Buffers.Binary;
using Godot;

namespace OpenNV.Runtime.Presentation.Rendering;

internal static partial class NativeDdsTexture
{
    private static bool _cubemapUploadVerified;
    internal static Cubemap LoadCubemap(byte[] payload, string source)
        => LoadShared(payload, source, false, static (bytes, identity, _) => CreateCubemap(bytes, identity));

    private static Cubemap CreateCubemap(byte[] payload, string source)
    {
        const int header = 128, capsOffset = 112;
        const uint cube = 0x200, faces = 0xfc00;
        // Preserve the established DDS -> Godot coordinate/face mapping.
        int[] order = [0, 1, 4, 5, 3, 2];
        if (payload.Length < header || !payload.AsSpan(0, 4).SequenceEqual("DDS "u8))
            throw new InvalidDataException($"Native NIF cubemap is not DDS: {source}");
        var caps = BinaryPrimitives.ReadUInt32LittleEndian(payload.AsSpan(capsOffset));
        if ((caps & cube) == 0 || (caps & faces) != faces || (payload.Length - header) % order.Length != 0)
            throw new InvalidDataException($"Native NIF environment texture is not a complete six-face DDS cubemap: {source}");
        var faceBytes = (payload.Length - header) / order.Length;
        var images = new Godot.Collections.Array<Image>();
        using var imagesOwner = (Godot.Collections.Array)images;
        try
        {
            foreach (var face in order)
            {
                var bytes = new byte[checked(header + faceBytes)];
                payload.AsSpan(0, header).CopyTo(bytes);
                BinaryPrimitives.WriteUInt32LittleEndian(bytes.AsSpan(capsOffset), 0);
                payload.AsSpan(header + face * faceBytes, faceBytes).CopyTo(bytes.AsSpan(header));
                var image = new Image();
                var error = image.LoadDdsFromBuffer(bytes);
                if (error != Error.Ok || image.IsEmpty())
                {
                    image.Dispose();
                    throw new InvalidDataException($"Godot could not decode native NIF cubemap face {face} from {source}: {error}");
                }
                images.Add(image);
            }
            var sourceBytes = images.Sum(image => (long)image.GetDataSize());
            PreserveCubeAlpha(images);
            var result = new Cubemap();
            var errorCreate = result.CreateFromImages(images);
            if (errorCreate != Error.Ok)
            {
                result.Dispose();
                throw new InvalidDataException($"Godot could not create native NIF cubemap {source}: {errorCreate}");
            }
            var uploadedBytes = images.Sum(image => (long)image.GetDataSize());
            result.SetMeta("opennv_dds_source_payload_bytes", sourceBytes);
            result.SetMeta("opennv_dds_upload_payload_bytes", uploadedBytes);
            result.SetMeta("opennv_dds_expanded_bc1", uploadedBytes != sourceBytes);
            if (!_cubemapUploadVerified && OS.GetCmdlineUserArgs().Contains("--verify-dds-cubemap", StringComparer.Ordinal))
            {
                try
                {
                    for (var face = 0; face < images.Count; face++)
                    {
                        using var actual = result.GetLayerData(face);
                        using var expected = (Image)images[face].Duplicate();
                        if (actual is null || actual.IsEmpty()) throw new InvalidDataException("Cubemap upload readback is empty.");
                        if (actual.IsCompressed() && actual.Decompress() != Error.Ok || expected.IsCompressed() && expected.Decompress() != Error.Ok)
                            throw new InvalidDataException("Cubemap readback could not decode its authored levels.");
                        actual.Convert(Image.Format.Rgba8); expected.Convert(Image.Format.Rgba8);
                        if (actual.GetSize() != expected.GetSize() || actual.GetMipmapCount() != expected.GetMipmapCount() ||
                            !actual.GetData().SequenceEqual(expected.GetData()))
                            throw new InvalidDataException($"Cubemap upload changed pixels or authored mip levels on face {face}.");
                    }
                    _cubemapUploadVerified = true;
                    GD.Print($"OPENNV_CUBEMAP_UPLOAD_VERIFIED faces=6 authoredMips=true exactRgbaBytes=true source={source}");
                }
                catch { result.Dispose(); throw; }
            }
            return result;
        }
        finally { foreach (var image in images) image.Dispose(); }
    }
}
