using Godot;
using OpenNV.Runtime;
using OpenNV.Runtime.Content;
using OpenNV.Runtime.InputSystem;
using OpenNV.Runtime.Presentation.Ui;

public partial class NativeNifInstanceAudit
{
    private void ExerciseCreationMaterials(string root)
    {
        RuntimeLiveContentSource.Configure(root, RuntimeLiveContentSource.FalloutNewVegasGame);
        using var source = RuntimeLiveContentSource.Current!;
        using var records = FalloutPluginStack.Load(source.PluginSources);
        var contract = FalloutNativeRaceSexResolver.Resolve(records);
        var settings = FalloutInstallationSettings.Read(source);
        var checkedParts = 0;
        foreach (var race in contract.Races)
            foreach (var female in new[] { false, true })
            {
                var creation = new FalloutNativeCharacterCreation(records, contract, contract.ForSex(female), settings);
                creation.ChangeIdentity(race.RuntimeFormId, female);
                void Audit()
                {
                    var appearance = creation.Appearance();
                    foreach (var part in appearance.Models.Concat(appearance.RaceParts).Where(part => part.ModelPath is not null && part.AlternateTextures.Count != 0))
                    {
                        if (!source.TryRead(part.ModelPath!, null, out var bytes, out _)) throw new FileNotFoundException(part.ModelPath);
                        var nif = OpenNV.Runtime.Formats.Gamebryo.FalloutNifFile.Read(bytes);
                        try
                        {
                            foreach (var block in nif.Blocks.Where(block => block.TypeName is "NiTriShape" or "NiTriStrips" or "BSSegmentedTriShape"))
                                _ = OpenNV.Runtime.Presentation.Rendering.NativeNpcMaterial.Alternate(part, nif, nif.ReadGeometry(block.Index));
                        }
                        catch (Exception error)
                        {
                            GD.Print($"OPENNV_CREATION_MATERIAL_SOURCE race={race.EditorId} female={female} role={part.Role} model={part.ModelPath} entries={System.Text.Json.JsonSerializer.Serialize(part.AlternateTextures.Select(entry => new { entry.ShapeName, entry.ShapeIndex, entry.TextureSet }))}");
                            throw new InvalidDataException("Owned creation material selection failed.", error);
                        }
                        checkedParts++;
                    }
                }
                Audit();
                foreach (var hair in race.HairFor(female)) { creation.SetHair(hair); Audit(); }
                foreach (var eye in race.EyesFor(female)) { creation.SetEyes(eye); Audit(); }
                foreach (var headPart in creation.FacialHair())
                {
                    creation.ToggleHeadPart(headPart); Audit(); creation.ToggleHeadPart(headPart);
                }
            }
        if (checkedParts == 0) throw new InvalidDataException("Owned creation audit did not exercise alternate textures.");
        GD.Print($"OPENNV_CREATION_MATERIALS_PASS parts={checkedParts} scope=alternate-texture-targets renderedPreview=unverified");
    }

