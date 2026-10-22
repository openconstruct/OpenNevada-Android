# Android bring-up

The initial target is an ARM64 Mangmi Air X with Android 14, a 720p system
display override, and Turnip. A signed debug APK has been exported and its
launcher exercised on that device. Ordinary gameplay acceptance remains open.

## Build configuration

Use Godot 4.7.2 Mono and its matching export templates, the .NET SDK pinned in
`global.json`, JDK 17, Android SDK platform 36 and build-tools 36.0.0. Configure
the Android SDK and Java SDK paths in Godot's editor settings. Keep local SDK
paths and signing credentials outside tracked source.

The preset is `Android Handheld Experimental`. It exports ARM64 only, uses a
separate package identity, and passes `-- --launcher --auto-launch` as its launch
arguments. A remembered campaign, or a single existing profile, enters the game
without showing the installation launcher. First setup, missing files, ambiguous
profiles without a remembered selection, or rejected launch validation return to
the picker. Holding B (or keyboard Escape) during startup requests the launcher.
The game's own New/Continue menu remains available; startup never creates or
overwrites a save automatically.
The Android framework is net9.0, as required by Godot 4.7.2's non-Gradle C#
template; desktop retains net8.0. Godot's Mobile renderer is selected through a
platform override. ETC2/ASTC imports are enabled as required by Android export.
The package uses the existing project mark and excludes diagnostic tool scenes;
several retained audit scenes reference scripts absent from the source tree.
Shader/material compatibility with owned content remains a device check.

Framework selection can be inspected without exporting:

```sh
dotnet msbuild runtime/OpenNV.csproj -nologo -getProperty:TargetFramework
dotnet msbuild runtime/OpenNV.csproj -nologo -getProperty:TargetFramework -p:GodotTargetPlatform=android
```

Once the export dependencies are configured:

```sh
godot --headless --path runtime --export-debug "Android Handheld Experimental" /absolute/output/OpenNevada.apk
adb install -r /absolute/output/OpenNevada.apk
```

These steps completed with no exporter errors or warnings on the Linux host.
The APK contains only the ARM64 native ABI and includes the managed assembly.
It contains no retail master/archive/executable/movie entries. Android
editor quick-deploy discovery is disabled because it triggers an EditorSettings
shutdown error in 4.7.2. The issue reproduces with an empty project; disabling
discovery leaves a clean editor exit and does not disable command-line export.

## Verified device boundary

The debug package installs as `io.github.openconstruct.opennevada`. A cold start
reaches `OPENNV_GODOT_LAUNCHER_READY`, and a focused visual check confirms the
launcher and filesystem picker at 1280x720. Godot reports Vulkan 1.0.328,
Forward Mobile, and Turnip Adreno 610. Device texture reporting and launcher
rendering do not establish owned world rendering, control support or gameplay.

The selected desktop installation passes plugin/archive admission and opening
compiled global-write audits: one base master and seven archives. All 277 copied
device files match their desktop SHA-256 hashes. The private installation is
registered through the ordinary picker. Launch indexes the base plugin and
prepares the opening cell, with visible title art and owner-confirmed music.
Baseline preparation reports 67 missing references, including packed collision
index-extraction failures, and HUD initialization reports missing owned
`[Interface] uHUDColor`. Ordinary gameplay remains unverified.

Packed collision now expands the decoded source triangles directly into the
physics shape. Face order, coordinate conversion, scale and backface collision
are retained without a render-mesh upload/readback. Synthetic validation checks
those properties and invalid inputs; targeted owned table, gun cabinet and wall
models pass the native shape audit. Updated APK 0.1.1 (version code 2) completes
the same cell preparation with 429 observed references out of 435, six missing,
and no packed-collision extraction errors. The HUD settings failure remains.
These counts do not establish actor behavior, world rendering or gameplay.

Keep installation data separate
from the APK and save state in app-owned storage. Shared app storage can reject
ADB sync directory creation under Android 14; debug private storage is available
through `adb shell run-as io.github.openconstruct.opennevada`. A large shell
stream stalled this device connection; file transfers need integrity checks.

## Source integration

