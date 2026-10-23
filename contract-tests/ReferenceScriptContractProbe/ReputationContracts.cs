using System.Text.Json;
using OpenNV.Runtime.Content;

internal static class ReputationContracts
{
    internal static void Run(FalloutPluginStack records)
    {
        var key = records.RuntimeFormKey(0xb00);
        var state = new FalloutReputationState(records);
        Require(state.Raw(key, 0) == 0 && state.Raw(key, 1) == 0 &&
            Enumerable.Range(0, 3).All(axis => state.Threshold(key, axis) == 1), "New reputation was not neutral on all axes.");
        foreach (var (amount, expected) in new[] { (14.99f, 1), (15f, 4), (49.99f, 4), (50f, 5), (99.99f, 5), (100f, 6) })
        {
            state.Change(key, 1, amount, "setreputation");
            Require(state.Threshold(key, 1) == expected && state.Raw(key, 0) == 0, "Threshold boundary or independent counter failed.");
        }
        state.Change(key, 0, 50, "addreputation");
        Require(state.Threshold(key, 0) == 2 && state.Threshold(key, 1) == 0 && state.Threshold(key, 2) == 0,
            "Mixed Dark Hero standing was reduced to a single-axis subtraction.");
        state.Change(key, 0, 1000, "addreputation"); state.Change(key, 1, 1000, "removereputation");
        Require(state.Percentage(key, 0) == 1 && state.Raw(key, 1) == 0 && state.Threshold(key, 2) == 6, "Reputation clamping failed.");
        var saved = JsonSerializer.Deserialize<FalloutReputationSnapshot[]>(JsonSerializer.Serialize(state.Capture()))!;
        var cold = new FalloutReputationState(records); cold.Restore(saved);
        Require(cold.Threshold(key, 2) == 6 && JsonSerializer.Serialize(cold.Capture()) == JsonSerializer.Serialize(saved), "Cold counters changed.");
        Reject(() => state.Raw(key, 2)); Reject(() => state.Threshold(key, 3));
        Reject(() => state.Change(key, 0, float.NaN, "addreputation"));
        Reject(() => state.Change(key, 0, -1, "setreputation"));
        Reject(() => new FalloutReputationState(records).Restore([saved[0], saved[0]]));
        Reject(() => new FalloutReputationState(records).Restore([saved[0] with { Maximum = 99 }]));
        Reject(() => new FalloutReputationState(records).Restore([saved[0] with { Infamy = float.PositiveInfinity }]));
        // All sixteen named standings on the mixed/good/bad query axes.
        int[][] expectedRanks = [[1,1,1], [0,4,0], [0,5,0], [0,6,0],
            [0,0,4], [3,0,0], [0,2,0], [0,3,0],
            [0,0,5], [0,0,2], [4,0,0], [2,0,0],
            [0,0,6], [0,0,3], [2,0,0], [5,0,0]];
        for (var infamy = 0; infamy < 4; infamy++)
            for (var fame = 0; fame < 4; fame++)
                for (var axis = 0; axis < 3; axis++)
                    Require(FalloutReputationState.Rank(fame, infamy, axis) == expectedRanks[infamy * 4 + fame][axis], "Named reputation matrix differs.");
        Console.WriteLine("OPENNV_REPUTATION_CONTRACT_PASS neutral=true boundaries=true independent=true clamped=true matrix=16 cold=true invalid=true");
    }
    private static void Require(bool condition, string message) { if (!condition) throw new InvalidOperationException(message); }
    private static void Reject(Action action)
    {
        try { action(); } catch (InvalidDataException) { return; }
        throw new InvalidOperationException("Invalid reputation state was accepted.");
    }
}
