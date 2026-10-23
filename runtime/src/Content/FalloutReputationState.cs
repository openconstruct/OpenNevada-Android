using System.Buffers.Binary;
using System.Globalization;

namespace OpenNV.Runtime.Content;

internal sealed record FalloutReputationSnapshot(FalloutFormKey Reputation, float Maximum, float Fame, float Infamy);

// Player-wide, independent fame and infamy counters. REPU DATA is a scale,
// never an initial standing. Only reached source commands mutate these counters.
internal sealed class FalloutReputationState(FalloutPluginStack records)
{
    private readonly Dictionary<FalloutFormKey, FalloutReputationSnapshot> _states = [];
    private float[]? _thresholds;

    private FalloutReputationSnapshot Require(FalloutFormKey key)
    {
        if (_states.TryGetValue(key, out var state)) return state;
        var record = records.GetEffective(key);
        if (record.Signature != "REPU") throw new InvalidDataException("Reputation target is not REPU.");
        var data = record.ReadSubrecords().Single(field => field.Signature == "DATA").Data;
        if (data.Length != 4) throw new InvalidDataException("REPU maximum is not Float32.");
        var maximum = BinaryPrimitives.ReadSingleLittleEndian(data.Span);
        if (!float.IsFinite(maximum) || maximum <= 0) throw new InvalidDataException("REPU maximum must be positive and finite.");
        state = new(key, maximum, 0, 0); _states.Add(key, state); return state;
    }

    internal float Raw(FalloutFormKey key, int axis)
    {
        var state = Require(key);
        return axis switch { 0 => state.Infamy, 1 => state.Fame, _ => throw new InvalidDataException("Reputation axis must be infamy 0 or fame 1.") };
    }
    internal float Percentage(FalloutFormKey key, int axis) => Raw(key, axis) / Require(key).Maximum;
    internal int Threshold(FalloutFormKey key, int axis)
    {
        _thresholds ??= Enumerable.Range(1, 3).Select(index => FalloutGameSettingFloats.Read(records,
            "fReputationThreshold" + new[] { "One", "Two", "Three" }[index - 1])).ToArray();
        if (_thresholds.Any(value => !float.IsFinite(value) || value <= 0 || value > 1) ||
            _thresholds[0] >= _thresholds[1] || _thresholds[1] >= _thresholds[2])
            throw new InvalidDataException("Reputation thresholds must increase within (0,1].");
        var fame = _thresholds.Count(value => Percentage(key, 1) >= value);
        var infamy = _thresholds.Count(value => Percentage(key, 0) >= value);
        return Rank(fame, infamy, axis);
    }
    internal static int Rank(int fame, int infamy, int axis)
    {
        if (fame is < 0 or > 3 || infamy is < 0 or > 3 || axis is < 0 or > 2)
            throw new InvalidDataException("Invalid reputation tier or threshold axis.");
        if (fame == 0 && infamy == 0) return 1;
        if (infamy == 0) return axis == 1 ? fame + 3 : 0;
        if (fame == 0) return axis == 2 ? infamy + 3 : 0;
        if (fame == infamy) return axis == 0 ? fame + 2 : 0;
        if (fame == 1) return axis == 2 ? infamy : 0;
        if (infamy == 1) return axis == 1 ? fame : 0;
        return axis == 0 ? 2 : 0; // Dark Hero / Soft-Hearted Devil.
    }
    internal void Change(FalloutFormKey key, int axis, float amount, string operation)
    {
        if (!float.IsFinite(amount) || amount < 0) throw new InvalidDataException("Reputation amount must be finite and nonnegative.");
        var current = Raw(key, axis); var state = Require(key);
        var value = operation switch
        {
            "setreputation" => amount,
            "addreputation" => current + amount,
            "removereputation" => current - amount,
            _ => throw new NotSupportedException("Unknown reputation mutation.")
        };
        value = Math.Clamp(value, 0, state.Maximum);
        _states[key] = axis == 1 ? state with { Fame = value } : state with { Infamy = value };
    }
    internal float? Evaluate(FalloutCondition condition) => condition.Function switch
    {
        573 => Raw(condition.FormArgument1, checked((int)condition.Argument2)),
        574 => Percentage(condition.FormArgument1, checked((int)condition.Argument2)),
        575 => Threshold(condition.FormArgument1, checked((int)condition.Argument2)),
        _ => null
    };
    internal IReadOnlyList<FalloutReputationSnapshot> Capture() => _states.Values.OrderBy(state => records.RuntimeFormId(state.Reputation)).ToArray();
    internal void Restore(IReadOnlyList<FalloutReputationSnapshot> snapshots)
    {
        if (_states.Count != 0) throw new InvalidOperationException("Reputation restore requires a fresh owner.");
        var validated = new FalloutReputationState(records);
        Validate(snapshots);
        foreach (var state in snapshots)
        {
            if (validated.Require(state.Reputation).Maximum != state.Maximum)
                throw new InvalidDataException("Saved reputation scale differs from winning REPU.");
            validated._states[state.Reputation] = state;
        }
        foreach (var pair in validated._states) _states.Add(pair.Key, pair.Value);
    }
    internal static void Validate(IReadOnlyList<FalloutReputationSnapshot> snapshots)
    {
        if (snapshots.Any(state => state is null) || snapshots.Select(state => state.Reputation).Distinct().Count() != snapshots.Count || snapshots.Any(state =>
            !float.IsFinite(state.Maximum) || state.Maximum <= 0 || !float.IsFinite(state.Fame) || !float.IsFinite(state.Infamy) ||
            state.Fame < 0 || state.Infamy < 0 || state.Fame > state.Maximum || state.Infamy > state.Maximum))
            throw new InvalidDataException("Saved reputation counters are invalid or duplicated.");
    }
    internal FalloutScriptFunction? Function(string name, FalloutScriptBindings bindings) => name.ToLowerInvariant() switch
    {
        "getreputation" or "getreputationpct" or "getreputationthreshold" => new(
            [FalloutScriptArgumentKind.Identifier, FalloutScriptArgumentKind.Number], arguments =>
            {
                var key = bindings.Form(arguments[0].Identifier!).FormKey;
                var axis = Axis(arguments[1].Number);
                return name.ToLowerInvariant() switch
                { "getreputation" => Raw(key, axis), "getreputationpct" => Percentage(key, axis), _ => Threshold(key, axis) };
            })
        { ReadOnly = true },
        _ => null
    };
    internal void Command(string operation, IReadOnlyList<string> arguments, FalloutScriptBindings bindings)
    {
        if (arguments.Count != 3) throw new InvalidDataException("Reputation command requires form, axis and amount.");
        Change(bindings.Form(arguments[0]).FormKey, Axis(double.Parse(arguments[1], CultureInfo.InvariantCulture)),
            float.Parse(arguments[2], CultureInfo.InvariantCulture), operation);
    }
    private static int Axis(double value) => double.IsFinite(value) && value == Math.Truncate(value)
        ? checked((int)value) : throw new InvalidDataException("Reputation axis is not an integer.");
}
