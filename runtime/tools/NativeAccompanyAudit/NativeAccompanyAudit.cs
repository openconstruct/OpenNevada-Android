using Godot;
using OpenNV.Runtime.Content;
using OpenNV.Runtime.Gameplay.State;
using OpenNV.Runtime.World.Actors;
using OpenNV.Runtime.World.Cells;

public partial class NativeAccompanyAudit : Node3D
{
    public override async void _Ready()
    {
        RuntimeNativeCreature? actor = null;
        RuntimeNativeCreature? cold = null;
        RuntimeNativePlayer? player = null;
        StaticBody3D? floor = null;
        try
        {
            var args = OS.GetCmdlineUserArgs();
            if (args.Length != 3) throw new ArgumentException("Accompany audit needs owned root, ACRE hex and PACK hex.");
            RuntimeLiveContentSource.Configure(args[0], RuntimeLiveContentSource.FalloutNewVegasGame);
            using var content = RuntimeLiveContentSource.Current!;
            using var records = FalloutPluginStack.Load(content.PluginSources);
            var globals = FalloutGlobalState.Read(records);
            var package = records.GetEffective(records.RuntimeFormKey(Convert.ToUInt32(args[2], 16)));
            var follow = FalloutFollowPackage.Read(package);
            if (follow.Procedure != 7) throw new InvalidDataException("Accompany audit selected a non-Accompany package.");
            var key = records.RuntimeFormKey(Convert.ToUInt32(args[1], 16));
            var cell = FalloutCellSceneReader.Read(records, FalloutCellSceneReader.ParentCell(records.GetEffective(key))!.Value);
            using var world = new FalloutReferenceWorld(records);
            var placed = cell.References.Single(value => value.FormKey == key);
            world.LoadCell(cell);
            world.InitializeActorTemplates(key, 1, globals);
            var state = world.Get(key);
            state.Enabled = true;
            for (var parent = state; parent.EnableParent is { } link; parent = world.Get(link.Reference))
                parent.Enabled = !link.Opposite;
            actor = RuntimeNativeCreature.Create(records, content, placed, state, .0142875f);
            actor.Position = new(0, 2, 0);
            AddChild(actor);
            actor.SetPhysicsProcess(false);
            player = new RuntimeNativePlayer { Position = new(0, 0, -12) };
            AddChild(player);
            player.SetProcess(false);
            player.SetPhysicsProcess(false);
            player.SetProcessUnhandledInput(false);
            player.CollisionLayer = 2;
            player.AddChild(new CollisionShape3D
            {
                Position = Vector3.Up * .9f,
                Shape = new CapsuleShape3D { Height = 1.8f, Radius = .32f },
            });
            floor = new StaticBody3D { Position = new(0, -.05f, 0) };
            floor.AddChild(new CollisionShape3D { Shape = new BoxShape3D { Size = new(40, .1f, 40) } });
            AddChild(floor);
            var context = new NativeActorCombatContext(() => player, () => new(1, 200, 200, 70, 70, 0, 100),
                (_, _) => throw new InvalidOperationException("Accompany fixture attacked its target."),
                (_, to) => [to], _ => true, () => 1, globals, .4f, 9.81f);
            actor.Combat = RuntimeNativeActorCombat.Attach(actor, actor.Skeleton, actor.Appearance.SkeletonPath,
                world, state, records, content, 2, 3, context);
            actor.Combat.SetPhysicsProcess(false);
            for (var frame = 0; frame < 90; frame++)
            {
                await ToSignal(GetTree(), SceneTree.SignalName.PhysicsFrame);
                actor.Combat.AdvancePackageMotion(package, player.GlobalPosition, 100, true, 1d / 60);
            }
            if (!actor.IsOnFloor() || Math.Abs(actor.GlobalPosition.Y) > .02f)
                throw new InvalidOperationException($"Accompany did not settle its raised root: {actor.GlobalPosition}.");
            var origin = actor.GlobalPosition;
            var stop = follow.Distance * actor.Skeleton.UnitsToMetres;
            for (var frame = 0; frame < 600; frame++)
            {
                await ToSignal(GetTree(), SceneTree.SignalName.PhysicsFrame);
                actor.Combat.AdvancePackageMotion(package, player.GlobalPosition, stop, true, 1d / 60);
                if (actor.GlobalPosition.DistanceTo(player.GlobalPosition) <= stop) break;
            }
            var final = actor.GlobalPosition.DistanceTo(player.GlobalPosition);
            if (actor.GlobalPosition.DistanceTo(origin) < 3 || final > stop + .15f)
                throw new InvalidOperationException(
                    $"Accompany root motion did not stop at its source distance: origin={origin}, final={actor.GlobalPosition}, stop={stop}.");
            var saved = System.Text.Json.JsonSerializer.Deserialize<FalloutReferenceSnapshot[]>(
                System.Text.Json.JsonSerializer.Serialize(world.Capture()))!;
            using var coldWorld = new FalloutReferenceWorld(records);
            coldWorld.Restore(saved);
            coldWorld.LoadCell(cell);
            cold = RuntimeNativeCreature.Create(records, content, placed, coldWorld.Get(key), .0142875f);
            AddChild(cold);
            cold.SetPhysicsProcess(false);
            cold.Combat = RuntimeNativeActorCombat.Attach(cold, cold.Skeleton, cold.Appearance.SkeletonPath,
                coldWorld, coldWorld.Get(key), records, content, 2, 3, context);
            cold.Combat.SetPhysicsProcess(false);
            cold.Combat.RestorePackageMotion();
            if (cold.GlobalPosition.DistanceTo(actor.GlobalPosition) > .0001f)
                throw new InvalidOperationException("Cold Accompany lost its observed position.");
            GD.Print($"OPENNV_NATIVE_ACCOMPANY_PASS reference={key} package={package.FormKey} distance={stop:R} " +
                $"movement={origin.DistanceTo(actor.GlobalPosition):R} coldPose=true " +
                "motionOwner=shared-follow fixture=synthetic-open-floor ordinaryGameplay=separate");
            GetTree().Quit();
        }
        catch (Exception error) { GD.PushError(error.ToString()); GetTree().Quit(1); }
        finally { cold?.Free(); actor?.Free(); player?.Free(); floor?.Free(); }
    }
}
