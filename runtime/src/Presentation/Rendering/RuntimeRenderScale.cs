using System.Globalization;
using Godot;

namespace OpenNV.Runtime.Presentation.Rendering;

internal static class RuntimeRenderScale
{
    internal static void Apply(Viewport viewport, string[] arguments, bool xr)
    {
        var option = Array.IndexOf(arguments, "--render-scale-3d");
        var scale = option < 0 ? 1f : option + 1 < arguments.Length
            ? float.Parse(arguments[option + 1], CultureInfo.InvariantCulture)
            : throw new ArgumentException("--render-scale-3d requires a numeric scale.");
        if (!float.IsFinite(scale) || scale is < .25f or > 1f)
            throw new ArgumentOutOfRangeException(nameof(arguments), "3D render scale must be between 0.25 and 1.");
        // Scale the world only. Canvas UI and the final window stay native.
        // XR uses its own eye resolution and must not inherit the flat policy.
        viewport.Scaling3DScale = xr ? 1 : scale;
        GD.Print($"OPENNV_RENDER_SCALE worldScale={viewport.Scaling3DScale:R} canvasScale=1 xr={xr}");
    }
}
