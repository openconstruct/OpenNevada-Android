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
The cache was not full; reducing its budget alone is not an evidenced fix.
Unsupported exterior controller sequence chains remain visible.

## Current diagnostic block

APK 0.1.9/code 10 is installed with data preserved. The opt-in --fps-overlay
is enabled in its preset. It displays
engine FPS at the top right on the native-resolution canvas, samples twice per
second, continues when paused and ignores pointer/controller focus. It neither
caps FPS nor establishes a performance improvement. Keep the same APK/save when
comparing the owner's Turnip auto, prefer-sysmem and prefer-gmem settings.

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
FPS sampling while paused. The full required runtime gate passes. These checks
do not establish device performance gains.

This block passed Release/Debug builds, formatting/analyzers, the full contract
and native Godot gate, and selected owned-data diagnostics. Its owned controller
choices and GPU effects diagnostics exit without warnings. Earlier menu-input
diagnostics reported two leaked ObjectDB instances; that separate issue is open.
No retail files, saves, executables or derivatives enter the repository/package.
