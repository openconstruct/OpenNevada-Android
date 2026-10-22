using Godot;
using OpenNV.Runtime;
using OpenNV.Runtime.Campaigns.NewVegas.Opening;
using OpenNV.Runtime.Content;
using OpenNV.Runtime.InputSystem;
using OpenNV.Runtime.Presentation.Ui;

public partial class NativeNifInstanceAudit
{
    private async Task ExerciseCreationChoices(string root)
    {
        var input = RuntimeConfiguration.Load().Player.DesktopInput;
        DesktopInputMap.Configure(input); GamepadInputMap.Configure(input);
        RuntimeLiveContentSource.Configure(root, RuntimeLiveContentSource.FalloutNewVegasGame);
        using var source = RuntimeLiveContentSource.Current!;
        using var records = FalloutPluginStack.Load(source.PluginSources);
        var controls = FalloutOpeningPlayerControlResolver.Resolve(records, ["VCG00", "VCG01"]);
        var cell = FalloutCellSceneReader.Read(records, FalloutDialogueTopic.Find(records, "CELL", "GSDocMitchellHouse").FormKey);
        var tags = FalloutNativeTagSkillResolver.Resolve(records, controls);
        var traits = FalloutNativeTraitFarewellResolver.Resolve(records, controls, cell);
        var tagEntry = new RuntimeNativeTagSkillEntry();
        IReadOnlyList<FalloutNativeSkillIdentity>? acceptedTags = null;
        AddChild(tagEntry); tagEntry.Configure(tags, []);
        tagEntry.Accepted += selection => acceptedTags = selection;
        try
        {
            await Frame(); await Frame(); Focus("Choice_0");
            await Press(JoyButton.X); Require(acceptedTags is null, "Incomplete tags accepted through X.");
            for (var index = 0; index < tags.RequiredCount; index++)
            {
                if (index != 0) await Press(JoyButton.DpadDown);
                await Press(JoyButton.A);
            }
            Require(Selected(tagEntry) == tags.RequiredCount, "A did not select the required tag skills.");
            await Press(JoyButton.DpadDown); await Press(JoyButton.A);
            Require(Selected(tagEntry) == tags.RequiredCount, "Tags exceeded their source selection limit.");
            await Press(JoyButton.DpadUp); await Press(JoyButton.A);
            Require(Selected(tagEntry) == tags.RequiredCount - 1, "A did not deselect a tag skill.");
            await Press(JoyButton.X); Require(acceptedTags is null, "Deselecting a tag left confirmation enabled.");
            await Press(JoyButton.A); await Press(JoyButton.X);
            Require(acceptedTags is not null && acceptedTags.Count == tags.RequiredCount, "X did not accept complete tags.");
            FalloutNativeTagSkillResolver.Validate(tags, acceptedTags!);
        }
        finally { tagEntry.Free(); }
        var traitEntry = new RuntimeNativeTraitEntry();
        IReadOnlyList<FalloutNativeTraitIdentity>? acceptedTraits = null;
        AddChild(traitEntry); traitEntry.Configure(traits, []);
        traitEntry.Accepted += selection => acceptedTraits = selection;
        try
        {
            await Frame(); await Frame(); Focus("Choice_0");
            await Press(JoyButton.X);
            Require(acceptedTraits is { Count: 0 }, "Optional traits could not be confirmed empty.");
            acceptedTraits = null;
            for (var index = 0; index < traits.MaximumTraits; index++)
            {
                if (index != 0) await Press(JoyButton.DpadDown);
                await Press(JoyButton.A);
            }
            await Press(JoyButton.DpadDown); await Press(JoyButton.A);
            Require(Selected(traitEntry) == traits.MaximumTraits, "Traits exceeded their source selection limit.");
            await Press(JoyButton.DpadUp); await Press(JoyButton.A);
            Require(Selected(traitEntry) == traits.MaximumTraits - 1, "A did not deselect a trait.");
            await Press(JoyButton.A);
            for (var index = traits.MaximumTraits - 1; index < traits.Traits.Count; index++) await Press(JoyButton.DpadDown);
            Focus("ConfirmChoices"); await Press(JoyButton.A);
            Require(acceptedTraits is not null && acceptedTraits.Count == traits.MaximumTraits, "A did not activate trait confirmation.");
            FalloutNativeTraitFarewellResolver.ValidateTraits(traits, acceptedTraits!);
            await Press(JoyButton.DpadDown); Focus("Choice_0");
        }
        finally { traitEntry.Free(); }
        var longMenu = new NativeMultiSelectMenu("Scroll navigation", Enumerable.Range(0, 80).Select(index => index.ToString()).ToArray(), "Confirm");
        AddChild(longMenu);
        try
        {
            await Frame(); await Frame();
            for (var index = 0; index < 79; index++) await Press(JoyButton.DpadDown);
            Focus("Choice_79"); await Frame(); await Frame();
            var scroll = longMenu.FindChildren("*", "ScrollContainer", true, false).Cast<ScrollContainer>().Single();
            var focused = GetViewport().GuiGetFocusOwner()!;
            Require(scroll.ScrollVertical > 0 && scroll.GetGlobalRect().Intersects(focused.GetGlobalRect()), "Controller focus did not reveal the scrolled choice.");
        }
        finally { longMenu.Free(); }
        GD.Print($"OPENNV_CREATION_CHOICES_PASS tags={tags.Skills.Count} traits={traits.Traits.Count} initialFocus=true dpad=true selectDeselect=true limits=true optionalZero=true confirmA=true confirmX=true scroll=true ordinaryGameplay=separate");

        static int Selected(Node entry) => entry.FindChildren("Choice_*", "Button", true, false).Cast<Button>().Count(button => button.ButtonPressed);
        void Focus(string name) => Require(GetViewport().GuiGetFocusOwner()?.Name == name, $"Expected {name}, got {GetViewport().GuiGetFocusOwner()?.Name}.");
        async Task Frame() => await ToSignal(GetTree(), SceneTree.SignalName.ProcessFrame);
        async Task Press(JoyButton button)
        {
            using var press = new InputEventJoypadButton { Device = 0, ButtonIndex = button, Pressed = true };
            using var release = new InputEventJoypadButton { Device = 0, ButtonIndex = button, Pressed = false };
            Input.ParseInputEvent(press); await Frame();
            Input.ParseInputEvent(release); await Frame();
        }
    }
}
