# Current work

## Objective and boundaries

Make ordinary Fallout: New Vegas gameplay work on the owner's Mangmi Air X,
including Pip-Boy and loot/controller interactions, while keeping the runtime within memory.
Shared first-party C#/Godot gameplay and saves remain authoritative. No subagents.
Cutscenes remain deferred. No campaign, world, controller or retail parity claim.

Retail files are private/read-only. All 277 installation files previously matched
host SHA-256; do not transfer again. Desktop GPU/X11 Godot tests are prohibited:
an earlier fixture hung AMD/Xorg. Use CPU/headless checks and handheld rendering;
do not reset the desktop display/GPU or change drivers for comparison.

## Candidate

APK 0.1.20/code 21 is installed, SHA-256
`89fbcfd0c6c036d048ba02b3763e60bae59221dc0f8d2e062c6b7ccadb714156`.
The final full local runtime gate, selected owned choice audit, Android export
and v2/v3 signatures pass. The owner approved install/restart from the latest
save. PID 28761 loaded that checkpoint in CELL 106185. World-ready was logged at
22:13:51 local; first substantial draw telemetry followed at 22:14:30, and an
inspected/deleted frame confirmed visible saloon gameplay near Sunny/Cheyenne.
This view subsequently measured about 20–23 FPS. No Sunny input or tutorial
acceptance was performed on code 21; leave the current session running. Pre-install backup:
work/opennv-pre-v21-save.private.json, SHA-256
b5f684801cc84f1492ee8e740615abeffa9b7cf8f1349c759a7d19377009799b.
It matches the post-dialogue save; no save fields were changed. Physical Sunny
choice/tutorial progression remains pending. The prior budget stopping point and
external handoff were completed; a new source work block is now active on
`codex/sunny-dialogue-package`.

## Sunny meeting package work block

APK 0.1.21/code 22 is built, exported and v2/v3 signature checked but not
installed or physically tested. SHA-256 `938b31e0862ba3dde82f1aa7c31a19f4a5310c57d0f60bcd7d5da53725b678c2`;
file work/opennv-sunny-package.apk. Installed code 21 remains the running
candidate and its session was not disturbed. On this branch the source
dialogue-package owner accepts PLDT near-editor wait locations and PLD2
near-reference trigger volumes.
The target must enter the authored XPRM box (or its radius) before ordinary NAVM
travel begins, and PTDT supplies the activation distance. Package conditions now
route GetDeadCount through retained corpse and lethal-transition state for NPC and
creature packages. Synthetic PLDT/PLD2 parsing with duplicated authored PLD2
passes. The owned native Sunny audit passes package=105cc7, outsideNoMotion,
triggerGated, ownedNavm, targetRange, requestOnce and
completionAfterConversation; it does not execute dialogue results, audio or
player input. Cheyenne's 152afa/84 condition no longer faults, but its source
Accompany procedure remains an explicit unbound failure. The full runtime gate,
Android export and v2/v3 signature checks pass for this block.

Prior code-19 result:
Owned Pip-Boy/source-color/controller audits, Android export and v2/v3 signatures
pass. The final required full runtime gate and handheld display check pass.
The owner confirmed physical Pip-Boy page/tab/row navigation, A activation
and B close back to gameplay on code 19.
The previous code-18 container build passed its required gate; the owner confirmed
physical D-pad selection, A transfer and B close on that build.

Android 14/API 34, SM6115/Adreno 610, about 3.57 GiB physical RAM and 2.5 GiB LZ4
zram. Qualcomm Vulkan 1.1.295 is current; earlier runs used Turnip 1.0.328. No
driver change was made. ADB port 5038 currently owns USB, serial MQ65172511055656.

World 640x360 (owner-selected after the bar probe), UI/output 1280x720,
managed Android optimization, source uGrids=5.
Bloom/blur/double/adaptive exposure off; output conversion/tint/fades remain.
Outdoor draw cap is the owner's selected 200 metres, with matching fog ending
by 190 metres. Gameplay residency/collisions remain intact. Raw-source LRU is
128 MiB on Android / 512 MiB desktop and counts retained payloads only.

## Current bar dialogue and FPS report

