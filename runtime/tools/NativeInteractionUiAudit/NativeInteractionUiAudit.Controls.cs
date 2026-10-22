using Godot;
using OpenNV.Runtime.Content;
using OpenNV.Runtime.Gameplay.State;
using OpenNV.Runtime.Presentation.Ui;
using OpenNV.Runtime.World.Cells;

public partial class NativeInteractionUiAudit
{
    private async Task ExerciseControllerContainers(FalloutPluginStack records, FalloutGlobalState globals, FalloutFormKey item)
    {
        var wasPaused = GetTree().Paused;
        var source = new FalloutPlayerInventory(); var player = new FalloutPlayerInventory();
        source.Add(records, item, 12, 1, silent: true, globals);
        var closed = 0; var changed = 0;
        var menu = new NativeOwnedContainerMenu(records, player, source, "Player", "Quantity", () => closed++, () => changed++);
        AddChild(menu); GetTree().Paused = true;
        try
        {
            Focus("ContainerRow_1_0");
            await Press(JoyButton.A);
            var quantity = menu.GetChildren().OfType<NativeOwnedQuantityMenu>().Single();
            await Press(JoyButton.X);
            Require(source.Item(item)!.Count == 12 && changed == 0, "Take All escaped the quantity modal.");
            await Press(JoyButton.B);
            Require(closed == 0 && source.Item(item)!.Count == 12, "Quantity cancel changed inventory or closed its parent.");
            await Press(JoyButton.A);
            quantity = menu.GetChildren().OfType<NativeOwnedQuantityMenu>().Single();
            await Press(JoyButton.LeftShoulder);
            Require(quantity.Quantity == 2, "Quantity shoulder adjustment failed.");
            await Press(JoyButton.A);
            Require(source.Item(item)!.Count == 10 && player.Item(item)!.Count == 2 && changed == 1,
                "Controller quantity transfer was duplicated or lost counts.");
            Focus("ContainerRow_1_0");
            await Press(JoyButton.DpadLeft); Focus("ContainerRow_0_0");
            await Press(JoyButton.A);
            Require(source.Item(item)!.Count == 11 && player.Item(item)!.Count == 1, "Controller deposit did not conserve counts.");
            await Press(JoyButton.B); await Press(JoyButton.B);
            Require(closed == 1, "Controller exit did not complete exactly once.");
        }
        finally { menu.Free(); GetTree().Paused = wasPaused; }

        source = new(); player = new();
        var forms = records.EffectiveRecords("MISC").Where(record => FalloutInventoryAccess.CanTransfer(record, false))
            .Take(9).Select(record => record.FormKey).ToArray();
        foreach (var form in forms) source.Add(records, form, 1, 1, silent: true, globals);
        menu = new(records, player, source, "Player", "Paging", () => { }, () => { }); AddChild(menu);
        try
        {
            Focus("ContainerRow_1_0");
            await Stick(1); Focus("ContainerRow_1_1");
            await Stick(.9f); Focus("ContainerRow_1_1");
            await Stick(0); await Stick(1); Focus("ContainerRow_1_2"); await Stick(0);
            await Press(JoyButton.RightShoulder); Focus("ContainerRow_1_8");
            await Press(JoyButton.A);
            Require(player.Items.Sum(value => value.Count) == 1 && source.Items.Sum(value => value.Count) == 8,
                "Paged row activation transferred the wrong number of items.");
        }
        finally { menu.Free(); }

        // The reported first-aid box exercises winning CNTO/LVLI admission too.
        var cell = FalloutCellSceneReader.Read(records, records.RuntimeFormKey(0x1055e1));
        using var world = new FalloutReferenceWorld(records); world.LoadCell(cell);
        var reference = cell.References.Single(value => value.FormKey == records.RuntimeFormKey(0x1767aa));
        Require(cell.BaseObjects[reference.Base].Signature == "CONT", "Owned first-aid reference is not a container.");
        source = world.Inventory(reference.FormKey, 1, globals).Contents;
        var admitted = source.Items.ToArray(); player = new(); closed = 0;
        menu = new(records, player, source, "Player", "First aid", () => closed++, () => { }); AddChild(menu);
        GetTree().Paused = true;
        try
        {
            Require(menu.Error is null && GetViewport().GuiGetFocusOwner() is not null, "Owned first-aid menu has no usable initial focus.");
            await Press(JoyButton.X); await Press(JoyButton.X);
            foreach (var value in admitted)
                Require(player.Item(value.FormKey)?.Count == value.Count && source.Item(value.FormKey) is null,
                    "Owned first-aid Take All failed count conservation.");
            await Press(JoyButton.B);
            Require(closed == 1, "Owned first-aid Exit failed while paused.");
        }
        finally { menu.Free(); GetTree().Paused = wasPaused; }
        GD.Print("OPENNV_CONTAINER_CONTROLLER_PASS paused=true focus=true transfer=true deposit=true quantityCancel=true modal=true paging=true stickLatch=true ownedFirstAid=true takeAllOnce=true closeOnce=true");

        void Focus(string name) => Require(GetViewport().GuiGetFocusOwner()?.Name == name, $"Expected container focus {name}, got {GetViewport().GuiGetFocusOwner()?.Name}.");
        async Task Frame() => await ToSignal(GetTree(), SceneTree.SignalName.ProcessFrame);
        async Task Press(JoyButton button)
        {
            using var press = new InputEventJoypadButton { Device = 0, ButtonIndex = button, Pressed = true };
            Input.ParseInputEvent(press); await Frame(); await Frame();
            using var release = new InputEventJoypadButton { Device = 0, ButtonIndex = button, Pressed = false };
            Input.ParseInputEvent(release); await Frame();
        }
        async Task Stick(float value)
        {
            using var motion = new InputEventJoypadMotion { Device = 0, Axis = JoyAxis.LeftY, AxisValue = value };
            Input.ParseInputEvent(motion); await Frame();
        }
        static void Require(bool condition, string message) { if (!condition) throw new InvalidDataException(message); }
    }
}
