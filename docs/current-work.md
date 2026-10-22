# Current work

## Active objective

Establish a reproducible Android ARM64 New Vegas build and exercise a small
ordinary gameplay sequence on a Mangmi Air X running the user's GammaOS fork.
Keep the shared C# gameplay/persistence owners and Godot presentation. Complete
campaign support remains open. Work without subagents.

The first acceptance sequence is launch -> select the owned install -> move/look
-> interact -> save -> quit -> cold Continue. No game-data or device gameplay
acceptance has been established in this fork. Launcher startup is verified.

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
- Android archive input bindings omit right-stick look. Review physical input as
  an independent capability, retaining modal and source-control gates.
- Owned installation checks pass base-game plugin/archive admission and the
  opening quests' compiled global-write bindings. One base master and seven
  archives are admitted. This does not establish ordinary opening gameplay.
- A signed ARM64 debug APK exports without warnings or errors. Android texture
  imports are enabled, the existing project mark is used as the icon, and stale
  diagnostic scenes are excluded from the package. The installed app reaches the
  launcher and filesystem picker at 1280x720 using Turnip and Forward Mobile.
  This is not owned-world rendering or ordinary gameplay acceptance.
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

## Next owners

1. Complete and verify the owned data transfer into app-accessible storage, then
   attempt ordinary launcher-driven content loading. A large ADB shell stream
   stalled; use the sync service and verify file integrity.
2. Integrate platform services separately from the Android archive's older
   desktop differences, guided by the next measured loading failure.
3. Integrate physical input with frame-rate-independent right-stick look,
   correct modal behavior, and safe disconnect/focus transitions.
4. Retain the passing owned-installation audits and extend them for each fix.
5. Test the acceptance sequence on the Air X, then select the next measured
   loading, memory, rendering, or gameplay failure.

## Publication boundary

Run the required runtime gate and selected owned-data audit before publishing
runtime/support claims. Device measurements are required for device performance
claims. No complete campaign or fixed frame-rate outcome is established.
Upstream historical capability notes in status.md are not device acceptance.
