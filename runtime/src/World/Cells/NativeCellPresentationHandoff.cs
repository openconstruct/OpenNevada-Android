using Godot;

namespace OpenNV.Runtime.World.Cells;

// The destination is authoritative before this boundary. Keep its presentation
// hidden while old native resources/finalizers drain, without advancing gameplay.
internal static class NativeCellPresentationHandoff
{
    internal static async Task Retire(Node host, Node3D source, Node3D destination, Action releaseUnusedPrototypes)
    {
        if (destination.Visible || destination.ProcessMode != Node.ProcessModeEnum.Disabled)
            throw new InvalidOperationException("Destination presentation must remain gated until source retirement.");
        var tree = host.GetTree();
        var paused = tree.Paused;
        tree.Paused = true;
        var loading = new CanvasLayer { Layer = 1000, ProcessMode = Node.ProcessModeEnum.Always };
        var background = new ColorRect { Color = Colors.Black, MouseFilter = Control.MouseFilterEnum.Stop };
        background.SetAnchorsAndOffsetsPreset(Control.LayoutPreset.FullRect);
        loading.AddChild(background);
        var center = new CenterContainer();
        center.SetAnchorsAndOffsetsPreset(Control.LayoutPreset.FullRect);
        center.AddChild(new Label { Text = "Loading…" });
        loading.AddChild(center);
        host.AddChild(loading);
        try
        {
            source.Free();
            releaseUnusedPrototypes();
            // Native resources can outlive their managed wrappers. This is one
            // loading boundary, never a per-frame collection policy.
            // Mesh -> material -> shader/texture wrappers can become eligible
            // only after the preceding native owner finalizes. Drain that
            // ownership depth rather than collecting only the outer mesh.
            for (var generation = 0; generation < 3; generation++)
            {
                GC.Collect(GC.MaxGeneration, GCCollectionMode.Forced, blocking: true, compacting: false);
                GC.WaitForPendingFinalizers();
            }
            await host.ToSignal(tree, SceneTree.SignalName.ProcessFrame);
            if (DisplayServer.GetName() != "headless")
            {
                // Deferred GPU destruction rotates through the device's frame
                // ring. Empty frames must retire every slot before new draws.
                var frames = RenderingServer.GetRenderingDevice()?.GetFrameDelay() ?? 1;
                for (var frame = 0; frame <= frames; frame++)
                    await host.ToSignal(RenderingServer.Singleton, RenderingServer.SignalName.FramePostDraw);
                RenderingServer.ForceSync();
            }
            await host.ToSignal(tree, SceneTree.SignalName.ProcessFrame);
        }
        finally
        {
            loading.QueueFree();
            destination.Show();
            destination.ProcessMode = Node.ProcessModeEnum.Inherit;
            tree.Paused = paused;
        }
    }
}
