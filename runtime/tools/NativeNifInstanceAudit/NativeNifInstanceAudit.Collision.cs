using Godot;
using OpenNV.Runtime.Content;
using OpenNV.Runtime.Formats.Gamebryo;

public partial class NativeNifInstanceAudit
{
    private static void ExercisePackedCollision()
    {
        var data = new FalloutNifPackedData(new(0, "hkPackedNiTriStripsData", 0, 0),
            [new(1, 0, 0), new(0, 1, 0), new(0, 0, 1), new(1, 1, 1)],
            [new(2, 0, 1, 0, default), new(3, 1, 0, 0, default)], []);
        using var shape = NativeNifCollisionBuilder.CreatePackedShape(data, new(2, 3, 4), .5f);
        Vector3[] expected = [new(0, 14, 0), new(7, 0, 0), new(0, 0, -10.5f),
            new(7, 14, -10.5f), new(0, 0, -10.5f), new(7, 0, 0)];
        if (!shape.GetFaces().SequenceEqual(expected) || !shape.BackfaceCollision)
            throw new InvalidDataException("Packed collision changed source face order, scale, axes or sidedness.");
        foreach (var invalid in new[]
        {
            data with { Triangles = [] },
            data with { Triangles = [new(0, 1, 4, 0, default)] },
            data with { Vertices = [new(float.NaN, 0, 0), .. data.Vertices[1..]] },
        })
        {
            try
            {
                using var unexpected = NativeNifCollisionBuilder.CreatePackedShape(invalid, new(1, 1, 1), 1);
            }
            catch (InvalidDataException) { continue; }
            throw new InvalidDataException("Invalid packed collision was admitted.");
        }
        GD.Print("OPENNV_PACKED_COLLISION_PASS sourceOrder=true axes=true scale=true backfaces=true invalidRefused=true");
    }

    private static void ExerciseOwnedPackedCollision(string root, string model)
    {
        RuntimeLiveContentSource.Configure(root, RuntimeLiveContentSource.FalloutNewVegasGame);
        using var content = RuntimeLiveContentSource.Current!;
        if (!content.TryRead(model, null, out var bytes, out _)) throw new FileNotFoundException(model);
        var source = FalloutNifFile.Read(bytes);
        var shapes = 0;
        var faces = 0;
        foreach (var block in source.Blocks.Where(block => block.TypeName == "bhkPackedNiTriStripsShape"))
        {
            var packed = (FalloutNifPackedShape)source.ReadObject(block.Index);
            var data = (FalloutNifPackedData)source.ReadObject(packed.Data);
            using var shape = NativeNifCollisionBuilder.CreatePackedShape(data, packed.Scale, .0142875f);
            var actual = shape.GetFaces();
            if (actual.Length != data.Triangles.Length * 3 || actual.Any(vertex => !vertex.IsFinite()))
                throw new InvalidDataException("Owned packed collision lost source faces.");
            shapes++;
            faces += actual.Length / 3;
        }
        if (shapes == 0) throw new InvalidDataException("Owned model has no packed collision to exercise.");
        GD.Print($"OPENNV_OWNED_PACKED_COLLISION_PASS model={model} shapes={shapes} triangles={faces} gameplay=unverified");
    }
}
