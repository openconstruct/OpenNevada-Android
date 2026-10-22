using Godot;
using OpenNV.Runtime.Content;
using OpenNV.Runtime.Presentation.Ui;

namespace OpenNV.Runtime.Campaigns.NewVegas.Opening;

internal partial class RuntimeNativeTraitEntry : CanvasLayer
{
    private const int LayerIndex = 120;
    private FalloutNativeTraitFarewellContract _contract = null!;
    private readonly List<FalloutNativeTraitIdentity> _selected = [];
    private NativeMultiSelectMenu _menu = null!;

    internal event Action<IReadOnlyList<FalloutNativeTraitIdentity>>? Accepted;

    internal void Configure(
        FalloutNativeTraitFarewellContract contract,
        IReadOnlyList<FalloutNativeTraitIdentity> current)
    {
        _contract = contract ?? throw new ArgumentNullException(nameof(contract));
        FalloutNativeTraitFarewellResolver.ValidateTraits(contract, current);
        _selected.AddRange(current);
        Name = "NativeTraitEntry";
        Layer = LayerIndex;
        _menu = new NativeMultiSelectMenu("CHOOSE UP TO TWO TRAITS",
            contract.Traits.Select(trait => trait.DisplayName).ToArray(), "CONFIRM TRAITS");
        _menu.ChoicePressed += index => Toggle(_contract.Traits[index]);
        _menu.Confirmed += Submit;
        AddChild(_menu);
        Input.MouseMode = Input.MouseModeEnum.Visible;
        Refresh();
    }

    private void Toggle(FalloutNativeTraitIdentity trait)
    {
        if (_selected.Contains(trait))
            _selected.Remove(trait);
        else if (_selected.Count < _contract.MaximumTraits)
            _selected.Add(trait);
        Refresh();
    }

    private void Refresh()
    {
        _menu.Refresh(_contract.Traits.Select(trait => _selected.Contains(trait)).ToArray(),
            $"Selected: {_selected.Count} / {_contract.MaximumTraits} maximum", true);
    }

    private void Submit()
    {
        var selection = _selected.OrderBy(value => value.RuntimeFormId).ToArray();
        FalloutNativeTraitFarewellResolver.ValidateTraits(_contract, selection);
        Accepted?.Invoke(selection);
    }

}
