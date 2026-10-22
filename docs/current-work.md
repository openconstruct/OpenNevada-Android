# Current work

## Objective and boundaries

Make ordinary Fallout: New Vegas gameplay work on the owner's Mangmi Air X,
including loot/controller interactions, while keeping the runtime within memory.
Shared first-party C#/Godot gameplay and saves remain authoritative. No subagents.
Cutscenes remain deferred. No campaign, world, controller or retail parity claim.

Retail files are private/read-only. All 277 installation files previously matched
host SHA-256; do not transfer again. Desktop GPU/X11 Godot tests are prohibited:
an earlier fixture hung AMD/Xorg. Use CPU/headless checks and handheld rendering;
do not reset the desktop display/GPU or change drivers for comparison.

## Candidate

APK 0.1.17/code 18 is installed, SHA-256
`a6d90e3f1fd270b89c9270afbb62b7ecdd052204659da46b5b489b5e56fc8194`.
The required full local runtime gate, selected owned interaction/controller audit,
Android export and v2/v3 signatures pass. The owner confirmed physical D-pad
selection, A transfer and B close on the handheld.

Android 14/API 34, SM6115/Adreno 610, about 3.57 GiB physical RAM and 2.5 GiB LZ4
zram. Qualcomm Vulkan 1.1.295 is current; earlier runs used Turnip 1.0.328. No
driver change was made. ADB port 5038 currently owns USB, serial MQ65172511055656.

World 960x540, UI/output 1280x720, managed Android optimization, source uGrids=5.
Bloom/blur/double/adaptive exposure off; output conversion/tint/fades remain.
Outdoor draw cap is the owner's selected 200 metres, with matching fog ending
by 190 metres. Gameplay residency/collisions remain intact. Raw-source LRU is
128 MiB on Android / 512 MiB desktop and counts retained payloads only.

## Reported first-aid-box failure

The owner traversed other building interiors and reported higher FPS, then an
apparent freeze while looting. Code-17 PID 17478 remained alive and rendered the
container overlay at 25 FPS, about 1.03 GiB RSS and 477 MiB process swap, with
about 631 MiB system free memory. This was not an observed process death/OOM.
The source first-aid box FalloutNV.esm:1767aa opened four items in interior
1055e1. Its menu had no initial row focus or dedicated gamepad Take All/Exit;
flat gameplay was intentionally paused behind an unresponsive controller UI.
Keyboard E closed it, restored control and persisted the current interior state.

General fix: initial visible container focus; D-pad/stick sides and rows; A
transfer; X Take All; B close; LB/RB page. Selection survives refresh and quantity
cancellation. One step per stick deflection rejects held-axis noise. Quantity
menus accept left/right amount, LB/RB +/-10, A confirm and B cancel. Source
quantity thresholds, transfer revalidation, inventory/save ownership and keyboard/
touch input remain. Separate hints avoid overlapping the active quantity picker.

The owned headless audit passes raw gamepad routing while paused, partial transfer
and deposit count conservation, modal isolation/cancellation, page navigation,
stick hysteresis and repeated Take All/Exit. It also resolves the reported first-
aid box through winning CNTO/LVLI records. The final run has no new leak warning.

Private checkpoint before installation:
`work/opennv-pre-container-controls-save.private.json`, SHA-256
`2e7c9dc97e5c6c05101122300fd640a40e6c33068db38e3bb6cd04708f9c3898`.
It was saved after closing the container at 19:41:30 local, in interior 1055e1,
VCG01 stage 200. Keep earlier checkpoints too; never edit save fields or inject
world/player state to manufacture success. Code-18 PID 20900 Continue began at
19:42:57; scene draw telemetry resumed at 19:43:38 (about 42 seconds), later
23 FPS at this view. Ordinary E reopened the same box at 19:44:26 with all four
items, initial Stimpak focus, visible outline and readable controller hints.
A temporary frame was inspected/deleted. The owner then confirmed real D-pad
selection, A transfer and B close. Native saves completed at 19:44:58, 19:45:07
and 19:45:11. The app remains running in the user's current interior session;
do not restart or restore an older checkpoint without preserving new progress.