The owner entered the saloon but the settler (104f03), Easy Pete (104c80) and
Sunny Smiles (104e85) did not converse. Code-19 PID 22188 first failed on QUST
13ec0d (vDialogueSloanGeneric), GetReputationThreshold 575. The fault then latched
the single conversation owner, blocked later requests and made CaptureCurrentState
reject saves/door autosaves as an active conversation. F5 at 21:08 confirmed the
same failure. Latest successful save was 20:59:03 in CELL 0e1aa7, stage VCG01:200.
Read-only backup: work/opennv-bar-report-save.private.json, SHA-256
f8169b566ac9e8f8ceeeac87f5a9a183ef999d2cf58b278358ebb791b567ca9d. The owner approved installing code 20 and restarting from this checkpoint.
The pre-install copy work/opennv-pre-v20-save.private.json has the same hash.
No save fields were edited. The previous build could not capture newer changes.
Code-20 PID 27444 cold Continue first drew the exterior at 21:23:27, about 140
seconds after Continue; steady play at this outdoor view was about 10 FPS.
Ordinary F5 saved successfully at 21:24:55, schema v22. Backup:
work/opennv-v20-loaded-save.private.json, SHA-256
4dd0a2d9da576a68381b28eda25ec7c831b03d3846638031603ca4d9fabc070a.
The owner confirmed Victor and Easy Pete conversations work; Sunny failed after
entering the saloon. At 21:31, binding Sunny attempted to clear the former
speaker's freed MeshInstance3D. Ordinary saving still succeeded at 21:31:24;
the owner is reloading to retry. Read-only post-dialogue backup:
work/opennv-v20-after-dialogues.private.json, SHA-256
b5f684801cc84f1492ee8e740615abeffa9b7cf8f1349c759a7d19377009799b.
A fresh speech-lifetime work block releases actor presentation references when
responses finish, fail or exit; binding another speaker also checks residency
before touching an old actor. Candidate code 21 passes the final full local gate and selected owned audit;
Android export and v2/v3 signatures pass. APK 0.1.20/code 21 SHA-256:
89fbcfd0c6c036d048ba02b3763e60bae59221dc0f8d2e062c6b7ccadb714156.
The owner approved installation/restart; code 21 is installed. Native freed/detached actor cleanup and synthetic
inventory query ownership/invalid run-on/missing-owner checks pass.
Logs: work/opennv-speech-choices-final-runtime-gate.log,
work/opennv-speech-lifetime-contract.log and work/opennv-speech-owned.log.
The owned audit reaches four Easy Pete choices and two Sunny choices without
executing their result scripts, audio or player input.
Reload allowed Sunny's greeting, confirmed by the owner and INFO 10b755 voice
telemetry. Building the next choices then failed at INFO 10b753, CTDA 47
GetItemCount on the player (run-on 1). The general dialogue condition owner now
routes item counts for speaker/listener/explicit references to retained inventories;
Form-list item queries remain unsupported and fail visibly. The owned selection audit now advances
past greetings into choice enumeration. Tutorial progression remains unaccepted.

Candidate code 20 adds player-owned fame/infamy, winning REPU scales and source
GMST thresholds, CTDA/raw/percentage/rank queries and reached script mutations.
Save v22 retains counters; v21 migrates from the previously unsupported initial
reputation lifetime. Random greeting pools use the saved shared script random
stream and authored Random End boundaries. New explicit conversation requests can
replace the failed transition owner while retaining world/results/said history;
a fault is no longer considered an active continuation for saving. GetDead and
GetDeadCount consume retained source-corpse and lethal-transition state. Resurrection
and respawn remain unsupported. Owned greeting selection succeeds for all three
reported speakers; this audit does not execute their result scripts or voices.
Synthetic contracts and selected owned greeting selection pass. The final full
local runtime gate passes. Export and signatures pass; physical dialogue acceptance is pending. See
[reputation and greeting ownership](reputation-and-greetings.md).

At the same bar view, code 19 measured about 13 FPS / 71 ms main GPU at 960x540.
A bounded 640x360 probe reached about 20 FPS / 40 ms GPU, still about 52–55 ms
process time. The UI stayed 1280x720. The original .75 scale was restored after
the probe, then the owner chose .5/640x360 for the next build. Default draw range
200 m/four cascades and post-effect choices remain. No 30 FPS result is claimed.

Source is now /home/jerr/open-nv, relocated at the owner's requested clean
stopping point. A fresh Debug restore/build at the new path passes. Toolchains/logs remain under
/home/jerr/Documents/Codex/2026-10-22/i/work. Owned game data stays in place.
Never overlap Android export with desktop builds or move a checkout during a build.

## Pip-Boy display and input fault

The owner reported a Pip-Boy freeze after container acceptance. PID 20900 remained
alive, rendering about 10–13 FPS with a blank CRT and no Pip-Boy exception.
Escape closed it, restored gameplay and saved the current interior at 19:49:40.
Pre-change backup: work/opennv-pre-pipboy-fix-save.private.json, SHA-256
6b01b969ebd1b37bc5409be98c0f677a366e4d16d03a511221b083d269766f7b.

