using Godot;

namespace OpenNV.Runtime.InputSystem;

internal static class GamepadInputMap
{
    internal const string AimAction = "opennv_pad_aim";
    private const string LookLeft = "opennv_pad_look_left", LookRight = "opennv_pad_look_right";
    private const string LookUp = "opennv_pad_look_up", LookDown = "opennv_pad_look_down";
    private const float LookRadiansPerSecond = 2.5f;

    internal static void Configure(DesktopInputConfiguration input)
    {
        Button("ui_accept", JoyButton.A);
        Button("ui_cancel", JoyButton.B);
        Button("ui_up", JoyButton.DpadUp);
        Button("ui_down", JoyButton.DpadDown);
        Button("ui_left", JoyButton.DpadLeft);
        Button("ui_right", JoyButton.DpadRight);
        Axis("ui_left", JoyAxis.LeftX, -1);
        Axis("ui_right", JoyAxis.LeftX, 1);
        Axis("ui_up", JoyAxis.LeftY, -1);
        Axis("ui_down", JoyAxis.LeftY, 1);
        Axis(input.MoveLeft.Action, JoyAxis.LeftX, -1);
        Axis(input.MoveRight.Action, JoyAxis.LeftX, 1);
        Axis(input.MoveForward.Action, JoyAxis.LeftY, -1);
        Axis(input.MoveBackward.Action, JoyAxis.LeftY, 1);
        Axis(LookLeft, JoyAxis.RightX, -1);
        Axis(LookRight, JoyAxis.RightX, 1);
        Axis(LookUp, JoyAxis.RightY, -1);
        Axis(LookDown, JoyAxis.RightY, 1);
        Axis(input.Fire.Action, JoyAxis.TriggerRight, 1);
        Axis(AimAction, JoyAxis.TriggerLeft, 1);
        Button(input.Activate.Action, JoyButton.A);
        Button(input.Jump.Action, JoyButton.Y);
        Button(input.Reload.Action, JoyButton.X);
        Button(input.PipBoy.Action, JoyButton.B);
        Button(input.Cancel.Action, JoyButton.Start);
        Button(input.Sprint.Action, JoyButton.LeftStick);
        Button(input.Save.Action, JoyButton.Back);
    }

    private static void Axis(string action, JoyAxis axis, float value) =>
        Add(action, new InputEventJoypadMotion { Device = -1, Axis = axis, AxisValue = value });
    private static void Button(string action, JoyButton button) =>
        Add(action, new InputEventJoypadButton { Device = -1, ButtonIndex = button });
    private static void Add(string action, InputEvent @event)
    {
        if (!InputMap.HasAction(action)) InputMap.AddAction(action, .22f);
        if (!InputMap.ActionHasEvent(action, @event)) InputMap.ActionAddEvent(action, @event);
    }

    internal static Vector2 ReadLook(double seconds, bool enabled) => LookDelta(
        enabled && Input.GetConnectedJoypads().Count != 0
            ? Input.GetVector(LookLeft, LookRight, LookUp, LookDown, .22f) : Vector2.Zero,
        seconds, enabled);

    internal static Vector2 LookDelta(Vector2 stick, double seconds, bool enabled) =>
        enabled ? stick * (LookRadiansPerSecond * (float)seconds) : Vector2.Zero;
}
