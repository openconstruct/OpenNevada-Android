using Godot;
using OpenNV.Runtime.Content;
using OpenNV.Runtime.Gameplay.State;
using OpenNV.Runtime.Presentation.Ui;

public partial class NativeNifInstanceAudit
{
    private async Task ExerciseHudPlacement(string root, string framePath)
    {
        RuntimeLiveContentSource.Configure(root, RuntimeLiveContentSource.FalloutNewVegasGame);
        using var source = RuntimeLiveContentSource.Current!;
        using var records = FalloutPluginStack.Load(source.PluginSources);
        var quest = FalloutDialogueTopic.Find(records, "QUST", "VCG01");
        var objective = quest.ReadSubrecords().First(field => field.Signature == "QOBJ");
        var index = System.Buffers.Binary.BinaryPrimitives.ReadUInt32LittleEndian(objective.Data.Span);
        var queue = new FalloutHudNotifications();
        queue.Publish([new(FalloutHudEventKind.ObjectiveDisplayed, quest.FormKey, 0, ObjectiveIndex: index)]);
        var canvas = new SubViewport { Size = new(1280, 720), Disable3D = true, RenderTargetUpdateMode = SubViewport.UpdateMode.Always };
        AddChild(canvas);
        var notice = new NativeOwnedHudMessages(records, queue);
        canvas.AddChild(notice);
        try
        {
            for (var frame = 0; frame < 4; frame++) await ToSignal(RenderingServer.Singleton, RenderingServer.SignalName.FramePostDraw);
            Require(notice.Error is null, "Owned quest notice failed to resolve.");
            Require(!notice.QuestTextOverlaps, "Wrapped quest title overlapped its objective text.");
            var bounds = notice.QuestNoticeBounds;
            Require(bounds.Position.X >= 0 && bounds.Position.Y >= 0 && bounds.End.X < 640 && bounds.End.Y < 360,
                "Compact quest notice did not remain within the upper-left quadrant.");
            notice.SetProcess(false); notice._Process(1);
            await ToSignal(RenderingServer.Singleton, RenderingServer.SignalName.FramePostDraw);
            using var image = canvas.GetTexture().GetImage();
            if (image.SavePng(framePath) != Error.Ok) throw new IOException("Could not capture temporary HUD placement preview.");
            GD.Print($"OPENNV_HUD_PLACEMENT_PASS upperLeft=true bounds={bounds} ownedText=true sourceSafeInset=true retailAlignment=unverified");
        }
        finally { canvas.Free(); }
    }
}