Post-acceptance backup: work/opennv-v18-controller-accepted-save.private.json,
SHA-256 462b01023d0b6ecdb1370a31caa2ec184ebc8e4058cd320616e9f468df36231f.
The code-18 log collector stopped after the bounded check. Hardware paging,
quantity selection and Take All have software-audit coverage but not separate
physical acceptance in this run.

## Retained memory/performance result

Merged PRs #14–16 share immutable DDS/cubemaps and exact-source Shader programs
through non-owning native-ID indexes. Materials retain independent uniforms.
Temporary Variants/upload arrays and unused StandardMaterial containers release
native references promptly; redundant FaceGen pins are removed. After a genuine
door commit/autosave, source scenes/prototypes retire through bounded finalizer
and rendering-frame-ring drains before revealing the destination. Selected owned
checks cover nested lifetime and failure cleanup. Transparent BC1 still has its
RGBA fallback; its measured extra payload is about 46 MiB, not gigabytes.

Code 15 passed ordinary Doc room -> exterior door -> movement -> F5 save -> cold
Continue. Room native allocated heap was about 738 MiB versus 1,080 MiB in code
13 (~342 MiB reduction). Counters overlap; do not add managed/native/GPU/PSS/swap
as separate physical-memory bins. One successful chain is not overall stability.

Outdoors remains about 7–10 FPS. Prior sequential probes at the same porch view:
main GPU about 84 ms at 960x540/four cascades, 79 with two, 77–80 with one, 61–64
with shadows off. At 640x360/shadows off, GPU fell to about 43 ms but active play
still measured 9–10 FPS; pause reached 21–22. At 960x540, range 200 m measured
about 80 ms GPU, 100 m about 70 and source 1,786 m about 83–85; active FPS stayed
about 8–10. No substantial active-FPS gain from these quality cuts was proven.
Both CPU updates and rendering need work. The world advanced during probes, so
these are not identical-state replays. Precise managed CPU owners remain open.

Diagnostic overlay keys: F6 shadows 4/2/1/off; F7 world scale .75/.5/1; F8 range
200/100/300/source metres. XR ignores these keys. --performance-log records
async per-viewport CPU/GPU timing in standalone OPENNV_RENDER_COST lines; code
17 fixed Android truncation caused by putting that data in resource snapshots.
All code-17 timing/resource lines parsed completely. No tracing services,
security settings or driver changes were needed for the CPU sample. Old log/
profile collectors stopped; any current collector must be stopped after testing.

## Verification and next owners

Latest gate: work/opennv-container-final-runtime-gate.log
Selected audit: work/opennv-container-final-owned.log
Export: work/opennv-container-export.log
Code-17 report: work/opennv-loot-freeze.private.log
Code-18 physical test: work/opennv-device-v18.private.log

1. Broaden physical looting coverage: quantity choices, paging, Take All and
   other containers. Basic selection/transfer/close is physically accepted.
2. Attribute active CPU update cost, remove repeated unchanged work, and profile
   mesh/material batching plus shader/terrain cost. Sustained 30 FPS is unachieved.
3. Test repeated doors, neighboring-cell commits and longer travel for resource
   growth. User traversal broadens evidence but is not complete travel support.
4. Preserve source gaps: 342 missing/unbound references in the prior exterior
   grid; source SPT resources/controllers/property-free LOD materials; script
   playGroup and other operands; Doc travel/procedure and six prior room refs.
   The current interior admitted 190/194 references; four LIGH declarations and
   a reference script's mySelf binding remain unsupported, independently of UI.
   Tag/trait physical acceptance and intermittent right-stick focus loss remain
   open. Earlier menu diagnostics reported two ObjectDB leaks.

Build toolchains and private continuation are in the task outputs file
`OpenNevada-Android-handoff.md`, outside the repository. Never overlap Android
export with desktop
builds/gates; they replace shared framework assets. Do not edit C# while formatting
runs. Keep frame recording off; inspect/delete temporary frames immediately.
Publish finished blocks through checked PRs, attach each PR, merge, and synchronize
clean main. Start the next block on a fresh codex/ branch.