A private code-19 diagnostic readback separated the source canvas from the arm:
only the new focus outline was visible; the menu art/text were black. The mesh
was visible, had its intended material and was picked correctly. The installation's
uPipboyColor is zero (unset); its named Pipboy RGB channels resolve to 1aff80ff.
The tile owner used the raw zero custom color instead of those channels. General
interface selection now accepts the independent custom setting, preserving a
nonzero override and resolving the named source channels when unset. Diagnostic
captures were inspected/deleted on host and device; the one-shot capture code
and launch flag were removed from the candidate.

The input adapter also omitted joypad forwarding into its source viewport.
Candidate controls: LB/RB STAT/ITEMS/DATA, left/right tabs, up/down rows, A use,
B close. Source tile targets have initial focus and visible highlight; refresh
retains it, paging reaches additional rows and stick hysteresis rejects repeats.
DATA initially selects quests, which remains usable in interiors; world-map exit
routing inside an interior and other unsupported pages still fail visibly.
XR pointer behavior and authoritative inventory/quest owners remain shared.
Owned paused-subviewport input and color audits and final Android export pass;
the final required full runtime gate passes. The owner confirmed every requested
physical Pip-Boy control, including B returning to gameplay.
Code-19 PID 22188 loaded the current interior checkpoint, then ordinary Tab opened
Pip-Boy at 19:58:10. A handheld frame confirmed visible source text/art, focus
outline and readable controller hints at 1280x720. It was inspected/deleted.
Menu rendering is still about 11 FPS; the display fix is not a performance result.
The owner answered "All work" for LB/RB pages, left/right tabs, up/down selection,
A activation and B close back to gameplay. Leave the current session running.
The private diagnostic APK is not the deliverable.

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
and 19:45:11. This was the pre-Pip-Boy interior session; preserve its newer checkpoints
in preference to older exterior saves.

Before final code-19 installation, ordinary F5 saved at 19:55:26.
Backup: work/opennv-pre-pipboy-release-save.private.json, SHA-256
7e31e06e096fa9d211e178de9aed3e7d4b9543fdfe8a7481e8f9cb289425c61e.

Post-container-acceptance backup: work/opennv-v18-controller-accepted-save.private.json,
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

Sunny package gate: work/opennv-sunny-package-runtime-gate.log (PASS)
Owned Sunny package audit: work/opennv-sunny-dialogue-owned.log (PASS)
Synthetic package locations: work/opennv-sunny-dialogue-synthetic.log (PASS)
Sunny package export: work/opennv-sunny-package-export.log (PASS)
Latest prior gate: work/opennv-bar-final-runtime-gate.log (PASS)
Bar selection audit: work/opennv-bar-final-owned.log
Relocated build: work/opennv-relocated-build.log (PASS)
Device code-20 log: work/opennv-device-v20.private.log
Synthetic detail: work/opennv-bar-synthetic.log / opennv-bar-save-contract.log
Prior Pip-Boy gate: work/opennv-pipboy-final-runtime-gate.log (PASS)
Selected audit: work/opennv-pipboy-final-owned.log
Source-color audit: work/opennv-pipboy-settings-owned.log
Handheld display log: work/opennv-device-v19.private.log
Export: work/opennv-pipboy-export.log
Prior container gate/audit: work/opennv-container-final-{runtime-gate,owned}.log
Code-17 report: work/opennv-loot-freeze.private.log
Code-18 physical test: work/opennv-device-v18.private.log

1. Finish the owner test of Sunny choices/tutorial handoff on installed code 21, then save and cold Continue. The source package now waits for the authored trigger volume, approaches the player and starts the shared conversation; the installed code-21 APK was built before this fix. Victor/Pete dialogue and Sunny greeting after reload are accepted on code 20. Then broaden looting coverage: quantity choices, paging, Take All and
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
   Cheyenne's 152afa GetDeadCount condition is now routed, but its Accompany
   procedure remains unbound. Tag/trait physical acceptance and intermittent
   right-stick focus loss remain open. Earlier menu diagnostics reported two
   ObjectDB leaks.

Build toolchains and private continuation are in the task outputs file
`OpenNevada-Android-handoff.md`, outside the repository. Never overlap Android
export with desktop
builds/gates; they replace shared framework assets. Do not edit C# while formatting
runs. Keep frame recording off; inspect/delete temporary frames immediately.
Publish finished blocks through checked PRs, attach each PR, merge, and synchronize
clean main. Start the next block on a fresh codex/ branch.
