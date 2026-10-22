using Godot;

namespace OpenNV.Runtime.Diagnostics.Performance;

// Diagnostic UI stays at the canvas resolution and never takes player input.
internal sealed partial class RuntimeFpsOverlay : CanvasLayer
{
    private Label _label = null!;
    private double _elapsed;
    internal Control Panel { get; private set; } = null!;
    internal string DisplayText => _label.Text;

    public override void _Ready()
    {
        Layer = 200;
        ProcessMode = ProcessModeEnum.Always;
        var canvas = new Control { MouseFilter = Control.MouseFilterEnum.Ignore };
        AddChild(canvas);
        canvas.SetAnchorsAndOffsetsPreset(Control.LayoutPreset.FullRect);
        var panel = new PanelContainer { MouseFilter = Control.MouseFilterEnum.Ignore };
        canvas.AddChild(panel);
        panel.SetAnchorsAndOffsetsPreset(Control.LayoutPreset.TopRight);
        panel.OffsetLeft = -152;
        panel.OffsetRight = -12;
        panel.OffsetTop = 12;
        panel.OffsetBottom = 50;
        using var background = new StyleBoxFlat { BgColor = new Color(0, 0, 0, .7f) };
        panel.AddThemeStyleboxOverride("panel", background);
        _label = new Label
        {
            Text = "— FPS",
            HorizontalAlignment = HorizontalAlignment.Center,
            VerticalAlignment = VerticalAlignment.Center,
            MouseFilter = Control.MouseFilterEnum.Ignore
        };
        _label.AddThemeFontSizeOverride("font_size", 22);
        panel.AddChild(_label);
        Panel = panel;
    }

    public override void _Process(double delta)
    {
        _elapsed += delta;
        if (_elapsed < .5) return;
        _elapsed = 0;
        _label.Text = $"{Engine.GetFramesPerSecond()} FPS";
    }
}
