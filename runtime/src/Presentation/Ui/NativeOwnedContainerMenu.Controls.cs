using Godot;

namespace OpenNV.Runtime.Presentation.Ui;

internal partial class NativeOwnedContainerMenu
{
    private readonly int[] _selection = [0, 0];
    private NativeMenuStick _stick;
    private Label _controllerHints = null!;

    private void AddControllerHints()
    {
        _controllerHints = new Label
        {
            Text = "Gamepad: D-pad / stick select  |  A transfer  |  X take all  |  B close  |  LB/RB page",
            HorizontalAlignment = HorizontalAlignment.Center,
            MouseFilter = MouseFilterEnum.Ignore,
            Visible = OS.HasFeature("android") || Input.GetConnectedJoypads().Count != 0
        };
        _controllerHints.AddThemeFontSizeOverride("font_size", 24);
        _controllerHints.AddThemeColorOverride("font_color", _tiles.Color);
        AddChild(_controllerHints);
    }

    private void LayoutControllerHints()
    {
        if (_controllerHints is null) return;
        _controllerHints.Position = new(20, 900);
        _controllerHints.Size = new(Math.Max(0, Size.X - 40), 40);
    }

    private void FocusSelection()
    {
        var count = TransferableItems(_side).Length;
        _selection[_side] = Math.Clamp(_selection[_side], 0, Math.Max(0, count - 1));
        var name = $"Item_{_side}_{_selection[_side] - _offsets[_side]}";
        var button = _rows.FirstOrDefault(row => (string?)row.Tile.Attribute("name") == name).Button;
        (button ?? _targets[1].Button).GrabFocus();
        QueueRedraw();
    }

    private void NavigateRows(int delta)
    {
        var count = TransferableItems(_side).Length;
        _selection[_side] = Math.Clamp(_selection[_side] + delta, 0, Math.Max(0, count - 1));
        _offsets[_side] = Math.Clamp(_offsets[_side], Math.Max(0, _selection[_side] - RowsPerPage + 1), _selection[_side]);
        Refresh();
    }

    private bool ControllerInput(InputEvent input)
    {
        if (input is InputEventJoypadButton { Pressed: true } button)
        {
            switch (button.ButtonIndex)
            {
                case JoyButton.B: Close(); return true;
                case JoyButton.X: TakeAll(); return true;
                case JoyButton.LeftShoulder: NavigateRows(-RowsPerPage); return true;
                case JoyButton.RightShoulder: NavigateRows(RowsPerPage); return true;
            }
        }
        var direction = _stick.Read(input);
        var horizontal = direction.X != 0 ? direction.X : input.IsActionPressed("ui_left") ? -1 : input.IsActionPressed("ui_right") ? 1 : 0;
        var vertical = direction.Y != 0 ? direction.Y : input.IsActionPressed("ui_up") ? -1 : input.IsActionPressed("ui_down") ? 1 : 0;
        if (input is InputEventJoypadMotion)
        {
            horizontal = direction.X; vertical = direction.Y;
        }
        if (horizontal != 0) { _side = horizontal < 0 ? 0 : 1; FocusSelection(); return true; }
        if (vertical != 0) { NavigateRows(vertical); return true; }
        if (input.IsActionPressed("ui_accept"))
        {
            var focused = _targets.Concat(_rows).FirstOrDefault(target => target.Button.HasFocus()).Button;
            focused?.EmitSignal(BaseButton.SignalName.Pressed);
            return true;
        }
        return false;
    }
}

// One menu step per stick deflection, with hysteresis to reject held-axis noise.
internal struct NativeMenuStick
{
    private int _x, _y;
    internal Vector2I Read(InputEvent input)
    {
        if (input is not InputEventJoypadMotion motion || motion.Axis is not (JoyAxis.LeftX or JoyAxis.LeftY)) return default;
        ref var held = ref (motion.Axis == JoyAxis.LeftX ? ref _x : ref _y);
        var next = Math.Abs(motion.AxisValue) < .25f ? 0 : Math.Abs(motion.AxisValue) >= .5f ? Math.Sign(motion.AxisValue) : held;
        if (held == next) return default;
        held = next;
        return motion.Axis == JoyAxis.LeftX ? new(next, 0) : new(0, next);
    }
}
