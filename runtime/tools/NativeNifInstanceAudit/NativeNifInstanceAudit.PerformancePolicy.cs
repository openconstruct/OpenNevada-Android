using Godot;
using OpenNV.Runtime.Content;
using OpenNV.Runtime.Presentation.Rendering;

public partial class NativeNifInstanceAudit
{
    private async Task ExercisePerformancePolicy(string root)
    {
        RuntimeLiveContentSource.Configure(root, RuntimeLiveContentSource.FalloutNewVegasGame);
        using var source = RuntimeLiveContentSource.Current!;
        using var records = FalloutPluginStack.Load(source.PluginSources);
        var settings = FalloutInstallationSettings.Read(source);
        var doc = FalloutCellSceneReader.Read(records, FalloutDialogueTopic.Find(records, "CELL", "GSDocMitchellHouse").FormKey);
        var destination = doc.References.Where(reference => reference.Teleport is not null)
            .Select(reference => (Reference: reference, Transition: FalloutDoorDestinationResolver.Resolve(records, reference)))
            .Single(value => value.Transition.DestinationScene.Cell.Worldspace is not null);
        var world = destination.Transition.DestinationScene.Cell.Worldspace!.Value;
        var point = destination.Reference.Teleport!.Position;
        var grid = new FalloutExteriorGrid(records);
        var persistent = grid.PersistentCell(world);
        var selected = grid.ResolveOwned(world, persistent, point[0], point[1], settings);
        var originalDiameter = settings.Unsigned("General", "uGridsToLoad");
        Require(selected.Radius * 2 + 1 == originalDiameter, "Runtime expanded the owned exterior grid diameter.");
        var oldForced = grid.Resolve(world, persistent, point[0], point[1], Math.Max(7, (int)originalDiameter));
        foreach (var invalid in new[] { 0, 2, -1 })
        {
            try { _ = grid.Resolve(world, persistent, point[0], point[1], invalid); throw new InvalidOperationException("Invalid grid diameter was admitted."); }
            catch (InvalidDataException) { }
        }
        var view = new SubViewport { Size = new(1280, 720), OwnWorld3D = true, RenderTargetUpdateMode = SubViewport.UpdateMode.Always };
        var effect = new NativeRenderSizeAuditEffect();
        var effects = new Godot.Collections.Array<CompositorEffect> { effect };
        using var effectsOwner = (Godot.Collections.Array)effects;
        using var compositor = new Compositor { CompositorEffects = effects };
        AddChild(view);
        try
        {
            RuntimeRenderScale.Apply(view, ["--render-scale-3d", "0.75"], true);
            Require(view.Scaling3DScale == 1, "Flat resolution policy leaked into XR.");
            foreach (var invalid in new[] { "0", "NaN", "1.1" })
            {
                try { RuntimeRenderScale.Apply(view, ["--render-scale-3d", invalid], false); throw new InvalidOperationException("Invalid render scale was admitted."); }
                catch (ArgumentOutOfRangeException) { }
            }
            RuntimeRenderScale.Apply(view, ["--render-scale-3d", "0.75"], false);
            view.AddChild(new WorldEnvironment { Compositor = compositor });
            view.AddChild(new Camera3D { Position = new(0, 0, 2), Current = true });
            view.AddChild(new MeshInstance3D { Mesh = new BoxMesh() });
            var canvas = new Control(); view.AddChild(canvas); canvas.SetAnchorsAndOffsetsPreset(Control.LayoutPreset.FullRect);
            for (var frame = 0; frame < 8; frame++) await ToSignal(RenderingServer.Singleton, RenderingServer.SignalName.FramePostDraw);
            Require(effect.InternalSize == new Vector2I(960, 540), $"Unexpected internal 3D extent: {effect.InternalSize}.");
            Require(canvas.Size == new Vector2(1280, 720), "3D scaling changed the UI canvas extent.");
            using var image = view.GetTexture().GetImage();
            Require(image.GetSize() == new Vector2I(1280, 720), "3D scaling changed the presented output extent.");
            GD.Print($"OPENNV_PERFORMANCE_POLICY_PASS ownedDiameter={originalDiameter} cells={selected.Cells.Count} references={selected.Scene.References.Count} previousForcedCells={oldForced.Cells.Count} previousForcedReferences={oldForced.Scene.References.Count} internal=960x540 ui=1280x720 output=1280x720 xrUnchanged=true invalidRejected=true ordinaryPerformance=unverified");
        }
        finally { view.Free(); effect.Dispose(); }
    }
}

internal partial class NativeRenderSizeAuditEffect : CompositorEffect
{
    internal Vector2I InternalSize { get; private set; }
    public NativeRenderSizeAuditEffect() { EffectCallbackType = EffectCallbackTypeEnum.PostTransparent; }
    public override void _RenderCallback(int effectCallbackType, RenderData renderData)
    {
        using var buffers = renderData.GetRenderSceneBuffers() as RenderSceneBuffersRD;
        if (buffers is not null) InternalSize = buffers.GetInternalSize();
    }
}
