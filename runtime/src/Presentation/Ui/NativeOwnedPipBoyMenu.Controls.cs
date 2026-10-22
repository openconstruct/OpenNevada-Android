using Godot;
using OpenNV.Runtime.Gameplay.State;

namespace OpenNV.Runtime.Presentation.Ui;

internal partial class NativeOwnedPipBoyMenu
{
    private NativeMenuStick _stick;
    private string? _controllerTarget;

    private NativeBitmapMenuButton[] ControllerTargets() => _buttons
        .Where(target => target.Button.Visible && !target.Button.Disabled)
        .Select(target => target.Button).ToArray();

    private void FocusControllerSelection()
    {
        var targets = ControllerTargets();
        var selected = targets.FirstOrDefault(button => button.Name.ToString() == _controllerTarget)
            ?? targets.FirstOrDefault(button => button.Name.ToString().Contains("PipBoyRow", StringComparison.Ordinal))
            ?? targets.FirstOrDefault();
        if (selected is null) return;
        _controllerTarget = selected.Name; selected.GrabFocus(); QueueRedraw();
    }

    private bool ControllerInput(InputEvent input)
    {
        if (input is InputEventJoypadButton { Pressed: true } shoulder &&
            shoulder.ButtonIndex is JoyButton.LeftShoulder or JoyButton.RightShoulder)
        {
            var delta = shoulder.ButtonIndex == JoyButton.LeftShoulder ? -1 : 1;
            var page = (FalloutPipBoyPage)(((int)_state.Page + delta + 3) % 3);
            _controllerTarget = null;
            Select(page, page == FalloutPipBoyPage.Data ? 2 : 0);
            return true;
        }
        if (input is not (InputEventJoypadButton or InputEventJoypadMotion)) return false;
        var direction = _stick.Read(input);
        if (input is InputEventJoypadButton { Pressed: true } button)
        {
            direction = button.ButtonIndex switch
            {
                JoyButton.DpadLeft => new(-1, 0),
                JoyButton.DpadRight => new(1, 0),
                JoyButton.DpadUp => new(0, -1),
                JoyButton.DpadDown => new(0, 1),
                _ => direction
            };
            if (button.ButtonIndex == JoyButton.A)
            {
                var focused = ControllerTargets().FirstOrDefault(target => target.HasFocus());
                focused?.EmitSignal(BaseButton.SignalName.Pressed);
                return true;
            }
        }
        if (direction.X != 0)
        {
            _controllerTarget = null;
            Select(_state.Page, (_state.Selection + direction.X + 5) % 5);
        }
        if (direction.Y != 0)
        {
            var targets = ControllerTargets();
            var rows = targets.Where(button => button.Name.ToString().Contains("PipBoyRow", StringComparison.Ordinal)).ToArray();
            if (rows.Length != 0) targets = rows;
            var index = Array.FindIndex(targets, button => button.HasFocus());
            var next = index < 0 ? 0 : index + direction.Y;
            if (rows.Length != 0 && (next < 0 || next >= targets.Length))
            {
                _offset = Math.Max(0, _offset + direction.Y);
                _controllerTarget = targets[Math.Clamp(next, 0, targets.Length - 1)].Name;
                Refresh();
            }
            else if (targets.Length != 0)
            {
                _controllerTarget = targets[(next + targets.Length) % targets.Length].Name;
                FocusControllerSelection();
            }
        }
        // Consume releases and held-axis noise too: the viewport's default GUI
        // navigation must not apply a second step or activate a different target.
        return true;
    }
}
