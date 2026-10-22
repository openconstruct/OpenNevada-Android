using Godot;
using OpenNV.Runtime.Presentation.Rendering;
using OpenNV.Runtime.Content;
using OpenNV.Runtime.Presentation.Ui;

public partial class NativeNifInstanceAudit
{
    private async Task ExerciseSpecialControls(string root, string framePath)
    {
        RuntimeLiveContentSource.Configure(root, RuntimeLiveContentSource.FalloutNewVegasGame);
        using var source = RuntimeLiveContentSource.Current!;
        using var records = FalloutPluginStack.Load(source.PluginSources);
        var cell = FalloutCellSceneReader.Read(records, FalloutDialogueTopic.Find(records, "CELL", "GSDocMitchellHouse").FormKey);
        var contract = FalloutNativeVigorResolver.Resolve(records, cell);
        var canvas = new SubViewport { Size = new(1280, 720), Disable3D = true, RenderTargetUpdateMode = SubViewport.UpdateMode.Always };
        AddChild(canvas);
        var preview = new TextureRect
        {
            Texture = canvas.GetTexture(),
            Size = GetViewport().GetVisibleRect().Size,
            StretchMode = TextureRect.StretchModeEnum.KeepAspectCentered,
            ExpandMode = TextureRect.ExpandModeEnum.IgnoreSize
        };
        AddChild(preview);
        var menu = new NativeOwnedLoveTesterMenu(contract, contract.Initial, records) { Size = new(1280, 720) };
        canvas.AddChild(menu);
        try
        {
            var guide = menu.GetNode<PanelContainer>("SpecialControllerGuide");
            guide.Show(); // A desktop diagnostic displays the handheld guide.
            var text = guide.GetNode<Label>("SpecialControls");
            Require(text.Text.Contains("D-pad", StringComparison.Ordinal) && text.Text.Contains("LB / RB", StringComparison.Ordinal),
                "SPECIAL initial attribute page omitted its controller guide.");
            for (var frame = 0; frame < 90 && menu.GetMeta("opennv_love_tester_turning").AsBool(); frame++)
                await ToSignal(RenderingServer.Singleton, RenderingServer.SignalName.FramePostDraw);
            using var next = new InputEventJoypadButton { Pressed = true, ButtonIndex = JoyButton.RightShoulder };
            menu._UnhandledInput(next);
            Require(text.Text.Contains("D-pad", StringComparison.Ordinal) && text.Text.Contains("LB / RB", StringComparison.Ordinal),
                "SPECIAL attribute page omitted adjustment or page controls.");
            for (var frame = 0; frame < 90 && menu.GetMeta("opennv_love_tester_turning").AsBool(); frame++)
                await ToSignal(RenderingServer.Singleton, RenderingServer.SignalName.FramePostDraw);
            await ToSignal(RenderingServer.Singleton, RenderingServer.SignalName.FramePostDraw);
            Require(new Rect2(Vector2.Zero, menu.Size).Encloses(guide.GetRect()), "SPECIAL guide escaped the handheld canvas.");
            using var image = canvas.GetTexture().GetImage();
            if (image.SavePng(framePath) != Error.Ok) throw new IOException("Could not capture temporary controller-guide preview.");
            GD.Print("OPENNV_SPECIAL_CONTROLS_PASS initialAttribute=true attribute=true pageControls=true canvas=1280x720 gameplay=separate");
        }
        finally { preview.Free(); canvas.Free(); }
    }

    private static void ExerciseCompositorOutput()
    {
        var device = RenderingServer.CreateLocalRenderingDevice() ??
            throw new NotSupportedException("Compositor output audit requires a Vulkan rendering device.");
        var output = new NativeCompositorOutput(device);
        using var samplerState = new RDSamplerState();
        var sampler = device.SamplerCreate(samplerState);
        try
        {
            foreach (var size in new[] { new Vector2I(8, 4), new Vector2I(3, 5), new Vector2I(8, 4) })
            {
                var pixels = new byte[size.X * size.Y * 8];
                for (var index = 0; index < pixels.Length / 2; index++)
                    System.Buffers.Binary.BinaryPrimitives.WriteUInt16LittleEndian(pixels.AsSpan(index * 2),
                        BitConverter.HalfToUInt16Bits((Half)((index + 1f) / (pixels.Length / 2 + 1))));
                using var sourceFormat = Format(RenderingDevice.TextureUsageBits.SamplingBit);
                using var targetFormat = Format(RenderingDevice.TextureUsageBits.ColorAttachmentBit | RenderingDevice.TextureUsageBits.CanCopyFromBit);
                using var view = new RDTextureView();
                var data = new Godot.Collections.Array<byte[]> { pixels };
                using var dataOwner = (Godot.Collections.Array)data;
                var source = device.TextureCreate(sourceFormat, view, data);
                var target = device.TextureCreate(targetFormat, view);
                try
                {
                    using var actual = device.TextureGetFormat(target);
                    if ((actual.UsageBits & RenderingDevice.TextureUsageBits.StorageBit) != 0)
                        throw new InvalidDataException("Audit accidentally admitted a storage-capable output.");
                    for (var repeat = 0; repeat < 2; repeat++)
                    {
                        output.Draw(source, sampler, target);
                        device.Submit(); device.Sync();
                        var readback = device.TextureGetData(target, 0);
                        if (!readback.SequenceEqual(pixels))
                            throw new InvalidDataException("Attachment output changed pixel coordinates, channels or half-float values.");
                    }
                }
                finally { device.FreeRid(target); device.FreeRid(source); }

                RDTextureFormat Format(RenderingDevice.TextureUsageBits usage) => new()
                {
                    Width = (uint)size.X,
                    Height = (uint)size.Y,
                    Depth = 1,
                    ArrayLayers = 1,
                    Mipmaps = 1,
                    TextureType = RenderingDevice.TextureType.Type2D,
                    Format = RenderingDevice.DataFormat.R16G16B16A16Sfloat,
                    UsageBits = usage
                };
            }
            GD.Print("OPENNV_COMPOSITOR_OUTPUT_PASS storageRequired=false halfFloatExact=true orientation=true channels=true repeated=true resize=true scope=synthetic-attachment-copy");
        }
        finally { device.FreeRid(sampler); output.Dispose(); device.Free(); }
    }
}
