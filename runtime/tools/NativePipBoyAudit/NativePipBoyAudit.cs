using System.Text.Json;
using Godot;
using OpenNV.Runtime.Content;
using OpenNV.Runtime.Gameplay.State;
using OpenNV.Runtime.Presentation.Ui;
using OpenNV.Runtime.World.Cells;

// Read a real checkpoint into disposable owners. Never writes that checkpoint.
public partial class NativePipBoyAudit : Node
{
    public override async void _Ready()
    {
        try
        {
            var cropped = NativeUiImage.Sample(new(0, 0, 123.25f, 133.4f), new(0, 0, 128, 128), 145, Vector2.Zero)!.Value;
            if (!cropped.Source.Size.IsEqualApprox(new(85, 92))) throw new InvalidOperationException("Zoomed status art was stretched instead of cropped.");
            var shifted = NativeUiImage.Sample(new(10, 20, 60, 40), new(100, 200, 128, 128), 200, new(20, 10))!.Value;
            if (!shifted.Source.Position.IsEqualApprox(new(110, 205)) || !shifted.Source.Size.IsEqualApprox(new(30, 20)))
                throw new InvalidOperationException("Atlas crop/zoom lost source coordinates.");
            var stretched = NativeUiImage.Sample(new(0, 0, 400, 200), new(0, 0, 128, 64), -1, Vector2.Zero)!.Value;
            if (stretched.Source.Size != new Vector2(128, 64) || NativeUiImage.Sample(new(0, 0, 1, 1), new(0, 0, 1, 1), 0, Vector2.Zero) is not null)
                throw new InvalidOperationException("Explicit UI stretch/hide semantics failed.");
            var args = OS.GetCmdlineUserArgs();
            if (args.Length != 2) throw new ArgumentException("Expected owned Data and a native campaign checkpoint.");
            RuntimeLiveContentSource.Configure(args[0], RuntimeLiveContentSource.FalloutNewVegasGame);
            using var content = RuntimeLiveContentSource.Current!;
            var settings = FalloutInstallationSettings.Read(content);
            var color = settings.InterfaceColor("Pipboy", "uPipboyColor");
            var tree = new NativeOwnedMenuTree(System.Xml.Linq.XElement.Parse("<menu><systemcolor>entity_pipboy</systemcolor></menu>"));
            var tint = tree.TileColor(tree.Root);
            if (tint.R != (color >> 24) / 255f || tint.G != ((color >> 16) & 255) / 255f || tint.B != ((color >> 8) & 255) / 255f)
                throw new InvalidDataException("Pip-Boy tiles bypassed the selected interface color.");
            GD.Print($"OPENNV_PIPBOY_COLOR_PASS custom={settings.Unsigned("Interface", "uPipboyColor")} resolved={color:x8}");
            var childRoot = System.Xml.Linq.XElement.Parse("<menu name='Probe'><image name='divider'><alpha><copy src='child()' trait='alpha'/></alpha><rect name='meter'><alpha>123</alpha></rect></image></menu>");
            var childTree = new NativeOwnedMenuTree(childRoot);
            if (childTree.Number(childRoot.Element("image")!, "alpha") != 123)
                throw new InvalidDataException("Unnamed child reference did not resolve its unique named tile.");
            using var records = FalloutPluginStack.Load(content.PluginSources);
            var saved = JsonSerializer.Deserialize<FalloutNativeCampaignState>(File.ReadAllText(args[1]))!;
            using var references = new FalloutReferenceWorld(records);
            references.Restore(saved.References!);
            var inventory = new FalloutPlayerInventory();
            var items = FalloutCampaignInventoryResolver.Resolve(records, saved.Inventory.Select(item =>
                new FalloutCampaignInventoryRequest(item.RuntimeFormId, item.EditorId, item.RecordType, item.Count)).ToArray(), null);
            inventory.Restore(items, saved.EquippedRuntimeFormIds.ToArray(), saved.InventoryRandomState);
            var quests = new FalloutQuestState(records); quests.Restore(saved.Quests!);
            var state = new FalloutPipBoyState(records, inventory); state.SetOpen(true);
            var menu = new NativeOwnedPipBoyMenu(records, state, inventory, quests, references, () => saved.Vitals!, () => saved.Special,
                saved.PlayerName, FalloutNativeTagSkillResolver.Resolve(records, FalloutOpeningPlayerControlResolver.Resolve(records, ["VCG00", "VCG01"])).Skills, saved.TagSkills, saved.Traits,
                FalloutCellSceneReader.ParentWorldspace(records.GetEffective(saved.ActiveCell)),
                new(saved.PlayerPosition[0] / .0142875f, -saved.PlayerPosition[2] / .0142875f, saved.PlayerPosition[1] / .0142875f), 0);
            var viewport = new SubViewport { Size = new(1280, 960), Disable3D = true, ProcessMode = ProcessModeEnum.Always };
            AddChild(viewport); viewport.AddChild(menu);
            var failures = new List<string>();
            var selections = new[] { (FalloutPipBoyPage.Stats, 0), (FalloutPipBoyPage.Stats, 1),
                (FalloutPipBoyPage.Stats, 2), (FalloutPipBoyPage.Stats, 3),
                (FalloutPipBoyPage.Items, 0), (FalloutPipBoyPage.Items, 1), (FalloutPipBoyPage.Items, 2),
                (FalloutPipBoyPage.Items, 3), (FalloutPipBoyPage.Items, 4), (FalloutPipBoyPage.Data, 1), (FalloutPipBoyPage.Data, 2) };
            foreach (var (page, index) in selections)
            {
                menu.Select(page, index);
                await ToSignal(GetTree(), SceneTree.SignalName.ProcessFrame);
                if (menu.Error is { } error) { failures.Add($"{page}/{index}: {error}"); continue; }
                var buttons = menu.GetChildren().OfType<BaseButton>().Where(button => button.Visible).ToArray();
                if (buttons.Any(button => button.Size.X <= 0 || button.Size.Y <= 0)) failures.Add($"{page}/{index}: empty button target");
                GD.Print($"OPENNV_PIPBOY_PAGE_BOUND {page}/{index} buttons={buttons.Length}");
            }
            menu.Select(FalloutPipBoyPage.Stats, 0);
            var wasPaused = GetTree().Paused; GetTree().Paused = true;
            try
            {
                await Press(JoyButton.DpadRight);
                Require(state.Page == FalloutPipBoyPage.Stats && state.Selection == 1, "Paused subviewport lost tab navigation.");
                var first = viewport.GuiGetFocusOwner();
                Require(first?.Name == "PipBoyTarget_PipBoyRow0", "SPECIAL has no initial row focus.");
                await Press(JoyButton.DpadDown);
                Require(viewport.GuiGetFocusOwner()?.Name == "PipBoyTarget_PipBoyRow1", "D-pad selected the wrong row.");
                await Press(JoyButton.A);
                Require(viewport.GuiGetFocusOwner()?.Name == "PipBoyTarget_PipBoyRow1", "Selection refresh lost focused row.");
                Require(JsonSerializer.SerializeToElement(menu.State).GetProperty("attribute").GetInt32() == 1,
                    "A did not activate the focused source attribute.");
                await Stick(1); var moved = viewport.GuiGetFocusOwner()?.Name;
                await Stick(.9f);
                Require(viewport.GuiGetFocusOwner()?.Name == moved, "Held stick applied repeated navigation.");
                await Stick(0);
                await Press(JoyButton.RightShoulder);
                Require(state.Page == FalloutPipBoyPage.Items && state.Selection == 0, "Shoulder did not switch page.");
                await Press(JoyButton.RightShoulder);
                Require(state.Page == FalloutPipBoyPage.Data && state.Selection == 2 && menu.Error is null,
                    "Data initial page must remain usable inside interiors.");
                await Press(JoyButton.RightShoulder);
                Require(state.Page == FalloutPipBoyPage.Stats, "Page navigation failed to wrap.");
            }
            finally { GetTree().Paused = wasPaused; }
            GD.Print("OPENNV_PIPBOY_CONTROLLER_PASS paused=true subviewport=true tabs=true rows=true activation=true focusRestore=true stickLatch=true pages=true");
            if (failures.Count != 0) throw new InvalidOperationException(string.Join("\n", failures));
            GD.Print("OPENNV_NATIVE_PIPBOY_AUDIT_PASS source-menus=true saved-items-quests-markers=true recording=off visual-and-input-acceptance=separate");
            viewport.Free(); GetTree().Quit();
            async Task Press(JoyButton button)
            {
                using var press = new InputEventJoypadButton { Device = 0, ButtonIndex = button, Pressed = true };
                viewport.PushInput(press, true); await ToSignal(GetTree(), SceneTree.SignalName.ProcessFrame);
                using var release = new InputEventJoypadButton { Device = 0, ButtonIndex = button, Pressed = false };
                viewport.PushInput(release, true); await ToSignal(GetTree(), SceneTree.SignalName.ProcessFrame);
            }
            async Task Stick(float value)
            {
                using var motion = new InputEventJoypadMotion { Device = 0, Axis = JoyAxis.LeftY, AxisValue = value };
                viewport.PushInput(motion, true); await ToSignal(GetTree(), SceneTree.SignalName.ProcessFrame);
            }
            static void Require(bool condition, string message) { if (!condition) throw new InvalidOperationException(message); }
        }
        catch (Exception error) { GD.PushError(error.ToString()); GetTree().Quit(1); }
    }
}
