using Godot;
using OpenNV.Runtime.Content;
using OpenNV.Runtime.World.Cells;

namespace OpenNV.Runtime.World.Actors;

internal partial class RuntimeNativeNpc
{
    private FalloutDialoguePackage? _dialoguePackage;
    private bool _dialoguePackageRequested;
    private bool _dialogueWaitReached;
    private bool _dialogueTriggered;
    private FalloutPlacedReference? _dialogueTriggerReference;
    private FalloutReferencePrimitive? _dialogueTriggerPrimitive;
    private int _dialogueTriggerRadius;
    internal Action<FalloutDialoguePackage, Action>? BeginPackageDialogue { get; set; }

    private void BeginDialoguePackage(FalloutPluginRecord package)
    {
        var dialogue = FalloutDialoguePackage.Read(package);
        if (dialogue.Type != 0) throw new NotSupportedException("Dialogue SayTo package completion is unbound.");
        _dialoguePackage = dialogue;
        _dialoguePackageRequested = false;
        _dialogueWaitReached = false;
        _dialogueTriggered = false;
        _dialogueTriggerReference = null;
        _dialogueTriggerPrimitive = null;
        _dialogueTriggerRadius = 0;
        _aiPackage = package;
        if (dialogue.StartLocation is { } start)
        {
            switch (start.Type)
            {
                case FalloutPackageLocation.NearReference:
                    var wait = _aiCell!.References.SingleOrDefault(value => value.FormKey == start.Reference) ??
                        throw new NotSupportedException(
                            $"Dialogue start reference {start.Reference} is outside the active cell.");
                    StartTravel(package, wait);
                    break;
                case FalloutPackageLocation.NearEditorLocation:
                    var self = _aiCell!.References.SingleOrDefault(value => value.FormKey == Appearance.Reference) ??
                        throw new InvalidDataException("Dialogue editor location has no source placement.");
                    var destination = _referenceTransform!(self);
                    if (GlobalPosition.DistanceTo(destination.Origin) > start.Radius * Skeleton.UnitsToMetres)
                        StartTravelTo(package, self.FormKey, destination, "dialogue-editor-location");
                    else
                        _dialogueWaitReached = true;
                    break;
                default:
                    throw new NotSupportedException($"Dialogue start location type {start.Type} is unbound.");
            }
        }
        else
        {
            _dialogueWaitReached = true;
        }
        if (dialogue.TriggerLocation is { } trigger)
        {
            if (trigger.Type != FalloutPackageLocation.NearReference || trigger.Reference is not { } triggerId)
                throw new NotSupportedException($"Dialogue trigger location type {trigger.Type} is unbound.");
            _dialogueTriggerReference = _aiCell!.References.SingleOrDefault(value => value.FormKey == triggerId) ??
                throw new NotSupportedException(
                    $"Dialogue trigger reference {triggerId} is outside the active cell.");
            _dialogueTriggerRadius = trigger.Radius;
            if (trigger.Radius == 0)
                _dialogueTriggerPrimitive = FalloutReferencePrimitive.Read(_aiStack!.GetEffective(triggerId)) ??
                    throw new InvalidDataException($"Dialogue trigger reference {triggerId} has no source volume.");
        }
        _packageEvents!.Change(_packageIdleSource);
        GD.Print($"OPENNV_NATIVE_DIALOGUE_PACKAGE reference={Appearance.Reference} package={package.FormKey} " +
            $"start={dialogue.StartLocation?.Type.ToString() ?? "none"} trigger={dialogue.TriggerLocation?.Type.ToString() ?? "none"} " +
            $"radius={_dialogueTriggerRadius} target={dialogue.Target} activation={dialogue.ActivationDistance}");
    }

    private void AdvanceDialoguePackage()
    {
        if (_dialoguePackage is not { } dialogue || _dialoguePackageRequested || _aiError is not null) return;
        if (!_dialogueWaitReached)
        {
            if (_travelActive) return;
            _dialogueWaitReached = true;
        }
        var targetNode = DialogueTargetNode(dialogue.Target);
        if (targetNode is null) return;
        if (_dialogueTriggerReference is { } trigger && !_dialogueTriggered)
        {
            var point = targetNode is RuntimeNativePlayer player
                ? player.CombatTargetPoint
                : targetNode.GlobalPosition;
            if (!DialogueTriggerContains(trigger, point)) return;
            _dialogueTriggered = true;
            GD.Print($"OPENNV_NATIVE_DIALOGUE_TRIGGER reference={Appearance.Reference} package={dialogue.Form} " +
                $"trigger={trigger.FormKey} target={dialogue.Target}");
        }
        var target = targetNode.GlobalPosition;
        if (GlobalPosition.DistanceTo(target) > dialogue.ActivationDistance * Skeleton.UnitsToMetres)
        {
            if (!_travelActive)
                StartTravelTo(_aiPackage!, dialogue.Target, new(Basis, GetParent<Node3D>().ToLocal(target)), "dialogue-target");
            return;
        }
        _travelActive = false;
        PlayLocomotion(false);
        _dialoguePackageRequested = true;
        (BeginPackageDialogue ?? throw new NotSupportedException("Dialogue package has no conversation owner."))(dialogue, () =>
        {
            if (_dialoguePackage?.Form == dialogue.Form) _packageEvents!.Complete();
        });
    }

    private bool DialogueTriggerContains(FalloutPlacedReference trigger, Vector3 point)
    {
        var transform = _referenceTransform!(trigger);
        if (_dialogueTriggerRadius > 0)
            return transform.Origin.DistanceTo(point) <= _dialogueTriggerRadius * Skeleton.UnitsToMetres;
        var primitive = _dialogueTriggerPrimitive!;
        var local = transform.AffineInverse() * point;
        // Placed-reference primitive dimensions stay in source local axes;
        // Godot maps source (X, Y, Z) to local (X, Z, Y).
        return Mathf.Abs(local.X) <= primitive.X * Skeleton.UnitsToMetres &&
            Mathf.Abs(local.Y) <= primitive.Z * Skeleton.UnitsToMetres &&
            Mathf.Abs(local.Z) <= primitive.Y * Skeleton.UnitsToMetres;
    }

    private Node3D? DialogueTargetNode(FalloutFormKey target)
    {
        if (_aiStack!.RuntimeFormId(target) == 0x14)
            return Combat?.PackagePlayer ??
                GetTree().Root.FindChildren("*", "", true, false).OfType<RuntimeNativePlayer>().SingleOrDefault();
        return GetTree().Root.FindChildren("*", "", true, false).SingleOrDefault(node =>
            node is RuntimeNativeNpc npc && npc.Appearance.Reference == target ||
            node is RuntimeNativeCreature creature && creature.Appearance.Reference == target) as Node3D;
    }
}
