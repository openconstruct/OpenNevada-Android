using Godot;

namespace OpenNV.Runtime.Presentation.Rendering;

// Shader programs are immutable; material parameters remain instance-owned.
// IDs allow native materials to own program lifetime without a process-long pin.
internal static class NativeShaderResources
{
    private static readonly Dictionary<string, ulong> Programs = new(StringComparer.Ordinal);
    private static long _created, _reused, _requests;

    internal static object State
    {
        get
        {
            lock (Programs) return new
            {
                created = _created,
                reused = _reused,
                indexEntries = Programs.Count,
                accounting = "cumulative-program-requests;non-owning-index-not-resident-memory"
            };
        }
    }

    internal static Shader Load(string code)
    {
        lock (Programs)
        {
            if (++_requests % 64 == 0)
                foreach (var stale in Programs.Where(pair => !GodotObject.IsInstanceIdValid(pair.Value)).Select(pair => pair.Key).ToArray())
                    Programs.Remove(stale);
            if (Programs.TryGetValue(code, out var id) && GodotObject.IsInstanceIdValid(id) &&
                GodotObject.InstanceFromId(id) is Shader existing && GodotObject.IsInstanceValid(existing))
            {
                ++_reused;
                return existing;
            }
            var shader = new Shader { Code = code };
            Programs[code] = shader.GetInstanceId();
            ++_created;
            return shader;
        }
    }
}
