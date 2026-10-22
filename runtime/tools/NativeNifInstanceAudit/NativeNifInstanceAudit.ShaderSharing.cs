using Godot;
using OpenNV.Runtime.Content;
using OpenNV.Runtime.Formats.Gamebryo;
using OpenNV.Runtime.Presentation.Rendering;

public partial class NativeNifInstanceAudit
{
    private async Task ExerciseShaderSharing(string root)
    {
        const string code = "shader_type spatial; uniform float owned_value; void fragment() { ALBEDO=vec3(owned_value); }";
        var shader = NativeShaderResources.Load(code);
        var id = shader.GetInstanceId();
        using var a = new ShaderMaterial { Shader = shader };
        using var b = new ShaderMaterial { Shader = shader };
        a.SetShaderParameter("owned_value", 0.25f); b.SetShaderParameter("owned_value", 0.75f);
        shader.Dispose();
        var recovered = NativeShaderResources.Load(new string(code.ToCharArray()));
        Require(recovered.GetInstanceId() == id && recovered.Code == code,
            "Native materials did not preserve immutable program reuse after wrapper disposal.");
        using var av = a.GetShaderParameter("owned_value");
        using var bv = b.GetShaderParameter("owned_value");
        Require(av.AsDouble() == 0.25 && bv.AsDouble() == 0.75, "Program reuse merged mutable uniforms.");
        using var changed = NativeShaderResources.Load(code.Replace("vec3(owned_value)", "vec3(1.0-owned_value)"));
        Require(changed.GetInstanceId() != id, "Changed shader source reused a stale program.");
        a.Shader = null; b.Shader = null; recovered.Dispose();
        await ToSignal(GetTree(), SceneTree.SignalName.ProcessFrame);
        Require(!GodotObject.IsInstanceIdValid(id), "Program cache retained an unused compiled resource.");

        RuntimeLiveContentSource.Configure(root, RuntimeLiveContentSource.FalloutNewVegasGame);
        using var content = RuntimeLiveContentSource.Current!;
        content.ArchiveWarmup.GetAwaiter().GetResult();
        Require(content.TryRead("meshes\\clutter\\junk\\opthamoscope01.nif", null, out var bytes, out _), "Owned shader model is missing.");
        var first = RuntimeNativeNifMeshBuilder.Build(bytes, 0.0142875f);
        var second = RuntimeNativeNifMeshBuilder.Build(bytes, 0.0142875f);
        try
        {
            static ShaderMaterial[] Materials(Node3D node) => node.FindChildren("*", "", true, false)
                .OfType<MeshInstance3D>().SelectMany(mesh => Enumerable.Range(0, mesh.Mesh.GetSurfaceCount())
                    .Select(surface => mesh.Mesh.SurfaceGetMaterial(surface))).OfType<ShaderMaterial>().ToArray();
            var one = Materials(first.Root); var two = Materials(second.Root);
            Require(one.Length > 0 && one.Length == two.Length, "Owned shader material graph differs between builds.");
            for (var index = 0; index < one.Length; index++)
            {
                Require(one[index].GetInstanceId() != two[index].GetInstanceId() &&
                    one[index].Shader.GetInstanceId() == two[index].Shader.GetInstanceId(),
                    "Owned builds did not share the immutable program with independent material owners.");
            }
        }
        finally { first.Root.Free(); second.Root.Free(); }
        GD.Print($"OPENNV_SHADER_SHARING_PASS nativeMaterialLifetime=true unusedReleased=true exactCode=true independentUniforms=true ownedMaterials=true state={System.Text.Json.JsonSerializer.Serialize(NativeShaderResources.State)}");
    }
}
