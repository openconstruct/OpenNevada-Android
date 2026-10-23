using System.Text.Json;
using Godot;
using OpenNV.Runtime;
using OpenNV.Runtime.Content;
using OpenNV.Runtime.Gameplay.State;
using OpenNV.Runtime.World.Actors;
using OpenNV.Runtime.World.Cells;

public partial class NativeCheyenneAccompanyAudit : Node3D
{
    public override async void _Ready()
    {
        RuntimeNativeCreature? actor = null;
        RuntimeNativeCreature? cold = null;
        RuntimeNativeNpc? sunny = null;
        StaticBody3D? floor = null;
        try
        {
            var args = OS.GetCmdlineUserArgs();
            if (args.Length != 1) throw new ArgumentException("Cheyenne accompany audit needs one owned root.");
            RuntimeLiveContentSource.Configure(args[0], RuntimeLiveContentSource.FalloutNewVegasGame);
            using var content = RuntimeLiveContentSource.Current!;
            using var records = FalloutPluginStack.Load(content.PluginSources);
            var configuration = RuntimeConfiguration.Load();
            var units = configuration.World.GameUnitsToMeters;
            var globals = FalloutGlobalState.Read(records);
            var quests = new FalloutQuestState(records);
            var cell = FalloutCellSceneReader.Read(records, records.RuntimeFormKey(0x106185));
            var sunnyReference = cell.References.Single(value => value.FormKey == records.RuntimeFormKey(0x104e85));
            var cheyenneKey = records.RuntimeFormKey(0x10588e);
            var cheyenneReference = cell.References.Single(value => value.FormKey == cheyenneKey);
            using var world = new FalloutReferenceWorld(records);
            world.LoadCell(cell);
            world.InitializeActorTemplates(cheyenneKey, 1, globals);
            var state = world.Get(cheyenneKey);
            state.Enabled = true;
            for (var parent = state; parent.EnableParent is { } link; parent = world.Get(link.Reference))
                parent.Enabled = !link.Opposite;
            sunny = RuntimeNativeNpc.Create(records, content, sunnyReference, units, (_, _, _, _) => new StandardMaterial3D());
            AddChild(sunny);
            sunny.GlobalPosition = new(0, .1f, -8);
            sunny.SetProcess(false);
            sunny.SetPhysicsProcess(false);
            var sunnyState = world.Get(sunnyReference.FormKey);
            sunnyState.Enabled = true;
            for (var parent = sunnyState; parent.EnableParent is { } link; parent = world.Get(link.Reference))
                parent.Enabled = !link.Opposite;
            actor = RuntimeNativeCreature.Create(records, content, cheyenneReference, state, units);
            AddChild(actor);
            actor.GlobalPosition = new(0, .1f, 0);
            actor.SetProcess(false);
            actor.SetPhysicsProcess(false);
            floor = new StaticBody3D { Position = new(0, -.05f, 0) };
            floor.AddChild(new CollisionShape3D { Shape = new BoxShape3D { Size = new(40, .1f, 40) } });
            AddChild(floor);
            var context = new NativeActorCombatContext(() => null, () => new(1, 200, 200, 70, 70, 0, 100),
                (_, _) => throw new InvalidOperationException("Accompany fixture attacked its target."),
                (_, to) => [to], _ => true, () => 1, globals, .4f, 9.81f);
            actor.Combat = RuntimeNativeActorCombat.Attach(actor, actor.Skeleton, actor.Appearance.SkeletonPath,
                world, state, records, content, 2, 3, context);
            actor.Combat.SetPhysicsProcess(false);
            actor.ConfigureAi(records, quests, world, globals: globals);
            var package = records.GetEffective(records.RuntimeFormKey(0x152afa));
            var follow = FalloutFollowPackage.Read(package);
            if (follow.Procedure != 7 || follow.Target != records.RuntimeFormKey(0x104e85))
                throw new InvalidOperationException("Cheyenne Accompany target/procedure differs.");
            var stop = follow.Distance * units;
            var origin = actor.GlobalPosition;
            string? selected = null;
            for (var frame = 0; frame < 3000; frame++)
            {
                await ToSignal(GetTree(), SceneTree.SignalName.PhysicsFrame);
                actor._PhysicsProcess(1d / 60);
                var ai = JsonSerializer.SerializeToElement(actor.AiState);
                if (ai.GetProperty("error").GetString() is { } aiError)
                    throw new InvalidOperationException(aiError);
                selected = ai.GetProperty("package").GetString();
                if (selected is not null && actor.GlobalPosition.DistanceTo(sunny.GlobalPosition) <= stop) break;
            }
            if (selected != records.RuntimeFormKey(0x152afa).ToString())
                throw new InvalidOperationException(
                    $"Cheyenne selected {selected ?? "nothing"} instead of the authored Accompany package.");
            var final = actor.GlobalPosition.DistanceTo(sunny.GlobalPosition);
            if (actor.GlobalPosition.DistanceTo(origin) < 3 || final > stop + .15f)
                throw new InvalidOperationException(
                    $"Cheyenne Accompany did not stop at its source distance: origin={origin}, final={actor.GlobalPosition}, stop={stop}.");
            var saved = System.Text.Json.JsonSerializer.Deserialize<FalloutReferenceSnapshot[]>(
                System.Text.Json.JsonSerializer.Serialize(world.Capture()))!;
            using var coldWorld = new FalloutReferenceWorld(records);
            coldWorld.Restore(saved);
            coldWorld.LoadCell(cell);
            cold = RuntimeNativeCreature.Create(records, content, cheyenneReference, coldWorld.Get(cheyenneKey), units);
            AddChild(cold);
            cold.Combat = RuntimeNativeActorCombat.Attach(cold, cold.Skeleton, cold.Appearance.SkeletonPath,
                coldWorld, coldWorld.Get(cheyenneKey), records, content, 2, 3, context);
            cold.Combat.SetPhysicsProcess(false);
            cold.Combat.RestorePackageMotion();
            if (cold.GlobalPosition.DistanceTo(actor.GlobalPosition) > .0001f)
                throw new InvalidOperationException("Cold Cheyenne Accompany lost its observed position.");
            GD.Print($"OPENNV_NATIVE_CHEYENNE_ACCOMPANY_PASS reference={cheyenneKey} package={package.FormKey} " +
                $"distance={stop:R} movement={origin.DistanceTo(actor.GlobalPosition):R} sourceTarget={follow.Target} " +
                "selection=true coldPose=true fixture=synthetic-floor ordinaryGameplay=separate");
            GetTree().Quit();
        }
        catch (Exception error) { GD.PushError(error.ToString()); GetTree().Quit(1); }
        finally { cold?.Free(); actor?.Free(); sunny?.Free(); floor?.Free(); }
    }
}
