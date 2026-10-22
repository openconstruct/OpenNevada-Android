using Godot;

namespace OpenNV.Runtime.Presentation.Rendering;

// Rendering-thread owner. Mobile scene color is a color attachment, not a
// storage image. Copy the completed compute result with an ordinary draw.
internal sealed class NativeCompositorOutput(RenderingDevice device) : IDisposable
{
    private Rid _shader;
    private readonly Dictionary<long, Rid> _pipelines = [];
    private readonly Dictionary<Rid, Rid> _framebuffers = [];
    private readonly Dictionary<(Rid, Rid), Rid> _uniforms = [];

    internal void Draw(Rid source, Rid sampler, Rid destination)
    {
        using var format = device.TextureGetFormat(destination);
        if ((format.UsageBits & RenderingDevice.TextureUsageBits.ColorAttachmentBit) == 0 ||
            format.Samples != RenderingDevice.TextureSamples.Samples1)
            throw new NotSupportedException("Compositor output requires a resolved color attachment.");
        EnsureShader();
        if (!_framebuffers.TryGetValue(destination, out var framebuffer) || !device.FramebufferIsValid(framebuffer))
        {
            foreach (var stale in _framebuffers.Where(pair => !device.FramebufferIsValid(pair.Value)).Select(pair => pair.Key).ToArray())
                _framebuffers.Remove(stale);
            var attachments = new Godot.Collections.Array<Rid> { destination };
            using var attachmentsOwner = (Godot.Collections.Array)attachments;
            framebuffer = device.FramebufferCreate(attachments);
            if (!framebuffer.IsValid) throw new InvalidOperationException("Could not create compositor output framebuffer.");
            _framebuffers[destination] = framebuffer;
            GD.Print($"OPENNV_COMPOSITOR_OUTPUT_TARGET backend=color-attachment format={format.Format} width={format.Width} height={format.Height} storageRequired=false");
        }
        var framebufferFormat = device.FramebufferGetFormat(framebuffer);
        if (!_pipelines.TryGetValue(framebufferFormat, out var pipeline) || !device.RenderPipelineIsValid(pipeline))
        {
            using var raster = new RDPipelineRasterizationState { CullMode = RenderingDevice.PolygonCullMode.Disabled };
            using var multisample = new RDPipelineMultisampleState();
            using var depth = new RDPipelineDepthStencilState();
            using var attachment = new RDPipelineColorBlendStateAttachment { EnableBlend = false };
            var colorAttachments = new Godot.Collections.Array<RDPipelineColorBlendStateAttachment> { attachment };
            using var colorAttachmentsOwner = (Godot.Collections.Array)colorAttachments;
            using var blend = new RDPipelineColorBlendState
            {
                Attachments = colorAttachments
            };
            pipeline = device.RenderPipelineCreate(_shader, framebufferFormat, -1,
                RenderingDevice.RenderPrimitive.Triangles, raster, multisample, depth, blend);
            if (!pipeline.IsValid) throw new InvalidOperationException("Could not create compositor output pipeline.");
            _pipelines[framebufferFormat] = pipeline;
        }
        var key = (source, sampler);
        if (!_uniforms.TryGetValue(key, out var uniforms) || !device.UniformSetIsValid(uniforms))
        {
            foreach (var stale in _uniforms.Where(pair => !device.UniformSetIsValid(pair.Value)).Select(pair => pair.Key).ToArray())
                _uniforms.Remove(stale);
            using var input = new RDUniform { UniformType = RenderingDevice.UniformType.SamplerWithTexture, Binding = 0 };
            input.AddId(sampler); input.AddId(source);
            var bindings = new Godot.Collections.Array<RDUniform> { input };
            using var bindingsOwner = (Godot.Collections.Array)bindings;
            uniforms = device.UniformSetCreate(bindings, _shader, 0);
            if (!uniforms.IsValid) throw new InvalidOperationException("Could not bind compositor output texture.");
            _uniforms[key] = uniforms;
        }
        var list = device.DrawListBegin(framebuffer, RenderingDevice.DrawFlags.IgnoreColor0);
        device.DrawListBindRenderPipeline(list, pipeline);
        device.DrawListBindUniformSet(list, uniforms, 0);
        device.DrawListDraw(list, false, 1, 3);
        device.DrawListEnd();
    }

    private void EnsureShader()
    {
        if (_shader.IsValid) return;
        using var source = new RDShaderSource
        {
            Language = RenderingDevice.ShaderLanguage.Glsl,
            SourceVertex = """
                #version 450
                void main() {
                    vec2 points[3] = vec2[](vec2(-1.0, -1.0), vec2(3.0, -1.0), vec2(-1.0, 3.0));
                    gl_Position = vec4(points[gl_VertexIndex], 0.0, 1.0);
                }
                """,
            SourceFragment = """
                #version 450
                layout(set = 0, binding = 0) uniform sampler2D completed_image;
                layout(location = 0) out vec4 output_color;
                void main() {
                    output_color = texelFetch(completed_image, ivec2(gl_FragCoord.xy), 0);
                }
                """
        };
        using var compiled = device.ShaderCompileSpirVFromSource(source);
        if (!string.IsNullOrWhiteSpace(compiled.CompileErrorVertex) || !string.IsNullOrWhiteSpace(compiled.CompileErrorFragment))
            throw new InvalidOperationException($"Compositor output shader failed: {compiled.CompileErrorVertex} {compiled.CompileErrorFragment}");
        _shader = device.ShaderCreateFromSpirV(compiled, "OpenNV_CompositorAttachmentOutput");
        if (!_shader.IsValid) throw new InvalidOperationException("Could not create compositor output shader.");
    }

    public void Dispose()
    {
        foreach (var uniforms in _uniforms.Values.Where(device.UniformSetIsValid)) device.FreeRid(uniforms);
        foreach (var framebuffer in _framebuffers.Values.Where(device.FramebufferIsValid)) device.FreeRid(framebuffer);
        _framebuffers.Clear(); _uniforms.Clear(); _pipelines.Clear();
        if (_shader.IsValid) device.FreeRid(_shader);
        _shader = default;
    }
}
