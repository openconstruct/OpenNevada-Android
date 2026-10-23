using System.Text.Json;
using Godot;
using OpenNV.Runtime;
using OpenNV.Runtime.Content;
using OpenNV.Runtime.Formats.Gamebryo;
using OpenNV.Runtime.World.Actors;
using OpenNV.Runtime.World.Cells;

public partial class NativeSunnyDialoguePackageAudit : Node3D
{
    public override void _Ready()
    {
        RuntimeNativeNpc? actor = null;
        RuntimeNativePlayer? player = null;
        try
        {
            var args = OS.GetCmdlineUserArgs();
            if (args.Length != 1) throw new ArgumentException("Sunny dialogue audit needs one owned root.");
            RuntimeLiveContentSource.Configure(args[0], RuntimeLiveContentSource.FalloutNewVegasGame);
            using var content = RuntimeLiveContentSource.Current!;
            using var records = FalloutPluginStack.Load(content.PluginSources);
            var configuration = RuntimeConfiguration.Load();
            var units = configuration.World.GameUnitsToMeters;
            var cell = FalloutCellSceneReader.Read(records, records.RuntimeFormKey(0x106185));
            var sunny = cell.References.Single(reference => reference.FormKey == records.RuntimeFormKey(0x104e85));
            var trigger = cell.References.Single(reference => reference.FormKey == records.RuntimeFormKey(0x15d9ba));
            Transform3D Placement(FalloutPlacedReference reference) => new(
                GamebryoCoordinate.ConvertReferenceEuler(
                    new(reference.RotationRadians[0], reference.RotationRadians[1], reference.RotationRadians[2]),
                    reference.Scale),
                GamebryoCoordinate.ConvertVector(new(reference.Position[0], reference.Position[1], reference.Position[2])) * units);
            Transform3D SourceFloor(float x, float y, float z) => new(Basis.Identity,
                GamebryoCoordinate.ConvertVector(new(x, y, z)) * units);
            var outside = SourceFloor(trigger.Position[0], trigger.Position[1] - 700, sunny.Position[2]);
            var inside = SourceFloor(trigger.Position[0], trigger.Position[1] - 250, sunny.Position[2]);
            var quests = new FalloutQuestState(records);
            actor = RuntimeNativeNpc.Create(records, content, sunny, units, (_, _, _, _) => new StandardMaterial3D());
            AddChild(actor);
            actor.Transform = Placement(sunny);
            actor.ConfigureAi(records, quests, cell, Placement);
            player = new RuntimeNativePlayer();
            AddChild(player);
            player.Configure(configuration, outside);
            player.SetPhysicsProcess(false);
            Action? complete = null;
            var requests = 0;
            actor.BeginPackageDialogue = (request, completed) =>
            {
                if (request.Form != records.RuntimeFormKey(0x105cc7))
                    throw new InvalidOperationException("Audit selected a different source package.");
                if (actor.Traveling || actor.SittingState != 0 ||
                    actor.GlobalPosition.DistanceTo(player.GlobalPosition) > request.ActivationDistance * units)
                    throw new InvalidOperationException("Sunny dialogue package did not travel into its source target range.");
                if (request.TriggerLocation is null || request.StartLocation is null)
                    throw new InvalidOperationException("Sunny package locations were not retained.");
                requests++;
                complete = completed;
            };
            for (var frame = 0; frame < 600; frame++) actor._Process(1d / 60);
            if (actor.AiError is not null || actor.AnimationError is not null)
                throw new InvalidOperationException(actor.AiError ?? actor.AnimationError);
            if (actor.CurrentPackage != records.RuntimeFormKey(0x105cc7) || actor.Traveling || requests != 0)
                throw new InvalidOperationException("Sunny moved before the player entered the source trigger.");
            player.Teleport(inside);
            for (var frame = 0; frame < 7200 && requests == 0; frame++)
            {
                actor._Process(1d / 60);
                if (actor.AiError is not null || actor.AnimationError is not null)
                    throw new InvalidOperationException(actor.AiError ?? actor.AnimationError);
            }
            if (requests != 1 || complete is null)
                throw new InvalidOperationException("Sunny did not request one conversation after the trigger.");
            bool Done() => JsonSerializer.SerializeToElement(actor.AiState).GetProperty("packageEvents").GetProperty("Done").GetBoolean();
            if (Done()) throw new InvalidOperationException("Sunny package completed before its conversation.");
            complete();
            for (var frame = 0; frame < 120; frame++) actor._Process(1d / 60);
            if (!Done() || requests != 1) throw new InvalidOperationException("Sunny package lost or repeated completion.");
            GD.Print("OPENNV_SUNNY_DIALOGUE_AUDIT_PASS package=FalloutNV.esm:105cc7 " +
                "start=near-editor trigger=near-reference target=FalloutNV.esm:14 " +
                "outsideNoMotion=true triggerGated=true ownedNavm=true targetRange=true requestOnce=true " +
                "completionAfterConversation=true pixels=unverified");
            GetTree().Quit();
        }
        catch (Exception error) { GD.PushError(error.ToString()); GetTree().Quit(1); }
        finally { actor?.Free(); player?.Free(); }
    }
}
