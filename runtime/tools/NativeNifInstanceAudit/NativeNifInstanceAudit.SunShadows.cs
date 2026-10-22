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
            var fog = environment.GetChildren().OfType<WorldEnvironment>().Single().Environment;
            var sourceFogBegin = fog.FogDepthBegin; var sourceFogEnd = fog.FogDepthEnd;
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
                var camera = new Camera3D { Near = .05f, Far = 1000 };
                scene.AddChild(camera);
                RuntimeViewDistance.Configure(["--view-distance-meters", "200"]);
                RuntimeViewDistance.Apply(camera, 3571);
                Require(camera.Far == 200 && fog.FogDepthEnd <= 190.001f && fog.FogDepthBegin < fog.FogDepthEnd,
                    "Handheld view distance missed camera clipping or source-material fog while paused.");
                Require(System.Text.Json.JsonSerializer.Serialize(sky.Capture()) == initial,
                    "Presentation distance changed authoritative source weather.");
                var scaledFog = RuntimeViewDistance.Fog(10000, 20000, .01f);
                Require(Math.Abs(scaledFog.Far * .01f - 190) < .001f && scaledFog.Near < scaledFog.Far,
                    "View distance did not convert metres to source fog units.");
                RuntimeViewDistance.Configure([]);
                RuntimeViewDistance.Apply(camera, 3571);
                Require(camera.Far == 3571 && fog.FogDepthBegin == sourceFogBegin && fog.FogDepthEnd == sourceFogEnd,
                    "Removing the presentation cap did not restore original camera/fog distance.");
            }
            finally { GetTree().Paused = false; }
            foreach (var invalid in new[] { new[] { "--sun-shadows" }, new[] { "--sun-shadows", "3" }, new[] { "--sun-shadows", "NaN" } })
            {
                try { RuntimeSunShadows.Configure(invalid); throw new InvalidOperationException("Invalid shadow policy admitted."); }
                catch (ArgumentException) { }
            }
            foreach (var value in new[] { "0", "-1", "NaN", "Infinity" })
            {
                try { RuntimeViewDistance.Configure(["--view-distance-meters", value]); throw new InvalidOperationException("Invalid distance admitted."); }
                catch (ArgumentOutOfRangeException) { }
            }
        }
        finally { scene.Free(); RuntimeSunShadows.Configure([]); RuntimeViewDistance.Configure([]); }
        // A detached environment must unsubscribe before future changes.
        RuntimeSunShadows.Set(2);
        RuntimeViewDistance.Set(200);
        await ToSignal(GetTree(), SceneTree.SignalName.ProcessFrame);
        RuntimeSunShadows.Configure([]);
        RuntimeViewDistance.Configure([]);
        Require(RuntimeSunShadows.Cascades == 4, "Desktop quality default changed.");
        GD.Print("OPENNV_SUN_SHADOW_POLICY_PASS ownedWeather=true pausedChanges=true lightingRetained=true invalidRejected=true detachedOwnerReleased=true defaultFour=true viewDistance=200 fogUnits=true sourceDistanceRestored=true");
    }
}
