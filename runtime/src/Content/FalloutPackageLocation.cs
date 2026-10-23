using System.Buffers.Binary;

namespace OpenNV.Runtime.Content;

// PLDT/PLD2 share one layout: a location kind, a kind-specific value and an
// author radius. Movement owners consume the kinds they implement; the rest
// remain explicit source failures instead of an invented destination.
internal sealed record FalloutPackageLocation(int Type, FalloutFormKey? Reference, int Radius)
{
    internal const int NearReference = 0;
    internal const int NearEditorLocation = 3;

    internal static FalloutPackageLocation? Read(FalloutPluginRecord package, string signature)
    {
        var fields = package.ReadSubrecords().Where(field => field.Signature == signature).ToArray();
        if (fields.Length == 0) return null;
        // Authored PLD2 is serialized twice with identical bytes in every
        // winning record that has it. A different second copy stays ambiguous.
        if (fields.Length > 2 || fields.Any(field => field.Data.Length != 12) ||
            fields.Length == 2 && !fields[0].Data.Span.SequenceEqual(fields[1].Data.Span))
            throw new InvalidDataException($"PACK {package.FormKey} has an invalid or duplicated {signature}.");
        var bytes = fields[0].Data.Span;
        var type = BinaryPrimitives.ReadInt32LittleEndian(bytes);
        var raw = BinaryPrimitives.ReadUInt32LittleEndian(bytes[4..]);
        var radius = BinaryPrimitives.ReadInt32LittleEndian(bytes[8..]);
        if (type is < NearReference or > 7)
            throw new NotSupportedException($"PACK {package.FormKey} {signature} location type {type} is unbound.");
        if (radius < 0) throw new InvalidDataException($"PACK {package.FormKey} {signature} radius is negative.");
        FalloutFormKey? reference = type == NearReference
            ? package.Plugin.AdjustOptionalFormId(raw) ??
                throw new InvalidDataException($"PACK {package.FormKey} {signature} near-reference target is absent.")
            : null;
        return new(type, reference, radius);
    }
}
