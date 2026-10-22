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

APK 0.1.12/code 13 is installed. Android 14/API 34, ARM64, about 3.57 GiB physical
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
USB then disappeared before the first gameplay frame could be verified; the
new native-lifetime changes' physical memory/performance impact and live door
transition remain unmeasured. Do not infer a crash from the lost connection.
The final device screen/running state is unknown until USB reconnects.

The owner completed SPECIAL, association, inkblots, tags and traits, reaching
VCG01 stage 200. Touch worked for tags/traits; newer controller changes still
need physical acceptance. A transient right-stick failure resolved after reload;
its owner remains open.

## Verification and next owners

Code 13 passes the complete required CPU/headless runtime gate: Release/Debug
builds, formatting/analyzers, contract probes, launcher checks and native Godot
audits. Selected owned DDS ownership and NPC assembly checks pass. Android debug
export succeeds and APK v2/v3 signatures verify. No desktop GPU test was run.

1. Reconnect USB, identify the current device state and complete the same-save
   interior/door test. Verify first exterior rendering, not only commit/autosave.
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
