using Godot;

namespace OpenNV.Runtime.Diagnostics.Performance;

internal partial class RuntimePerformanceObserver
{
    private readonly HashSet<Viewport> _viewports = [];
    private sealed record ViewportCost(string Name, double CpuMilliseconds, double GpuMilliseconds);
    private sealed record RenderCost(ulong SampleMilliseconds, int SunShadowCascades, float WorldScale,
        float? ViewDistanceMeters, float? CameraFarMeters,
        double SetupCpuMilliseconds, ViewportCost[] Viewports);
    private RenderCost? _renderCost;
    private int _renderCostPending;

    public override void _Ready()
    {
        if (!LogEnabled) return;
        ProcessMode = ProcessModeEnum.Always;
        TrackViewport(GetTree().Root);
        foreach (var node in GetTree().Root.FindChildren("*", nameof(Viewport), true, false)) TrackViewport(node);
        GetTree().NodeAdded += TrackViewport;
        GetTree().NodeRemoved += UntrackViewport;
    }

    private void TrackViewport(Node node)
    {
        if (node is Viewport viewport && _viewports.Add(viewport))
            RenderingServer.ViewportSetMeasureRenderTime(viewport.GetViewportRid(), true);
    }

    private void UntrackViewport(Node node)
    {
        if (node is Viewport viewport && _viewports.Remove(viewport))
            RenderingServer.ViewportSetMeasureRenderTime(viewport.GetViewportRid(), false);
    }

    private RenderCost? SampleRenderCost()
    {
        if (Interlocked.CompareExchange(ref _renderCostPending, 1, 0) == 0)
        {
            var views = _viewports.Select(view => (Name: view.Name.ToString(), Rid: view.GetViewportRid())).ToArray();
            var cascades = Presentation.Rendering.RuntimeSunShadows.Cascades;
            var scale = GetViewport().Scaling3DScale;
            var distance = Presentation.Rendering.RuntimeViewDistance.Meters;
            var far = GetViewport().GetCamera3D()?.Far;
            RenderingServer.CallOnRenderThread(Callable.From(() =>
            {
                try
                {
                    var costs = views.Select(view => new ViewportCost(view.Name,
                        RenderingServer.ViewportGetMeasuredRenderTimeCpu(view.Rid),
                        RenderingServer.ViewportGetMeasuredRenderTimeGpu(view.Rid))).ToArray();
                    Volatile.Write(ref _renderCost, new(Time.GetTicksMsec(), cascades, scale, distance, far, RenderingServer.GetFrameSetupTimeCpu(), costs));
                }
                finally { Volatile.Write(ref _renderCostPending, 0); }
            }));
        }
        return Volatile.Read(ref _renderCost);
    }
}
