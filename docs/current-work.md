# Current work

## Objective and boundaries

Improve the experimental ARM64 Android build on the owner's Mangmi Air X and
GammaOS fork. Keep one shared C#/Godot gameplay and persistence path. Work without
subagents. Cutscenes remain deferred. Full campaign support and ordinary
interaction/combat/travel acceptance remain open; checks and selected restoration
are not parity or complete gameplay proof.

## Installed and observed

APK 0.1.8/code 9 was installed and physically retried. The owner previously
completed SPECIAL, word association, inkblots, tag skills and traits, reaching
VCG01 stage 110 and later stage 200. Tag/trait selection used touch; the newer
controller menu changes still need physical acceptance. A stage-110 save was
written and restored through the ordinary save browser. Preserve it and the
registered installation. A transient right-stick failure resolved after reload;
its cause remains open.

The device has Android 14/API 34, ARM64, about 3.57 GiB physical memory and
2.5 GiB LZ4 zram, with Turnip Adreno 610. Its 277 private installation files were
SHA-256 verified against the owner's host install; do not transfer them again.

## Measured performance and loading

APK 0.1.8 enables managed Android optimization and follows owned uGridsToLoad=5,
selecting 25 cells/2,337 references rather than the previous forced 49/3,683.
Vulkan confirms 960x540 internal 3D with 1280x720 UI/output. Bloom, blur, double
vision and exposure adaptation remain disabled. Output conversion stays active.
The owned grid/render-extent audit and full runtime gate pass.

Device telemetry confirms optimized managed code. Doc's room measured 20–22 FPS;
sustained 30 FPS is not established. Cold Continue took about 56 seconds to
restore state and 72 seconds to the first compositor target. These are one-run
observations, not a controlled performance comparison.

The third exterior attempt also ended with Android LOW_MEMORY, reported PSS
about 2.2 GB, during model preparation and before model-ready/commit. At the last
350-prototype checkpoint, source payload cache held about 188 MB of its 512 MiB
budget, managed allocation was about 503 MB and swap about 2.0 GiB. These memory
counters overlap; do not sum them as separate physical allocations. Loading-phase
FPS/video counters may be stale while synchronous preparation blocks drawing.
The cache was not full; reducing its budget alone is not an evidenced crash fix.
Unsupported exterior controller sequence chains remain visible.

## Current cache-budget block

The owner requests a smaller cache and authorizes restarting from the title.
APK 0.1.10/code 11 is installed with registered data preserved. Its Android
source payload cache is reduced from
512 MiB to 128 MiB. Desktop retains 512 MiB. Existing LRU eviction, oversized
resource bypass and source rereads retain their behavior; source selection and
content are unchanged. Eviction counts/bytes and an explicit accounting label
clarify telemetry. The byte counter measures only retained source payloads,
not managed object overhead, in-flight reads, caller-held arrays, decoded NIFs
or GPU resources. Eviction removes cache ownership; it cannot free data still
held by readers. No immediate 384 MiB saving or exterior-memory fix is claimed.
More rereads can increase loading time; measure the actual tradeoff on-device.
Device startup confirms budgetBytes=134217728 and the new accounting label.
The save SHA-256 is unchanged across install. The active driver now identifies
Qualcomm Adreno 610/Vulkan 1.1.295 rather than the earlier Turnip/Vulkan 1.0.328;
no driver setting was changed by this work. Performance comparisons across
those runs are confounded by that driver difference.

Physical code-11 Continue restored the save in about 56 seconds; first compositor
output arrived about 102 seconds after Continue. The stationary interior view
reported roughly 27–30 FPS, without a sustained-30 gameplay claim. A temporary
frame confirmed the room and top-right counter; it was deleted. Ordinary movement
and activation then opened the exit. During exterior preparation the source
cache stayed below 128 MiB and evicted 397 entries/about 53 MiB by prototype 350.
Android still killed PID 27243 for LOW_MEMORY at about 2.2 GB reported PSS before
model-ready/commit. The smaller cap works but does not fix exterior loading. The
save is preserved; duplicate texture uploads and transition overlap remain next.

The top-right FPS overlay remains enabled, with 960x540 3D and native 1280x720
UI/output. Keep the same APK/save and driver when comparing changes. The code-11
stationary interior view measured roughly 27–30 FPS, with dips during movement;
this does not establish sustained 30 FPS or isolate the cache change.

Ordinary NIF material loading currently creates DDS textures repeatedly, and
animated prototype instances rebuild materials/controllers. Investigate sharing
immutable texture resources without sharing mutable controller/material state
or retaining unused resources. This is a source finding, not an implemented
memory fix. Do not omit scenery, reduce authored texture quality or change
source residency speculatively.

## Next divergence owners

1. Bound exterior preparation memory and resource lifetime using the measured
   LOW_MEMORY exit. Trace source admission, decoded data, prototypes and GPU
   resources; do not omit scenery or invent location-specific limits.
2. Measure frame time and loading; retain the transient right-stick failure and
   inspect modal/capture/focus gates if it recurs. Do not remove their guards
   without evidence of the incorrect owner.
3. Trace the unsupported exterior controller chains, six earlier missing interior
   references and source reference-script parse divergence (unmatched branch end).
4. Exercise ordinary menus/controls, travel, saving and cold Continue as a chain.
   Active furniture/speech/modal continuation and full campaign support remain
   open. Preserve visible failures rather than skipping gameplay state.

## Verification and publication

The FPS overlay passes the selected owned grid/Vulkan audit: native top-right
bounds at 1280x720 with 960x540 3D, no pointer interception, and numeric engine
FPS sampling while paused. The earlier FPS block's full required runtime gate passed. These checks
do not establish device performance gains.

Earlier UI work passed Release/Debug builds, formatting/analyzers, the full contract
and native Godot gate, and selected owned-data diagnostics. Its owned controller
choices and GPU effects diagnostics exit without warnings. Earlier menu-input
diagnostics reported two leaked ObjectDB instances; that separate issue is open.
No retail files, saves, executables or derivatives enter the repository/package.

## Desktop graphics boundary

The owner confirmed recovery from the desktop lockup. Kernel logs showed AMD
flip/commit timeouts and Xorg/amdgpu warnings during the desktop Vulkan fixture;
it was stopped without a result. Do not repeat desktop GPU fixtures or reset
the desktop display/GPU. Use CPU-only/headless checks and handheld rendering.
The complete required headless gate has since passed, including Release/Debug,
formatting/analyzers, cache LRU contracts and native Godot audits. The selected
owned installation/source audit also passes without opening a desktop window.

Code 10 reload PID 26721 restored the saved state but stalled before the first
world frame. Drawing resumed about 46 seconds later; telemetry then reported
21–28 FPS. The loading overlay's 58 FPS was stale. This was a long first-frame
stall, not a new LOW_MEMORY death. Its precise owner remains unisolated. The
owner authorized replacing the running session and restarting from the save.

After the exterior kill, code 11 was relaunched and ordinary Continue restored
the unchanged save again. The room rendered and the ordinary pause menu was
opened, leaving the handheld paused in the working interior. Temporary display
checks were inspected and deleted. No frame recording remains enabled.
