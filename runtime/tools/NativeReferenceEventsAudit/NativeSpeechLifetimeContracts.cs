using System.Reflection;
using Godot;
using OpenNV.Runtime.World.Actors;

internal static class NativeSpeechLifetimeContracts
{
    internal static void Run(Node host)
    {
        const BindingFlags flags = BindingFlags.Instance | BindingFlags.NonPublic;
        var speakerField = typeof(RuntimeNativeSpeech).GetField("_speaker", flags)!;
        var creatureField = typeof(RuntimeNativeSpeech).GetField("_creatureSpeaker", flags)!;
        var release = typeof(RuntimeNativeSpeech).GetMethod("ReleaseSpeakerPresentation", flags)!;
        var speech = new RuntimeNativeSpeech();
        var retired = new RuntimeNativeNpc { ProcessMode = Node.ProcessModeEnum.Disabled };
        var detached = new RuntimeNativeNpc { ProcessMode = Node.ProcessModeEnum.Disabled };
        var creature = new RuntimeNativeCreature { ProcessMode = Node.ProcessModeEnum.Disabled };
        try
        {
            host.AddChild(retired);
            retired.AddChild(new MeshInstance3D());
            // A door frees the old actor and all its mesh children while the
            // session-level voice owner survives. Cleanup must not publish poses.
            speakerField.SetValue(speech, retired);
            creatureField.SetValue(speech, creature);
            retired.Free();
            release.Invoke(speech, null);
            RequireReleased();
            // Detached presentation also cannot receive face publication.
            speakerField.SetValue(speech, detached);
            release.Invoke(speech, null);
            RequireReleased();
            release.Invoke(speech, null);
            RequireReleased();
            GD.Print("OPENNV_NATIVE_SPEECH_LIFETIME_PASS retiredActor=true detachedActor=true releasedReferences=true repeatedRelease=true");
        }
        finally
        {
            if (GodotObject.IsInstanceValid(retired)) retired.Free();
            detached.Free(); creature.Free(); speech.Free();
        }
        void RequireReleased()
        {
            if (speakerField.GetValue(speech) is not null || creatureField.GetValue(speech) is not null)
                throw new InvalidOperationException("Completed speech retained a presentation actor.");
        }
    }
}
