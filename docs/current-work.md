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

## Current block

Prepare APK 0.1.7/code 8:

- Share a functional tag/trait selection control with initial and visible focus,
  explicit D-pad navigation, A select/unselect, X confirmation, and scrolling.
  Original C# selection limits and validation stay authoritative. Exact tag
  count, optional zero traits, rejection at the limit and deselection are tested.
- Disable bloom at the owner's request, in addition to automatic exposure,
  source blur and double vision. Skip bloom targets/downsampling/blur passes;
  preserve cinematic color, tint, fades and output color conversion. Other
  presentation modes retain defaults. Release compositor resources explicitly on
  cell presentation teardown; the GPU fixture exposed retained shader/pipeline
  allocations. This does not establish an exterior memory fix. Correct the
  obsolete image-space summary
  to report the actual automatic-exposure policy.
- Place quest updates in the upper-left safe inset for the handheld, explicitly
  requested by the owner. This is an OpenNV presentation policy; original native
  placement is unbound. Measure wrapped title height before placing objectives,
  preventing text overlap. Preserve source text, event order and notification time.

Owned selection, GPU effects and HUD placement checks pass, as does the full
runtime gate. The effects fixture exits without its earlier RID leak warnings.
The 720p HUD preview was inspected and deleted. Signed ARM64 export completes
without warnings or errors and its v2/v3 signature verifies. Installation is verified as
0.1.7/code 8. Ordinary device acceptance of these menus is still pending. Keep a
handoff note and experimental APK in the task outputs. Do not interrupt a resumed
playtest when installing; coordinate with the owner if it has restarted.

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
