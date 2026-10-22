# Current work

## Active objective

Establish a reproducible Android ARM64 New Vegas build and exercise a small
ordinary gameplay sequence on a Mangmi Air X running the user's GammaOS fork.
Keep the shared C# gameplay/persistence owners and Godot presentation. Complete
campaign support remains open. Work without subagents.

The first acceptance sequence is launch -> select the owned install -> move/look
-> interact -> save -> quit -> cold Continue. Launcher startup, installation
selection and owned title art/music are verified. Ordinary device gameplay
acceptance remains open.

## Verified baseline

- Source starts at Android fork commit `7fb55d6`, one README-only commit after
  desktop commit `ab2a603`. A tracked ARM64 Android preset now selects the existing
  launcher, Mobile renderer, and Android-only net9.0 framework override.
- The original Android Beta is a render/menu/audio validation release. Its
  separate source archive mixes Android additions with earlier desktop versions;
  it must not replace the working tree wholesale.
- The full runtime gate passes with Godot 4.7.2 Mono and .NET SDK 9.0.311 on Linux:
  Release/Debug builds, formatting/analyzers, 38 C# probes and native Godot scene
  checks. The Windows diagnostic helper is cross-compiled on this host.
- Desktop syntax checks pass. The retained JavaScript suite reports 11 passes,
  two skips, and zero failures. These checks are not gameplay proof.
- The original Android archive omits right-stick look. The current controller
  adapter binds menu navigation/accept/cancel and the shared move, look,
  interaction, jump, reload, Pip-Boy, pause, save and trigger actions. Desktop
  bindings remain intact. Right-stick look integrates elapsed time and respects
  modal, source-control, focus, capture, XR and furniture gates.
- Owned installation checks pass base-game plugin/archive admission and the
  opening quests' compiled global-write bindings. One base master and seven
  archives are admitted. This does not establish ordinary opening gameplay.
- A signed ARM64 debug APK exports without warnings or errors. Android texture
  imports are enabled, the existing project mark is used as the icon, and stale
  diagnostic scenes are excluded from the package. The installed app reaches the
  launcher and filesystem picker at 1280x720 using Turnip and Forward Mobile.
  This is not owned-world rendering or ordinary gameplay acceptance.
- The device's 277 installation files match their desktop SHA-256 hashes.
  The install is registered in private app storage. Ordinary launch indexes
  465,016 records and prepares the opening cell; title art is visible and the
  owner confirms music playback. Those counts are not gameplay acceptance.
- Baseline cell preparation reports 67 missing references, including packed
  collision failures in render-mesh index extraction. HUD initialization also
  fails on the missing owned `[Interface] uHUDColor` setting. These are visible
  divergences, not permission to omit collision or invent presentation defaults.
- Packed collision now builds directly from decoded source triangles, keeping
  face order and material-table alignment without render upload/readback.
  Synthetic shape validation and three affected owned models pass locally.
  Updated APK 0.1.1 (code 2) retains the registered installation and completes
  the same device preparation with 429 observed / 435 discovered references,
  six missing, and no packed-collision extraction errors. HUD initialization
  still fails on `uHUDColor`. Reference counts are not gameplay or parity proof.
- Android editor quick-deploy discovery is disabled in the preset. Its enabled
  state reproduces a 4.7.2 editor shutdown error even in an empty project. CLI
  export and ADB installation both succeed.

## Device observations

Read-only ADB reports Android 14/API 34, ARM64, a 1280x720 display override,
approximately 3.57 GiB physical memory, and 2.5 GiB LZ4 zram. The Vulkan report
identifies Turnip Adreno 610 and reports BC texture support. These observations
do not establish Godot rendering, texture correctness, or game memory needs.
Both AIR X Controller and virtual controller/mouse inputs are exposed; device
selection and overlapping input need acceptance checks.

## Current device result and next owners

APK 0.1.5 (code 6) reached ordinary character creation acceptance, bed exit,
SPECIAL allocation and VCG01 stage 80. A native world save was written at stage
55 without a prior file. Cold Continue is unverified. The current session failed
when speech ended and package locomotion resumed: its new animation clock kept
an old published root distance, reporting backwards accumulation. The owner
remains in that dialogue; do not restart unsaved play without coordinating.

APK 0.1.6 (code 7) is installed. An ordinary title-menu Continue after the app
restart restored stage 55 and the room rendered. This is a selected cold-save
restoration, not the complete interaction/save acceptance sequence:

- Copy completed compute output into Mobile scene color with a color-attachment
  draw; that scene texture does not support the old storage-image binding.
  Synthetic Vulkan readback checks coordinates, channels, reuse and resize.
  This transport check is separate from source rendering or device performance.
- Honor the owner's Android preference: retain bloom and output color transfer,
  omit automatic exposure adaptation, source blur and double vision. Gameplay
  image-space state still updates. Other presentation modes retain their defaults.
- Accrue quest script time on new low-FPS frames even when already overdue.
  The regression and owned opening-program fixtures cover 60 and 6 FPS; fixture
  callbacks are not ordinary gameplay acceptance.
- Reset published locomotion distance when a new walk animation clock begins,
  preserving the actual navigation path and world position.
- Show contextual SPECIAL controller help and log character-preview preparation
  timings. Preview diagnostics do not establish handheld layout acceptance.

The required runtime gate, owned shader programs, opening-clock fixtures and
menu input audit pass. The latter reports two leaked ObjectDB instances on exit.
The exact 720p SPECIAL preview was inspected and deleted. Synthetic Vulkan
attachment readback passes. Signed ARM64 export completes without warnings or
errors; these checks do not establish ordinary device behavior.

The device logs the new attachment output on its 1280x720
A2B10G10R10UnormPack32 scene color. The prior storage-image binding failures are
absent from the captured resumed-world log. The source reference-script parse
divergence remains. The explicit policy log confirms bloom and output transfer
on, automatic exposure/blur/double vision off; an older image-space summary
still hardcodes adaptation=runtime and must be corrected in the next telemetry
block.

Next executable outcome: retry ordinary SPECIAL controls, Doc movement and the
next-test transition from the resumed stage-55 session. Measure device frame time
after the compositor correction; no FPS improvement is established. Keep cutscenes deferred.

The six missing references, reached reference-script parsing divergence,
movement/physics, expensive character preparation, active interaction save
continuations and full campaign support remain open. Retain their visible errors;
do not invent quest transitions or relax source admission to continue a demo.

## Publication boundary

Run the required runtime gate and selected owned-data audit before publishing
runtime/support claims. Device measurements are required for device performance
claims. No complete campaign or fixed frame-rate outcome is established.
Upstream historical capability notes in status.md are not device acceptance.
