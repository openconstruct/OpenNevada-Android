using Godot;
using OpenNV.Runtime.Content;
using OpenNV.Runtime.Presentation.Ui;

namespace OpenNV.Runtime.Campaigns.NewVegas.Opening;

internal partial class RuntimeNativeTagSkillEntry : CanvasLayer
{
    private const int LayerIndex = 120;
    private FalloutNativeTagSkillContract _contract = null!;
    private readonly List<FalloutNativeSkillIdentity> _selected = [];
    private NativeMultiSelectMenu _menu = null!;

    internal event Action<IReadOnlyList<FalloutNativeSkillIdentity>>? Accepted;

    internal void Configure(
        FalloutNativeTagSkillContract contract,
        IReadOnlyList<FalloutNativeSkillIdentity> current)
    {
        _contract = contract ?? throw new ArgumentNullException(nameof(contract));
        if (current.Count != 0)
            FalloutNativeTagSkillResolver.Validate(contract, current);
        _selected.AddRange(current);
        Name = "NativeTagSkillEntry";
        Layer = LayerIndex;
        _menu = new NativeMultiSelectMenu("CHOOSE THREE TAG SKILLS",
            contract.Skills.Select(skill => skill.DisplayName).ToArray(), "CONFIRM TAG SKILLS");
        _menu.ChoicePressed += index => Toggle(_contract.Skills[index]);
        _menu.Confirmed += Submit;
        AddChild(_menu);
        Input.MouseMode = Input.MouseModeEnum.Visible;
        Refresh();
    }

    private void Toggle(FalloutNativeSkillIdentity skill)
    {
        if (_selected.Contains(skill))
            _selected.Remove(skill);
        else if (_selected.Count < _contract.RequiredCount)
            _selected.Add(skill);
        Refresh();
    }

    private void Refresh()
    {
        _menu.Refresh(_contract.Skills.Select(skill => _selected.Contains(skill)).ToArray(),
            $"Selected: {_selected.Count} / {_contract.RequiredCount}", _selected.Count == _contract.RequiredCount);
    }

    private void Submit()
    {
        var selection = _selected.OrderBy(value => value.RuntimeFormId).ToArray();
        FalloutNativeTagSkillResolver.Validate(_contract, selection);
        Accepted?.Invoke(selection);
    }
}
