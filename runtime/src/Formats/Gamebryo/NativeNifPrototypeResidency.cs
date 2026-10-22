namespace OpenNV.Runtime.Formats.Gamebryo;

internal static class NativeNifPrototypeResidency
{
    internal static void Trim(IDictionary<string, RuntimeNativeNifPrototype> prototypes, IReadOnlySet<string> retained)
    {
        foreach (var path in prototypes.Keys.Where(path => !retained.Contains(path)).ToArray())
        {
            prototypes[path].Scene.Root.Free();
            prototypes.Remove(path);
        }
    }
}
