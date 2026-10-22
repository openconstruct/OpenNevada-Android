# Current work

## Objective and boundaries

Make the experimental ARM64 Android build fit the owner's Mangmi Air X without
omitting owned content or reducing authored texture quality. Keep the shared
C#/Godot gameplay and persistence path. Work without subagents. Cutscenes remain
deferred. Full campaign support and ordinary interaction/combat/travel acceptance
remain open. Selected rendering and restoration checks are not parity proof.

Retail inputs are private and read-only. The 277 installation files already
matched the owner's host installation by SHA-256; do not transfer them again.
Desktop graphics tests are disabled: an earlier Vulkan fixture triggered AMD
flip/commit timeouts and a desktop lockup. Use CPU/headless checks and the handheld;
do not reset the desktop display/GPU.

## Current build and device

APK 0.1.13/code 14 is installed; code 15 is being validated. Android 14/API 34, ARM64, about 3.57 GiB physical
memory and 2.5 GiB LZ4 zram. The current driver identifies Qualcomm Adreno 610,
Vulkan 1.1.295. Earlier runs used Turnip/Vulkan 1.0.328; do not treat those runs
as controlled comparisons. No driver setting was changed by this work.

Managed Android optimization is enabled. Owned uGridsToLoad=5 selects 25 cells
and 2,337 references rather than the previous forced 49/3,683. Internal 3D is
960x540; UI/output is 1280x720. Bloom, blur, double vision and exposure adaptation
are disabled. Output conversion, tint and fades remain active. The native
non-interactive top-right FPS counter updates while paused. Loading FPS may be
stale when synchronous preparation blocks drawing.

The Android source-payload LRU budget is 128 MiB; desktop remains 512 MiB.
Its counter measures retained raw bytes only, excluding readers' arrays, decoded
objects and graphics resources. The smaller cache evicted about 53 MiB during
an earlier attempt but did not prevent Android LOW_MEMORY.

## Shared uploads and native lifetime

Immutable 2D DDS textures and cubemaps now reuse native resources. Keys include
winning source identity, SHA-256 of actual bytes, normal validation policy and
resource kind. The per-installation index stores native instance IDs without
owning textures; native material/scene ownership controls lifetime. Expired
metadata is pruned. Animated materials/controllers retain independent state.

Scoped Variants release temporary packed mesh arrays and resource-valued shader
arguments promptly. Mesh/morph array containers and cubemap face images are
released after their destination takes ownership. Synthetic and owned-data
checks prove reuse after a managed wrapper is disposed while native materials
retain it; unused-resource release; changed bytes/source/policy isolation;
independent materials; authored alpha/mips; and immediate native packed-buffer
release without managed collection. The owned NPC assembly check retains all
13 Doc Mitchell parts and exact preparation vectors.

Performance snapshots label upload/reuse payload bytes as cumulative work,
not resident GPU/driver memory. They also report transparent-BC1 expansion.
The device supports BC1 RGB/RGBA, BC2 and BC3. However, this Godot version's
ordinary DXT1 upload path forces opaque BC1 and its Texture2DRD wrapper lacks
BC1_RGBA mapping. Transparent BC1 still uses the existing RGBA fallback;
compressed-alpha support is not implemented. The measured outdoor expansion
was roughly 46 MiB beyond compressed payload size, not gigabytes.

An opt-in first-cubemap resource readback verifies all six faces and authored
mips against uploaded RGBA bytes. Code 13 passed this check on the handheld for
an owned environment map. No frame recording is enabled by this check.

## Physical results and saves

The intermediate code-12 build on the same Qualcomm driver reduced Godot's
tracked interior video memory from about 389 MiB to 176 MiB. Its 765 texture
reuses are cumulative work avoided, not resident RAM saved. Stationary interior
performance was about 28–30 FPS, with no sustained-30 gameplay acceptance.

Ordinary door activation passed the previous prototype-350 failure point:
406 prototypes and terrain finished, the exterior committed and wrote an
authentic exterior autosave. Android then killed the process for LOW_MEMORY
before its first exterior frame. Cold Continue from that autosave rendered
Goodsprings using the owned 5x5 grid. Initial performance was roughly 4–9 FPS
with about 2,178 draw calls. This does not establish a successful live transition
or acceptable exterior performance.