The original Android release supplies changes separately in a source archive.
Review startup, access to the selected installation, input, serialization, movie
playback, and rendering individually. That archive also contains earlier desktop
code; importing it wholesale would remove newer gameplay and persistence fixes.

## Native renderer selection

Windows `RendererInfo.txt` is optional for native startup. An existing report
still selects its original shader package strictly; malformed reports fail.
Without that report, OpenNV uses a deterministic native policy: the first owned
shader package in logical path order whose required image-space programs decode
with supported instructions and parameter interfaces. This selects original
programs, not substitute filters or a fabricated Windows hardware report.
Malformed data fails; unsupported candidates can be rejected during negotiation.
The selected source and policy are logged as `OPENNV_IMAGE_SPACE_PROGRAMS_READY`.
This is an OpenNV backend policy, not reconstructed retail hardware detection or
pixel parity. The rendered-menu path shares that selection. Decoded programs are
reused in-process and never written as persistent launch assets.

The owned probe admits seven blur programs and the prefilter/double-vision
programs from the installed source. Synthetic checks retain explicit renderer
selection and reject malformed reports. Device world rendering remains open.

## Controller input

Experimental APK 0.1.2 (code 3) adds explicit menu accept/cancel/navigation and
shared gameplay action bindings. Existing keyboard and mouse events are retained;
reconfiguring the controller map does not append duplicate events. A registered
installation initially focuses Play. The start menu focuses the first enabled
entry, shows a focus outline, and defaults New Game confirmation to No. B cancels
the confirmation and returns focus to the main menu. Touch selection remains
available.

| Control | Binding |
| --- | --- |
| Menu D-pad / left stick | Navigate |
| Menu A / B | Accept / cancel |
| Gameplay left / right stick | Move / look |
| A / Y / X / B | Interact / jump / reload or ready weapon / Pip-Boy |
| RT / LT | Fire / aim |
| Start / Back | Pause / quick save |
| Left stick press | Sprint |

Right-stick look uses elapsed physics time and a radial deadzone. It shares
mouse-look limits and source-control/furniture handling and is suppressed during
modal UI, XR, uncaptured input or lost window focus. A disconnected-controller
look sample is zero. This implements presentation input, not new gameplay state.
Physical controls, focus/disconnect transitions, combat and saves still require
ordinary device acceptance; menu input checks do not establish those outcomes.

The native `--menu-input <owned-install>` audit exercises Play, D-pad selection,
A acceptance, B cancellation, confirmation, retained desktop bindings, duplicate
configuration and look integration at two frame rates without starting a campaign.

## First device acceptance

Use ordinary physical controls and the user's own installation:

1. Open the launcher, select the installation, and enter the existing opening.
2. Move, look, interact, and enter/exit a menu.
3. Save to app-owned storage, quit, and cold Continue with the same state.
4. Exercise pause, focus loss, controller disconnect, and reconnect.

Right-stick look requires frame-rate-independent handling and must honor modal
and source-control gates. Multiple exposed controller/mouse devices require
selection checks. Profile memory, loading, frame times, and cell transitions
after the sequence works. A rendered menu or desktop probe is not this acceptance.

Retail data remains read-only and external. Packages include no game data or
saves. The shared C# gameplay/save owners remain authoritative.

## Gameplay testing without cutscenes

The owner has deferred cutscenes in favor of gameplay and content. APK 0.1.4
(code 5) passes `--skip-cutscenes` in the Android experimental preset. The shared
PlayBink owner validates the command, logs `OPENNV_MOVIE_SKIPPED` with its source
and policy, and releases the script's movie blocker through the existing deferred
continuation. It does not invoke codec helpers or modify the movie file. Desktop
presets retain playback. This deliberately omits cutscenes; it is not a decoder
fix or original cutscene timing proof.

Unsigned installation settings now read their original executable defaults when
no INI override exists. Admitted typed initializers retain their section, DWORD
bits, writable receiver and ambiguity checks. Native UI treats a zero custom HUD
color as unset and uses named system channels from the installation INI. This is
an explicit native backend policy; matched retail color selection is unverified.
The owned `--audit-interface-settings <installation>` probe checks that both HUD
and main-menu colors retain their selected source channels.
