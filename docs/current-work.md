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

## Next owners

1. Prioritize gameplay and content on the owner's direction. Cutscenes are
   deferred. Android experimental startup passes `--skip-cutscenes`; PlayBink
   retains command validation, explicitly logs skipped playback, and releases
   the movie blocker through the existing deferred continuation. This is an
   intentional playback omission, not movie support or script parity.
2. Verify APK 0.1.4 on the Air X. Cold startup already bypasses installation
   setup using the registered profile and retains the game's New/Continue menu.
   The prior device build reached original shader package 002, then failed on
   desktop `ffprobe` and missing HUD preferences. Missing/ambiguous profiles
   still return to setup, and B during startup requests setup.
3. Validate owned interface defaults and native UI policy. Integer initializers
   now admit unsigned INI identities with sections using the existing DWORD or
   sign-extended immediate forms and writable receiver validation. Unsigned
   settings prefer INI overrides, then the original executable association.
   The selected executable declares an unset custom HUD color. Native UI uses
   the installation's named HUD/main-menu channels when that value is zero.
   Owned setting checks pass; matched retail color-selection proof remains open.
4. Enter ordinary gameplay, exercise move/look/interaction and menus, save,
   quit, then cold Continue. The owner confirms D-pad focus and New Game
   confirmation in the prior build. Physical gameplay controls and persistence
   acceptance remain open. Do not infer playability from menu or parser checks.
5. Trace the six remaining missing references and other measured loading
   divergences. Keep the shared source-driven C# owners; no campaign shortcuts.
6. Profile loading, memory, frame time and transitions after the acceptance
   sequence works. Keep cutscene decoding in the backlog.

## Publication boundary

Run the required runtime gate and selected owned-data audit before publishing
runtime/support claims. Device measurements are required for device performance
claims. No complete campaign or fixed frame-rate outcome is established.
Upstream historical capability notes in status.md are not device acceptance.