Both the original interior save and newer exterior autosave are privately
backed up. Code 13 was installed and the exact original interior backup restored
and checksum verified for the repeatable door test. Ordinary Continue reached
cell-ready and authoritative world restoration. The cubemap GPU check passed.
After recharging/reconnection, code 13 restored the same save and rendered the
interior at about 27–29 FPS with tracked video memory about 172 MiB. Continue
took about 100 seconds to the first frame. Its ordinary door test prepared all
406 prototypes/terrain, committed after about 56 seconds and wrote an authentic
exterior autosave. Android reported LOW_MEMORY about 11 seconds after commit,
before the first exterior frame. Swap was about 2.1 GiB. That newer exterior
save is also privately backed up. Scoped native temporaries alone did not fix
the transition; the native heap remained about 1.1 GB allocated indoors.

The active handoff block gates destination presentation during source retirement:
free the previous room, evict prototypes outside the destination model set,
drain the mesh/material/shader-texture finalizer ownership depth, and drain the rendering device's frame
ring before revealing the complete destination. Authoritative state and its
autosave commit first. Gameplay remains paused during the gated frames; failures
release the pause/presentation gate. The selected headless owned-model check
passes geometry/contact retention, source/prototype release and failure cleanup.
Code 14 passed the full required gate and signed Android export, but its physical
door attempt died during destination preparation, before the retirement boundary.
Cold Continue from the genuine exterior save also hit LOW_MEMORY before rendering.
Neither run validates the new GPU retirement boundary or establishes a regression
cause. The device currently holds the private genuine code-13 exterior save.

A bounded native allocation profile captured roughly 242 MiB of new outstanding
allocations over about six seconds of exterior preparation: about 194 MiB through
Godot, 47 MiB through the Vulkan driver and 1 MiB through Mono. This is a sampled
partial window, not total heap attribution. Godot frames are stripped; their
specific C++ owners remain unresolved. The first startup profile overran its
buffer and was discarded. Traced, traced_probes and heapprofd were temporarily
started, then restored to stopped with persist.traced.enable=0.

Code 15 additionally shares exact-source immutable Shader programs via a
non-owning native-ID index, replacing permanent shader pins and repeated program
creation in the native NIF, FaceGen, LAND/LOD and sky material owners. Uniforms
remain per material. Redundant FaceGen strong DDS caching is removed. The main
SLS builder now disposes its temporary StandardMaterial input container and
copied environment pass after the final material takes ownership. The selected
owned shader audit passes exact-code identity, changed-code separation, native
material lifetime after wrapper disposal, unused release and independent uniform
state. Owned NPC assembly still retains all 13 parts. Physical savings remain
unmeasured; full code-15 validation/export/device comparison is next.

The owner completed SPECIAL, association, inkblots, tags and traits, reaching
VCG01 stage 200. Touch worked for tags/traits; newer controller changes still
need physical acceptance. A transient right-stick failure resolved after reload;
its owner remains open.

## Verification and next owners

Code 13 passes the complete required CPU/headless runtime gate: Release/Debug
builds, formatting/analyzers, contract probes, launcher checks and native Godot
audits. Selected owned DDS ownership and NPC assembly checks pass. Android debug
export succeeds and APK v2/v3 signatures verify. No desktop GPU test was run.

1. Validate/export code 15 and complete the same-save interior/door test. Verify
   first exterior rendering, not only commit/autosave. ADB currently uses port
   5038; do not assume the earlier 5040 server still owns the USB interface.
2. Trace remaining transition overlap and prototype/resource ownership against
   measured LOW_MEMORY. Do not omit scenery or invent location-specific limits.
3. Measure outdoor frame time and draw submission; 2,178 draw calls and 4–9 FPS
   show that fitting memory alone will not establish sustained 30 FPS.
4. Keep unsupported exterior controllers, Doc's unresolved travel/procedure,
   six missing interior references and unmatched reference-script branch visible.
   Exercise ordinary menus, travel, saving and cold Continue as a chain.

Earlier UI diagnostics reported two ObjectDB leaks; this separate issue remains
open. Initial compositor stalls of roughly 45 seconds and about 102-second total
Continue loading were observed earlier; shader/pipeline compilation versus other
first-frame work has not been isolated.
