using Godot;

namespace OpenNV.Runtime.Presentation.Rendering;

// Presentation quality only: sunlight, source meshes and contact geometry remain.
internal static class RuntimeSunShadows
{
    internal static int Cascades { get; private set; } = 4;
    internal static event Action? Changed;

    internal static void Configure(string[] arguments)
    {
        var option = Array.IndexOf(arguments, "--sun-shadows");
        var cascades = option < 0 ? 4 : option + 1 < arguments.Length && int.TryParse(arguments[option + 1], out var value)
            ? value : throw new ArgumentException("--sun-shadows requires 0, 1, 2 or 4 cascades.");
        Set(cascades);
    }

    internal static void Set(int cascades)
    {
        if (cascades is not (0 or 1 or 2 or 4)) throw new ArgumentOutOfRangeException(nameof(cascades));
        Cascades = cascades;
        Changed?.Invoke();
        GD.Print($"OPENNV_SUN_SHADOWS cascades={cascades}");
    }

    internal static void Apply(DirectionalLight3D sun)
    {
        sun.ShadowEnabled = Cascades != 0;
        sun.DirectionalShadowMode = Cascades switch
        {
            4 => DirectionalLight3D.ShadowMode.Parallel4Splits,
            2 => DirectionalLight3D.ShadowMode.Parallel2Splits,
            _ => DirectionalLight3D.ShadowMode.Orthogonal
        };
    }
}
