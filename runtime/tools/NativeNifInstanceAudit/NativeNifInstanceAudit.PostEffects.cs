using Godot;
using OpenNV.Runtime;
using OpenNV.Runtime.Content;
using OpenNV.Runtime.Presentation.Rendering;

public partial class NativeNifInstanceAudit
{
    private async Task ExercisePostEffects(string root)
    {
        RuntimeLiveContentSource.Configure(root, RuntimeLiveContentSource.FalloutNewVegasGame);
        using var source = RuntimeLiveContentSource.Current!;
        var programs = FalloutImageSpacePrograms.Read(source);
        var configuration = RuntimeConfiguration.Load();
        var views = new List<SubViewport>();
        var resources = new List<Resource>();
        var effects = new List<RetailHdrCompositorEffect>();
        try
        {
            var off = Create(false, false);
            var on = Create(true, false);
            var tinted = Create(false, true);
            for (var frame = 0; frame < 12; frame++) await ToSignal(RenderingServer.Singleton, RenderingServer.SignalName.FramePostDraw);
            using var plainImage = off.GetTexture().GetImage();
            using var bloomImage = on.GetTexture().GetImage();
            using var tintedImage = tinted.GetTexture().GetImage();
            var plain = plainImage.GetPixel(64, 64);
            GD.Print($"OPENNV_POST_EFFECTS_SAMPLE plain={plain} bloom={bloomImage.GetPixel(64, 64)} tinted={tintedImage.GetPixel(64, 64)}");
            var color = tintedImage.GetPixel(64, 64);
            Require(plain.R > .05f && plain.R < .95f, "Bloom-off path lost the image or bypassed cinematic brightness.");
            Require(color.R > color.G + .1f && color.B > color.G + .05f, "Bloom-off path discarded tint or fade.");
            var haloDifference = 0f;
            for (var y = 0; y < 128; y++)
                for (var x = 0; x < 128; x++)
                    if (plainImage.GetPixel(x, y).R < .005f)
                        haloDifference = Math.Max(haloDifference, bloomImage.GetPixel(x, y).R - plainImage.GetPixel(x, y).R);
            Require(haloDifference > .001f, "Bloom on/off did not change the halo outside the bright source.");
            GD.Print($"OPENNV_POST_EFFECTS_GPU_PASS bloomOff=true haloDifference={haloDifference:R} cinematic=true tint=true fade=true sourcePrograms=true pixelParity=unverified");
        }
        finally
        {
            foreach (var effect in effects) effect.ReleaseRenderingResources();
            await ToSignal(RenderingServer.Singleton, RenderingServer.SignalName.FramePostDraw);
            Require(effects.All(effect => !effect.Operational), "Compositor resources remained operational after release.");
            foreach (var view in views) view.Free();
            foreach (var resource in resources) resource.Dispose();
            await ToSignal(GetTree(), SceneTree.SignalName.ProcessFrame);
        }

        SubViewport Create(bool bloom, bool tint)
        {
            var view = new SubViewport { Size = new(128, 128), OwnWorld3D = true, RenderTargetUpdateMode = SubViewport.UpdateMode.Always };
            AddChild(view); views.Add(view);
            var effect = new RetailHdrCompositorEffect(1, 1, new(1, 0, 1, .1f), tint ? new(1, 0, 0, 1) : Vector4.Zero,
                tint ? new(0, 0, 1, .2f) : Vector4.Zero, configuration.FalloutEnvironment.ImageSpace, configuration.Capture,
                configuration.ActorCompiler.FaceGenMaterial.RuntimeAlbedoTransfer, false, programs, false, false, bloom);
            var world = new Godot.Environment { BackgroundMode = Godot.Environment.BGMode.Color, BackgroundColor = Colors.Black };
            var compositor = new Compositor { CompositorEffects = new Godot.Collections.Array<CompositorEffect> { effect } };
            resources.AddRange([world, compositor, effect]); effects.Add(effect);
            view.AddChild(new WorldEnvironment { Environment = world, Compositor = compositor });
            view.AddChild(new Camera3D { Position = new(0, 0, 2), Projection = Camera3D.ProjectionType.Orthogonal, Size = 2, Current = true });
            var shader = new Shader { Code = "shader_type spatial; render_mode unshaded; void fragment() { ALBEDO = vec3(8.0); }" };
            var material = new ShaderMaterial { Shader = shader };
            var mesh = new QuadMesh { Size = new(.4f, .4f) };
            resources.AddRange([material, shader, mesh]);
            view.AddChild(new MeshInstance3D { Mesh = mesh, MaterialOverride = material });
            return view;
        }
    }
}
