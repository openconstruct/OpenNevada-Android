using System.Buffers.Binary;
using Godot;
using OpenNV.Runtime.Content;
using OpenNV.Runtime.Formats.Gamebryo;
using OpenNV.Runtime.Presentation.Rendering;

public partial class NativeNifInstanceAudit
{
    private async Task ExerciseDdsSharing(string root)
    {
        RuntimeLiveContentSource.Configure(root, RuntimeLiveContentSource.FalloutNewVegasGame);
        using var content = RuntimeLiveContentSource.Current!;
        content.ArchiveWarmup.GetAwaiter().GetResult();
        var bytes = DdsFixture(opaque: true);
        var first = NativeDdsTexture.Load(bytes, "synthetic-sharing");
        var id = first.GetInstanceId();
        using var a = new StandardMaterial3D { AlbedoTexture = first };
        using var b = new StandardMaterial3D { AlbedoTexture = first };
        a.AlbedoColor = Colors.Red;
        Require(b.AlbedoColor == Colors.White, "Shared textures merged mutable material state.");
        first.Dispose();
        Require(GodotObject.IsInstanceIdValid(id), "Materials did not retain their native texture.");
        var recovered = NativeDdsTexture.Load(bytes.ToArray(), "synthetic-sharing");
        Require(recovered.GetInstanceId() == id && GodotObject.IsInstanceValid(recovered),
            "Texture reuse failed after the managed wrapper was disposed while native materials retained it.");
        using var normal = NativeDdsTexture.Load(bytes, "synthetic-sharing", normal: true);
        Require(normal.GetInstanceId() != id, "Normal validation policy was omitted from texture identity.");
        using var differentSource = NativeDdsTexture.Load(bytes, "synthetic-other-source");
        Require(differentSource.GetInstanceId() != id, "Winning source identity was omitted from texture reuse.");
        var changedBytes = DdsFixture();
        using var changed = NativeDdsTexture.Load(changedBytes, "synthetic-sharing");
        Require(changed.GetInstanceId() != id, "Changed source pixels reused a stale texture.");
        using var expected = ReadDds(changedBytes);
        Require(expected.Decompress() == Error.Ok, "Changed fixture did not decode.");
        using var actual = changed.GetImage();
        Require(actual.GetData().SequenceEqual(expected.GetData()), "Sharing changed alpha or authored mip bytes.");
        a.AlbedoTexture = null; b.AlbedoTexture = null;
        recovered.Dispose();
        await ToSignal(GetTree(), SceneTree.SignalName.ProcessFrame);
        Require(!GodotObject.IsInstanceIdValid(id), "The texture index pinned an unused resource.");
        using var reloaded = NativeDdsTexture.Load(bytes, "synthetic-sharing");
        Require(reloaded.GetInstanceId() != id, "Expired texture identity was reused.");

        var faceBytes = DdsFixture(opaque: true);
        var cubeBytes = new byte[128 + (faceBytes.Length - 128) * 6];
        faceBytes.AsSpan(0, 128).CopyTo(cubeBytes);
        BinaryPrimitives.WriteUInt32LittleEndian(cubeBytes.AsSpan(112), 0xfe00);
        for (var face = 0; face < 6; face++)
        {
            var offset = 128 + face * (faceBytes.Length - 128);
            faceBytes.AsSpan(128).CopyTo(cubeBytes.AsSpan(offset));
            BinaryPrimitives.WriteUInt16LittleEndian(cubeBytes.AsSpan(offset), (ushort)(65535 - face * 513));
        }
        var cube = NativeDdsTexture.LoadCubemap(cubeBytes, "synthetic-cube");
        var cubeId = cube.GetInstanceId();
        using var cubeShader = new Shader { Code = "shader_type spatial; uniform samplerCube environment_cube;" };
        using var cubeMaterial = new ShaderMaterial { Shader = cubeShader };
        NativeVariant.SetShaderParameter(cubeMaterial, "environment_cube", cube);
        Require(cube.GetLayers() == 6 && cube.GetWidth() == 8 && cube.HasMipmaps(), "Cubemap faces or mip declaration changed.");
        cube.Dispose();
        var cubeAgain = NativeDdsTexture.LoadCubemap(cubeBytes.ToArray(), "synthetic-cube");
        Require(cubeAgain.GetInstanceId() == cubeId, "Native material ownership did not preserve cubemap reuse.");
        cubeMaterial.SetShaderParameter("environment_cube", default(Variant));
        cubeAgain.Dispose();
        await ToSignal(GetTree(), SceneTree.SignalName.ProcessFrame);
        Require(!GodotObject.IsInstanceIdValid(cubeId), "Cubemap reuse index pinned an unused resource.");

        // A managed Variant can hide a large native packed array from the GC's
        // allocation accounting. Its buffer must release when the array clears.
        using var slots = new Godot.Collections.Array();
        slots.Resize(1);
        var vertices = new Vector3[262144];
        NativeVariant.SetArrayElement(slots, 0, vertices);
        var allocated = (long)OS.GetStaticMemoryUsage();
        slots.Clear();
        var released = (long)OS.GetStaticMemoryUsage();
        Require(allocated - released >= vertices.LongLength * 12,
            "A temporary Variant retained packed mesh data after its native owner cleared.");

        string? ownedIdentity = null;
        foreach (var path in content.ResourcePathsUnder("textures\\architecture"))
        {
            if (!path.EndsWith(".dds", StringComparison.OrdinalIgnoreCase) ||
                !content.TryRead(path, null, out var ownedBytes, out var identity)) continue;
            if (BinaryPrimitives.ReadUInt32LittleEndian(ownedBytes.AsSpan(112)) != 0 || FalloutDdsMipChain.ReadPartial(ownedBytes) is not null) continue;
            using var ownedFirst = NativeDdsTexture.Load(ownedBytes, identity);
            var ownedSecond = NativeDdsTexture.Load(ownedBytes.ToArray(), identity);
            Require(ownedFirst.GetInstanceId() == ownedSecond.GetInstanceId(), "Owned DDS upload was duplicated.");
            ownedIdentity = identity;
            break;
        }
        Require(ownedIdentity is not null, "No owned 2D DDS was exercised.");
        GD.Print($"OPENNV_DDS_SHARING_PASS nativeMaterialLifetime=true unusedReleased=true sourceAndBytesIdentity=true normalPolicy=true mipAlphaBytes=true independentMaterials=true cubemapLifetime=true owned={ownedIdentity} stats={System.Text.Json.JsonSerializer.Serialize(NativeDdsTexture.SharingState)}");
    }
}
