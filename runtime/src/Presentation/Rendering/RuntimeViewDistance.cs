using System.Globalization;
using Godot;

namespace OpenNV.Runtime.Presentation.Rendering;

// Explicit presentation cap; resident gameplay, contacts and source files stay intact.
internal static class RuntimeViewDistance
{
    internal static float? Meters { get; private set; }
    internal static event Action? Changed;

    internal static void Configure(string[] arguments)
    {
        var option = Array.IndexOf(arguments, "--view-distance-meters");
        float? meters = option < 0 ? null : option + 1 < arguments.Length
            ? float.Parse(arguments[option + 1], CultureInfo.InvariantCulture)
            : throw new ArgumentException("--view-distance-meters requires a positive distance.");
        Set(meters);
    }

    internal static void Set(float? meters)
    {
        if (meters is { } value && (!float.IsFinite(value) || value <= 0))
            throw new ArgumentOutOfRangeException(nameof(meters));
        Meters = meters;
        Changed?.Invoke();
        GD.Print($"OPENNV_VIEW_DISTANCE meters={meters?.ToString("R", CultureInfo.InvariantCulture) ?? "source"}");
    }

    internal static float Far(float sourceFar) => Meters is { } limit ? Math.Min(sourceFar, limit) : sourceFar;

    internal static (float Near, float Far) Fog(float near, float far, float units)
    {
        if (Meters is not { } limit) return (near, far);
        var end = Math.Min(far, limit * .95f / units);
        return (Math.Min(near, end * .75f), end);
    }

    internal static void Apply(Camera3D camera, float sourceFar)
    {
        var far = Far(sourceFar);
        if (far <= camera.Near) throw new ArgumentOutOfRangeException(nameof(sourceFar), "View distance must exceed the camera near plane.");
        camera.Far = far;
    }
}
