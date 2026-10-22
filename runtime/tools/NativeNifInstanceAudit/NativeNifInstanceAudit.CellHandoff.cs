using Godot;
using OpenNV.Runtime.Content;
using OpenNV.Runtime.Formats.Gamebryo;
using OpenNV.Runtime.Presentation.Rendering;
using OpenNV.Runtime.World.Cells;
using System.Runtime.CompilerServices;

public partial class NativeNifInstanceAudit
{
    private async Task ExerciseCellHandoff(string root)
    {
        RuntimeLiveContentSource.Configure(root, RuntimeLiveContentSource.FalloutNewVegasGame);
        using var content = RuntimeLiveContentSource.Current!;
        content.ArchiveWarmup.GetAwaiter().GetResult();
        const string path = "meshes\\clutter\\junk\\opthamoscope01.nif";
        Require(content.TryRead(path, null, out var bytes, out _), "Owned handoff model is missing.");
        var prototypes = new Dictionary<string, RuntimeNativeNifPrototype>(StringComparer.OrdinalIgnoreCase)
        {
            ["source"] = new(bytes, 0.0142875f),
            ["destination"] = new(bytes, 0.0142875f)
        };
        var retiredPrototype = prototypes["source"].Scene.Root.GetInstanceId();
        var retainedPrototype = prototypes["destination"].Scene.Root.GetInstanceId();
        var source = new Node3D();
        source.AddChild(prototypes["source"].Instantiate());
        var (retiredShader, retiredTexture) = AttachSourceOnlyResourceChain(source);
        AddChild(source);
        var sourceId = source.GetInstanceId();
        var target = new Node3D { Visible = false, ProcessMode = ProcessModeEnum.Disabled };
        target.AddChild(prototypes["destination"].Instantiate());
        AddChild(target);
        var meshes = target.FindChildren("*", "", true, false).OfType<MeshInstance3D>()
            .Select(node => (Node: node, Mesh: node.Mesh.GetInstanceId(), Surfaces: node.Mesh.GetSurfaceCount())).ToArray();
        Require(meshes.Length > 0, "Owned handoff model has no geometry.");
        var contacts = target.FindChildren("*", "", true, false).OfType<CollisionShape3D>()
            .Select(node => node.Shape.GetInstanceId()).ToArray();
        Require(contacts.Length > 0, "Owned handoff model has no contacts.");
        var gated = false;
        void ObserveBoundary()
        {
            if (!GodotObject.IsInstanceIdValid(sourceId))
            {
                Require(!target.Visible && target.ProcessMode == ProcessModeEnum.Disabled && GetTree().Paused,
                    "Destination or gameplay advanced before source retirement completed.");
                gated = true;
            }
        }
        GetTree().ProcessFrame += ObserveBoundary;
        try
        {
            await NativeCellPresentationHandoff.Retire(this, source, target,
                () => NativeNifPrototypeResidency.Trim(prototypes, new HashSet<string>(StringComparer.OrdinalIgnoreCase) { "destination" }));
        }
        finally { GetTree().ProcessFrame -= ObserveBoundary; }
        Require(gated && target.Visible && target.ProcessMode == ProcessModeEnum.Inherit && !GetTree().Paused,
            "Handoff did not restore presentation/gameplay after the gated frame.");
        Require(!GodotObject.IsInstanceIdValid(sourceId) && !GodotObject.IsInstanceIdValid(retiredPrototype) &&
            GodotObject.IsInstanceIdValid(retainedPrototype) && prototypes.Count == 1,
            "Prototype retirement retained the source or removed the destination.");
        foreach (var (node, mesh, surfaces) in meshes)
            Require(node.Mesh.GetInstanceId() == mesh && node.Mesh.GetSurfaceCount() == surfaces,
                "Retiring the prototype changed published destination geometry.");
        Require(contacts.All(GodotObject.IsInstanceIdValid), "Retiring the source removed destination contacts.");
        Require(!GodotObject.IsInstanceIdValid(retiredShader) && !GodotObject.IsInstanceIdValid(retiredTexture),
            "Source-only shader/texture wrappers survived the nested native owner teardown.");
        target.Free();
        NativeNifPrototypeResidency.Trim(prototypes, new HashSet<string>());
        var failureSource = new Node3D();
        var failureTarget = new Node3D { Visible = false, ProcessMode = ProcessModeEnum.Disabled };
        AddChild(failureSource); AddChild(failureTarget);
        try
        {
            await NativeCellPresentationHandoff.Retire(this, failureSource, failureTarget,
                () => throw new InvalidOperationException("injected retirement failure"));
            throw new InvalidDataException("Retirement failure was swallowed.");
        }
        catch (InvalidOperationException error) when (error.Message == "injected retirement failure") { }
        Require(failureTarget.Visible && failureTarget.ProcessMode == ProcessModeEnum.Inherit && !GetTree().Paused,
            "A retirement failure trapped gameplay behind the loading gate.");
        failureTarget.Free();
        GD.Print("OPENNV_CELL_HANDOFF_PASS ownedGeometry=true contactsRetained=true sourceAndUnusedPrototypeReleased=true nestedResourcesReleased=true gatedFrames=true pauseRestored=true failureGateReleased=true");
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static (ulong Shader, ulong Texture) AttachSourceOnlyResourceChain(Node3D source)
    {
        var input = source.FindChildren("*", "", true, false).OfType<MeshInstance3D>().First();
        using var arrays = input.Mesh.SurfaceGetArrays(0);
        var mesh = new ArrayMesh();
        mesh.AddSurfaceFromArrays(Mesh.PrimitiveType.Triangles, arrays);
        var shader = NativeShaderResources.Load("shader_type spatial; uniform sampler2D retired_texture; void fragment() { ALBEDO=texture(retired_texture,UV).rgb; }");
        using var image = Image.CreateEmpty(1, 1, false, Image.Format.Rgba8);
        image.Fill(Colors.Red);
        var texture = ImageTexture.CreateFromImage(image);
        var material = new ShaderMaterial { Shader = shader };
        NativeVariant.SetShaderParameter(material, "retired_texture", texture);
        mesh.SurfaceSetMaterial(0, material);
        source.AddChild(new MeshInstance3D { Mesh = mesh });
        return (shader.GetInstanceId(), texture.GetInstanceId());
    }
}
