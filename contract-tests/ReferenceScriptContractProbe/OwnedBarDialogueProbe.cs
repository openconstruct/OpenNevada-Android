using System.Text.Json;
using OpenNV.Runtime.Content;
using OpenNV.Runtime.Gameplay.State;
using OpenNV.Runtime.World.Cells;

internal static class OwnedBarDialogueProbe
{
    internal static void Run(string root, string save)
    {
        RuntimeLiveContentSource.Configure(root, RuntimeLiveContentSource.FalloutNewVegasGame);
        using var content = RuntimeLiveContentSource.Current!;
        using var records = FalloutPluginStack.Load(content.PluginSources);
        var saved = JsonSerializer.Deserialize<FalloutNativeCampaignState>(File.ReadAllText(save))!;
        var quests = new FalloutQuestState(records); quests.Restore(saved.Quests!);
        var globals = FalloutGlobalState.Read(records); globals.Restore(saved.Globals!);
        using var world = new FalloutReferenceWorld(records); world.Restore(saved.References!);
        var reputations = new FalloutReputationState(records); reputations.Restore(saved.Reputations ?? []);
        foreach (var form in new uint[] { 0x104f03, 0x104c80, 0x104e85 })
        {
            var reference = records.RuntimeFormKey(form);
            var actor = FalloutDialogueTopic.RequiredForm(records.GetEffective(reference), "NAME");
            var identity = FalloutDialogueSpeaker.Read(records, actor);
            var conditions = new FalloutDialogueConditions(records, quests, reference, identity, condition =>
            {
                if (FalloutPlatformConditions.Evaluate(condition) is { } platform) return platform;
                if (reputations.Evaluate(condition) is { } standing) return standing;
                if (condition.Function == 84) return world.DeadCount(condition.FormArgument1);
                if (condition.Function is 56 or 58 or 59 or 79 or 420 or 421 or 546) return quests.Evaluate(condition);
                if (condition.Function == 74) return globals.Get(condition.FormArgument1);
                if (condition.Function == 53) return (float)world.Get(condition.FormArgument1).Read(condition.Argument2);
                if (condition.RunOn == 1 && condition.Function == 70 && condition.Argument1 <= 1)
                    return (condition.Argument1 == 1) == saved.Character.Female ? 1 : 0;
                throw new NotSupportedException($"Owned greeting {form:x8} condition {condition.Owner.FormKey}/{condition.Function}/{condition.RunOn} is unbound.");
            }, isDead: actor => records.RuntimeFormId(actor) == 0x14 ? saved.Vitals!.HitPoints <= 0 : world.QueryDead(actor) != 0,
                itemCount: (actor, item) => records.RuntimeFormId(actor) == 0x14
                    ? saved.Inventory.SingleOrDefault(value => value.RuntimeFormId == records.RuntimeFormId(item))?.Count ?? 0
                    : world.Inventory(actor, 1, globals).Contents.Item(item)?.Count ?? 0);
            // Selection audit only: never executes results or changes the copied save.
            var conversation = new FalloutConversation(records, quests, conditions.Evaluate, (_, _) => { }, randomIndex: _ => 0);
            conversation.Start(actor, FalloutDialogueTopic.Find(records, "DIAL", "GREETING").FormKey);
            if (conversation.Phase != "speaking" || conversation.Response is null)
                throw new InvalidOperationException("Reported actor has no eligible spoken greeting.");
            Console.WriteLine($"OPENNV_BAR_GREETING_SELECTED reference={reference} info={conversation.Info!.Record.FormKey} resultScripts=not-executed audio-and-input=separate");
            var remaining = 32;
            while (conversation.Phase == "speaking" && --remaining > 0) conversation.CompleteResponse();
            if (remaining == 0) throw new InvalidOperationException("Owned greeting follow-ups did not settle.");
            Console.WriteLine($"OPENNV_BAR_CHOICES_SELECTED reference={reference} phase={conversation.Phase} choices={conversation.Choices.Count}");
        }
        var rep = records.EffectiveRecords("REPU").First().FormKey;
        reputations.Change(rep, 1, 2, "addreputation");
        var cold = new FalloutReputationState(records); cold.Restore(reputations.Capture());
        if (cold.Raw(rep, 1) != reputations.Raw(rep, 1)) throw new InvalidOperationException("Cold reputation lost a source counter.");
        Console.WriteLine("OPENNV_OWNED_BAR_DIALOGUE_PASS reportedSpeakers=3 reputationCold=true");
    }
}
