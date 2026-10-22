using Godot;

namespace OpenNV.Runtime.Presentation.Rendering;

// These APIs copy their input Variant. Release our temporary native references
// immediately instead of waiting for a managed finalizer to release large data.
internal static class NativeVariant
{
    internal static void SetArrayElement<[MustBeVariant] T>(Godot.Collections.Array array, int index, T value)
    {
        using var argument = Variant.From(value);
        array[index] = argument;
    }

    internal static void SetShaderParameter<[MustBeVariant] T>(ShaderMaterial material, StringName name, T value)
    {
        using var argument = Variant.From(value);
        material.SetShaderParameter(name, argument);
    }
}
