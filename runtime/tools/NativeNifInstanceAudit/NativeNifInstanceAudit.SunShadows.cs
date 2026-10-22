using Godot;
using OpenNV.Runtime.Content;
using OpenNV.Runtime.Gameplay.State;
using OpenNV.Runtime.Presentation.Rendering;

public partial class NativeNifInstanceAudit
{
    private async Task ExerciseSunShadowPolicy(string root)
    {
        RuntimeLiveContentSource.Configure(root, RuntimeLiveContentSource.FalloutNewVegasGame);
        using var source = RuntimeLiveContentSource.Current!;
        using var records = FalloutPluginStack.Load(source.PluginSources);
        var sky = new FalloutSkyLightingState(records, FalloutGameSettingFloats.Read(records, "fDaytimeColorExtension"));
        var initial = System.Text.Json.JsonSerializer.Serialize(sky.Capture());
        var scene = new Node3D();
        AddChild(scene);
        var environment = new RuntimeNativeExteriorEnvironment();
        environment.Configure(records, sky, () => 12, .0142875f);
        scene.AddChild(environment);
        try
        {
            var sun = environment.GetChildren().OfType<DirectionalLight3D>().Single();
            var color = sun.LightColor; var energy = sun.LightEnergy; var basis = sun.Basis;
            GetTree().Paused = true;
            try
            {
                foreach (var count in new[] { 0, 4, 2, 1, 0 })
                {
                    RuntimeSunShadows.Configure(["--sun-shadows", count.ToString(System.Globalization.CultureInfo.InvariantCulture)]);
                    Require(sun.ShadowEnabled == (count != 0), "Live shadow quality missed the paused source sun.");
                    Require(sun.DirectionalShadowMode == (count == 4 ? DirectionalLight3D.ShadowMode.Parallel4Splits :
                        count == 2 ? DirectionalLight3D.ShadowMode.Parallel2Splits : DirectionalLight3D.ShadowMode.Orthogonal), "Cascade selection differs.");
                    Require(sun.LightColor == color && sun.LightEnergy == energy && sun.Basis == basis &&
                        System.Text.Json.JsonSerializer.Serialize(sky.Capture()) == initial, "Shadow quality changed source sunlight or authoritative sky state.");
                }
            }
            finally { GetTree().Paused = false; }
            foreach (var invalid in new[] { new[] { "--sun-shadows" }, new[] { "--sun-shadows", "3" }, new[] { "--sun-shadows", "NaN" } })
            {
                try { RuntimeSunShadows.Configure(invalid); throw new InvalidOperationException("Invalid shadow policy admitted."); }
                catch (ArgumentException) { }
            }
        }
        finally { scene.Free(); RuntimeSunShadows.Configure([]); }
        // A detached environment must unsubscribe before future changes.
        RuntimeSunShadows.Set(2);
        await ToSignal(GetTree(), SceneTree.SignalName.ProcessFrame);
        RuntimeSunShadows.Configure([]);
        Require(RuntimeSunShadows.Cascades == 4, "Desktop quality default changed.");
        GD.Print("OPENNV_SUN_SHADOW_POLICY_PASS ownedWeather=true pausedChanges=true lightingRetained=true invalidRejected=true detachedOwnerReleased=true defaultFour=true");
    }
}
