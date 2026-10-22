# Current work

## Objective and boundaries

Improve the experimental ARM64 Android build on the owner's Mangmi Air X and
GammaOS fork. Keep one shared C#/Godot gameplay and persistence path. Work without
subagents. Cutscenes remain deferred. Full campaign support and ordinary
interaction/combat/travel acceptance remain open; checks and selected restoration
are not parity or complete gameplay proof.

## Installed and observed

APK 0.1.7/code 8 is now installed; the following gameplay observations are from
the preceding APK 0.1.6/code 7. The owner completed SPECIAL,
word association, inkblots, tag skills and traits, reaching Doc's farewell at
VCG01 stage 110, and later stage 200 before the door attempt. Tag/trait selection needed touchscreen input. Save at stage 110
was written successfully, then selected through the ordinary save browser.
The owner reported right-stick look stopped while walking still worked; both
worked after loading. Its cause remains unmeasured; do not claim it fixed.

The owner attempted the exterior door. Android records a LOW_MEMORY process
exit during Goodsprings preparation, at about 2.1 GB reported PSS. The captured
log also contains unsupported exterior NIF controller sequence chains. Loading
that session cannot finish after process death. The post-test stage-110 save is
retained. Do not erase installation data or overwrite it with a fixture.

Mobile compositor attachment output is active on the device's 1280x720
A2B10G10R10UnormPack32 scene color. Previous storage-image binding errors are
absent from the captured resumed-world log. FPS remains unsatisfactory and no
improvement is quantified. Synthetic exact-half-float attachment readback is a
separate transport check, not rendering parity.

The device has Android 14/API 34, ARM64, about 3.57 GiB physical memory and
2.5 GiB LZ4 zram, with Turnip Adreno 610. Its 277 private installation files were
SHA-256 verified against the owner's host install; do not transfer them again.

## Current performance block

Prepare APK 0.1.8/code 9. The owner reports about 20 FPS subjectively with bloom
off, but no measured frame-rate baseline exists. The second exterior attempt
also ended with Android LOW_MEMORY (PID 22798, about 2.0 GB reported PSS). A
sample immediately before it had about 1.85 GiB swapped out. The ordinary save
from stage 110 remains the continuation point.

Concrete source divergences found:

- The runtime forced a minimum 7x7 exterior grid despite the owned default INI
  requesting 5x5. Remove that artificial floor and let owned uGridsToLoad drive
  residency through the existing grid resolver. Source identity, spatial
  admission, collision and continued streaming remain the same owners.
- The signed Android development export compiled C# with Optimize=false. Enable
  managed optimization for Android without changing desktop Debug configuration.
- Apply the owner's selected 0.75 scale to flat-world 3D only: 960x540 internal
  rendering, 1280x720 UI/output. Restore scale 1 for XR and reject invalid scales.
- Add opt-in, sparse performance/memory logging and door-loading checkpoints.
  Report actual managed optimization, FPS/process/physics time, draw calls,
  managed/Godot/video memory, Linux RSS/swap and source payload cache state.
  Frame capture stays off; disabled logging does no sampling work.

The owned grid/render-extent audit and full runtime gate pass. The selected
graph is 25 cells/2,337 references versus the earlier 49/3,683; these counts are
not device memory or gameplay proof. Vulkan confirms 960x540 internal 3D with
1280x720 UI/output. Android ExportDebug properties report Optimize=true/net9.0.
Finish signed export and publication of the checked candidate. Installation
and ordinary cold Continue/exterior retry remain necessary to establish device
performance or an exterior-memory fix. The default source cache is still a
512 MiB budget; do not equate that budget with measured bytes or reduce it
without checking the new memory checkpoints.

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

This block passed Release/Debug builds, formatting/analyzers, the full contract
and native Godot gate, and selected owned-data diagnostics. Its owned controller
choices and GPU effects diagnostics exit without warnings. Earlier menu-input
diagnostics reported two leaked ObjectDB instances; that separate issue is open.
No retail files, saves, executables or derivatives enter the repository/package.
