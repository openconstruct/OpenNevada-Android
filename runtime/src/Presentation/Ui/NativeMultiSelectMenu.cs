using Godot;

namespace OpenNV.Runtime.Presentation.Ui;

// Shared presentation for bounded multi-selection. The caller owns the allowed
// choices, limits and acceptance validation; this control only adapts input.
internal sealed partial class NativeMultiSelectMenu : Control
{
    private readonly List<Button> _choices = [];
    private readonly Button _confirm;
    private readonly Label _status;
    private readonly ScrollContainer _scroll;
    internal event Action<int>? ChoicePressed;
    internal event Action? Confirmed;

    internal NativeMultiSelectMenu(string title, IReadOnlyList<string> choices, string confirmText)
    {
        Name = "Choices";
        SetAnchorsAndOffsetsPreset(LayoutPreset.FullRect);
        var shade = new ColorRect { Color = new(0, 0, 0, .82f) };
        shade.SetAnchorsAndOffsetsPreset(LayoutPreset.FullRect); AddChild(shade);
        var margin = new MarginContainer();
        margin.SetAnchorsAndOffsetsPreset(LayoutPreset.FullRect);
        foreach (var side in new[] { "left", "right", "top", "bottom" })
            margin.AddThemeConstantOverride("margin_" + side, 24);
        AddChild(margin);
        var center = new CenterContainer(); margin.AddChild(center);
        var panel = new VBoxContainer { CustomMinimumSize = new(580, 0), SizeFlagsVertical = SizeFlags.ExpandFill };
        center.AddChild(panel);
        panel.AddChild(new Label { Text = title, HorizontalAlignment = HorizontalAlignment.Center });
        _scroll = new ScrollContainer
        {
            FollowFocus = true,
            HorizontalScrollMode = ScrollContainer.ScrollMode.Disabled,
            SizeFlagsVertical = SizeFlags.ExpandFill,
            CustomMinimumSize = new(0, 200)
        };
        panel.AddChild(_scroll);
        var list = new VBoxContainer { SizeFlagsHorizontal = SizeFlags.ExpandFill };
        _scroll.AddChild(list);
        for (var index = 0; index < choices.Count; index++)
        {
            var selected = index;
            var button = new Button { Name = "Choice_" + index, Text = choices[index], ToggleMode = true };
            StyleFocus(button);
            button.Pressed += () => ChoicePressed?.Invoke(selected);
            _choices.Add(button); list.AddChild(button);
        }
        _status = new Label { HorizontalAlignment = HorizontalAlignment.Center }; panel.AddChild(_status);
        _confirm = new Button { Name = "ConfirmChoices", Text = confirmText };
        StyleFocus(_confirm);
        _confirm.Pressed += () => Confirmed?.Invoke(); panel.AddChild(_confirm);
        panel.AddChild(new Label
        {
            Text = "D-pad ↑ / ↓: Move   A: Select / unselect   X: Confirm",
            HorizontalAlignment = HorizontalAlignment.Center
        });
        Resized += () => _scroll.CustomMinimumSize = new(0, Math.Max(120, Size.Y - 220));
    }

    public override void _Ready()
    {
        if (_choices.Count != 0) _choices[0].CallDeferred(Button.MethodName.GrabFocus);
    }

    internal void Refresh(IReadOnlyList<bool> selected, string status, bool canConfirm)
    {
        if (selected.Count != _choices.Count) throw new ArgumentException("Choice state does not match the presented list.", nameof(selected));
        for (var index = 0; index < selected.Count; index++) _choices[index].SetPressedNoSignal(selected[index]);
        _status.Text = status;
        var returnFocus = _confirm.HasFocus() && !canConfirm;
        _confirm.Disabled = !canConfirm;
        if (returnFocus && _choices.Count != 0) _choices[0].GrabFocus();
    }

    public override void _Input(InputEvent input)
    {
        var direction = input.IsActionPressed("ui_up", true) ? -1 : input.IsActionPressed("ui_down", true) ? 1 : 0;
        if (direction != 0)
        {
            var enabled = _choices.Concat(_confirm.Disabled ? [] : new[] { _confirm }).ToArray();
            if (enabled.Length == 0) return;
            var current = Array.FindIndex(enabled, button => button.HasFocus());
            var next = current < 0 ? (direction > 0 ? 0 : enabled.Length - 1) : (current + direction + enabled.Length) % enabled.Length;
            enabled[next].GrabFocus();
            GetViewport().SetInputAsHandled();
        }
        if (input is InputEventJoypadButton { Pressed: true, ButtonIndex: JoyButton.X })
        {
            GetViewport().SetInputAsHandled();
            if (!_confirm.Disabled) Confirmed?.Invoke();
        }
    }

    private static void StyleFocus(Button button)
    {
        using var outline = new StyleBoxFlat
        {
            BgColor = new(0, 0, 0, 0),
            BorderColor = Colors.White,
            BorderWidthLeft = 3,
            BorderWidthRight = 3,
            BorderWidthTop = 3,
            BorderWidthBottom = 3
        };
        button.AddThemeStyleboxOverride("focus", outline);
    }
}