    private async Task ExerciseMenuInput(string root)
    {
        var input = RuntimeConfiguration.Load().Player.DesktopInput;
        DesktopInputMap.Configure(input);
        GamepadInputMap.Configure(input);
        using var accept = new InputEventJoypadButton { Device = 0, ButtonIndex = JoyButton.A, Pressed = true };
        if (!InputMap.EventIsAction(accept, "ui_accept"))
            throw new InvalidDataException("Controller A is not bound to ui_accept.");
        var before = InputMap.ActionGetEvents(input.Activate.Action).Count;
        GamepadInputMap.Configure(input);
        if (InputMap.ActionGetEvents(input.Activate.Action).Count != before ||
            !InputMap.ActionGetEvents(input.Activate.Action).Any(e => e is InputEventKey) ||
            !InputMap.ActionGetEvents(input.Activate.Action).Any(e => e is InputEventJoypadButton))
            throw new InvalidDataException("Controller configuration duplicates or replaces desktop bindings.");
        var stick = new Vector2(.4f, -.6f);
        var slow = Enumerable.Range(0, 30).Aggregate(Vector2.Zero, (sum, _) => sum + GamepadInputMap.LookDelta(stick, 1.0 / 30, true));
        var fast = Enumerable.Range(0, 120).Aggregate(Vector2.Zero, (sum, _) => sum + GamepadInputMap.LookDelta(stick, 1.0 / 120, true));
        if (slow.DistanceTo(fast) > .00001f || GamepadInputMap.LookDelta(stick, 1, false) != Vector2.Zero)
            throw new InvalidDataException("Stick look depends on frame count or ignores its input gate.");
        var auditRoot = ProjectSettings.GlobalizePath("user://input-audit-" + Guid.NewGuid().ToString("N"));
        var profiles = GodotLauncherProfileStore.Open(Path.Combine(auditRoot, "profiles.json"), Path.Combine(auditRoot, "saves"));
        var launcher = new NativeGodotLauncher();
        profiles.Save("newvegas", root);
        if (profiles.AutoLaunchCampaignId != "newvegas") throw new InvalidDataException("Single-profile auto launch was lost.");
        profiles.Save("fallout3", Path.Combine(auditRoot, "missing-fo3"));
        if (profiles.AutoLaunchCampaignId is not null) throw new InvalidDataException("Ambiguous profiles auto-launched without a selection.");
        launcher.Configure(profiles);
        var launched = false;
        launcher.LaunchRequested += _ => launched = true;
        AddChild(launcher);
        try
        {
            if (GetViewport().GuiGetFocusOwner() is not Button focused || focused.Disabled ||
                !focused.Text.StartsWith("Play", StringComparison.Ordinal))
                throw new InvalidDataException("Registered installation did not focus Play in the launcher.");
            await Press(JoyButton.A);
            if (!launched) throw new InvalidDataException("Controller A did not launch the registered installation.");
        }
        finally
        {
            launcher.Free();
        }
        launcher = new NativeGodotLauncher();
        launcher.Configure(GodotLauncherProfileStore.Open(Path.Combine(auditRoot, "profiles.json"), Path.Combine(auditRoot, "saves")));
        launched = false;
        launcher.LaunchRequested += _ => launched = true;
        AddChild(launcher);
        try
        {
            if (!launcher.TryAutoLaunch() || !launched)
                throw new InvalidDataException("Remembered profile did not auto-launch after reload.");
        }
        finally { launcher.Free(); }
        profiles.Save("newvegas", Path.Combine(auditRoot, "missing-nv"));
        launcher = new NativeGodotLauncher();
        launcher.Configure(profiles);
        launcher.LaunchRequested += _ => throw new InvalidDataException("Missing installation was auto-launched.");
        AddChild(launcher);
        try
        {
            if (launcher.TryAutoLaunch()) throw new InvalidDataException("Missing installation did not return to the picker.");
        }
        finally { launcher.Free(); Directory.Delete(auditRoot, true); }
        RuntimeLiveContentSource.Configure(root, RuntimeLiveContentSource.FalloutNewVegasGame);
        using var content = RuntimeLiveContentSource.Current!;
        using var records = FalloutPluginStack.Load(content.PluginSources);
        string? accepted = null;
        var menu = new NativeGamebryoStartMenu(action => accepted = action);
        AddChild(menu);
        try
        {
            menu.SetReady(records, false);
            Focus("sNew");
            await Press(JoyButton.DpadDown); Focus("sLoad");
            await Press(JoyButton.DpadUp); Focus("sNew");
            await Press(JoyButton.A); Focus("sNo");
            await Press(JoyButton.B); Focus("sNew");
            if (accepted is not null) throw new InvalidDataException("Cancel accepted New Game.");
            await Press(JoyButton.A); Focus("sNo");
            await Press(JoyButton.DpadUp); Focus("sYes");
            await Press(JoyButton.A);
            if (accepted != "sNew") throw new InvalidDataException("Controller confirmation did not accept New Game.");
            menu.Hide();
            var creationMenu = new NativeOwnedRaceSexMenu(records, error => throw new InvalidDataException("Creation focus failed.", error));
            AddChild(creationMenu);
            try
            {
                var direction = 0;
                creationMenu.Navigate += value => direction = value;
                creationMenu.SetCanvas(new(1280, 720));
                creationMenu.SetPage(1, "Controller audit", [new("Choice", false, () => { })]);
                await Press(JoyButton.DpadDown);
                if (GetViewport().GuiGetFocusOwner()?.Name != "RSM_back_button") throw new InvalidDataException("D-pad did not focus creation Back.");
                await Press(JoyButton.A);
                if (direction != -1) throw new InvalidDataException("Controller did not activate creation Back.");
                await Press(JoyButton.DpadDown);
                if (GetViewport().GuiGetFocusOwner()?.Name != "RSM_next_button") throw new InvalidDataException("D-pad did not focus creation Next.");
                await Press(JoyButton.A);
                if (direction != 1) throw new InvalidDataException("Controller did not activate creation Next.");
                direction = 0;
                await Press(JoyButton.B);
                if (direction != -1) throw new InvalidDataException("Controller B did not navigate creation Back.");
                GD.Print("OPENNV_CREATION_FOCUS_PASS back=true next=true accept=true cancel=true");
            }
            finally { creationMenu.Free(); creationMenu = null!; }
            GC.Collect(); GC.WaitForPendingFinalizers();
            await ToSignal(GetTree(), SceneTree.SignalName.ProcessFrame);
            GD.Print("OPENNV_MENU_INPUT_PASS navigation=true accept=true cancel=true confirmation=true desktopRetained=true idempotent=true lookFrameRateIndependent=true autoLaunch=true rememberedProfile=true missingAndAmbiguousFallback=true");
        }
        finally
        {
            menu.GetNode<AudioStreamPlayer>("OwnedMainTitleMusic").Stop();
            // Let the audio thread release its current playback before quitting.
            await ToSignal(GetTree().CreateTimer(.05), SceneTreeTimer.SignalName.Timeout);
            menu.Free();
        }
        return;

        void Focus(string name)
        {
            if (GetViewport().GuiGetFocusOwner()?.Name != name)
                throw new InvalidDataException($"Expected controller focus on {name}; got {GetViewport().GuiGetFocusOwner()?.Name}; confirmations={menu.FindChildren("confirm_container", "", true, false).Count}; accepted={accepted}.");
        }
        async Task Press(JoyButton button)
        {
            using var press = new InputEventJoypadButton { Device = 0, ButtonIndex = button, Pressed = true };
            using var release = new InputEventJoypadButton { Device = 0, ButtonIndex = button, Pressed = false };
            Input.ParseInputEvent(press);
            await ToSignal(GetTree(), SceneTree.SignalName.ProcessFrame);
            Input.ParseInputEvent(release);
            await ToSignal(GetTree(), SceneTree.SignalName.ProcessFrame);
        }
    }
}
