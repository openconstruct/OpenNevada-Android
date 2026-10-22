using System.Runtime.CompilerServices;
using System.Security.Cryptography;
using Godot;
using OpenNV.Runtime.Content;

namespace OpenNV.Runtime.Presentation.Rendering;

internal static partial class NativeDdsTexture
{
    // IDs do not own native resources. Materials keep textures alive; this index
    // must not defeat scene/prototype eviction by holding a strong reference.
    private readonly record struct TextureKey(string Source, string Sha256, bool Normal, Type ResourceType);
    private sealed record TextureEntry(ulong InstanceId, long UploadBytes);
    private sealed class TextureIndex
    {
        internal readonly Dictionary<TextureKey, TextureEntry> Entries = [];
        internal long Uploads, Reuses, UploadedPayloadBytes, ReusedPayloadBytes, ExpandedBc1Uploads;
        internal long ExpandedBc1SourceBytes, ExpandedBc1UploadBytes;
        internal long CubemapUploads, CubemapReuses;
    }
    private static readonly ConditionalWeakTable<RuntimeLiveContentSource, TextureIndex> TextureIndexes = new();

    internal static object? SharingState
    {
        get
        {
            if (RuntimeLiveContentSource.Current is not { } source || !TextureIndexes.TryGetValue(source, out var index)) return null;
            lock (index)
                return new
                {
                    uploads = index.Uploads,
                    reuses = index.Reuses,
                    indexEntries = index.Entries.Count,
                    uploadedPayloadBytes = index.UploadedPayloadBytes,
                    reusedPayloadBytes = index.ReusedPayloadBytes,
                    expandedBc1Uploads = index.ExpandedBc1Uploads,
                    expandedBc1SourceBytes = index.ExpandedBc1SourceBytes,
                    expandedBc1UploadBytes = index.ExpandedBc1UploadBytes,
                    cubemapUploads = index.CubemapUploads,
                    cubemapReuses = index.CubemapReuses,
                    accounting = "cumulative-upload-payloads-not-resident-or-driver-memory"
                };
        }
    }

    private static T LoadShared<T>(byte[] payload, string source, bool normal, Func<byte[], string, bool, T> create) where T : Resource
    {
        if (RuntimeLiveContentSource.Current is not { } owner) return create(payload, source, normal);
        // Hash the actual winning bytes, not just the path or byte[] identity.
        // A changed loose file or differently resolved archive cannot reuse stale pixels.
        var key = new TextureKey(source, Convert.ToHexString(SHA256.HashData(payload)), normal, typeof(T));
        var index = TextureIndexes.GetValue(owner, static _ => new());
        lock (index)
        {
            if (index.Entries.TryGetValue(key, out var entry) && GodotObject.IsInstanceIdValid(entry.InstanceId) &&
                GodotObject.InstanceFromId(entry.InstanceId) is T existing && GodotObject.IsInstanceValid(existing))
            {
                ++index.Reuses;
                if (typeof(T) == typeof(Cubemap)) ++index.CubemapReuses;
                index.ReusedPayloadBytes += entry.UploadBytes;
                return existing;
            }
            var texture = create(payload, source, normal);
            var bytes = texture.GetMeta("opennv_dds_upload_payload_bytes", 0L).AsInt64();
            ++index.Uploads;
            if (typeof(T) == typeof(Cubemap)) ++index.CubemapUploads;
            index.UploadedPayloadBytes += bytes;
            if (texture.GetMeta("opennv_dds_expanded_bc1", false).AsBool())
            {
                ++index.ExpandedBc1Uploads;
                index.ExpandedBc1SourceBytes += texture.GetMeta("opennv_dds_source_payload_bytes", 0L).AsInt64();
                index.ExpandedBc1UploadBytes += bytes;
            }
            index.Entries[key] = new(texture.GetInstanceId(), bytes);
            // Bound stale metadata without retaining a managed wrapper merely to
            // inspect it. Live entries remain owned by ordinary scene resources.
            if (index.Uploads % 64 == 0)
                foreach (var stale in index.Entries.Where(pair => !GodotObject.IsInstanceIdValid(pair.Value.InstanceId)).Select(pair => pair.Key).ToArray())
                    index.Entries.Remove(stale);
            return texture;
        }
    }
}
