# Current work

## Objective and boundaries

Make the experimental ARM64 Android runtime fit the owner's Mangmi Air X while
preserving owned content admission and authored texture quality. Keep the shared
C#/Godot gameplay/save path. No subagents. Cutscenes stay deferred. Full campaign,
ordinary travel/combat chains and physical controller acceptance remain open.
A rendered checkpoint is not parity or whole-world support.

Retail inputs are private/read-only. All 277 installation files already matched
the host by SHA-256; do not transfer again. Desktop GPU/X11 tests are prohibited:
an earlier Vulkan fixture caused AMD/Xorg timeouts and a desktop hang. Use
CPU/headless checks and handheld rendering; do not reset the desktop display/GPU.

## Candidate and settings

APK 0.1.15/code 16 is installed, SHA-256
`2a832ef237c3b749ed8f83577abf011f9dc589562da535b85d39f4a8da7e267e`.
It adds render timings and reversible quality probes to merged PR #15/main
dffaa93. The required local runtime gate and selected owned sun/weather check
pass. This diagnostic build does not establish an FPS improvement.

Android 14/API 34, SM6115/Adreno 610, about 3.57 GiB physical memory and 2.5 GiB
LZ4 zram. Current driver: Qualcomm Vulkan 1.1.295. Older runs used Turnip 1.0.328;
no driver changes were made by this work. ADB port 5038 currently owns USB.

Managed Android optimization is enabled. Owned uGridsToLoad=5 selects 25 cells
and 2,337 references. Internal 3D is 960x540; UI/output is 1280x720. Bloom, blur,
double vision and exposure adaptation are off. Output conversion/tint/fades and
the top-right FPS overlay remain. Loading FPS/counters can be stale while work
blocks drawing. The raw-source LRU is 128 MiB on Android, 512 MiB on desktop;
it does not account for readers' arrays, decoded objects or graphics resources.

## Resource ownership changes

Immutable 2D DDS/cubemaps reuse by winning identity, actual byte hash, normal
validation policy and resource kind. Native-ID indexes do not retain textures.
Scoped Variants and mesh/morph/cubemap containers promptly release temporary
references. Cubemap readback verified all six faces and authored mips on-device.
Transparent BC1 still uses the original RGBA fallback: compressed-alpha support
needs a native Godot wrapper change. Its measured outdoor expansion was about
46 MiB beyond compressed payloads; no gigabyte saving is implied by that lane.

Code 15 also shares exact-source immutable Shader programs through a non-owning
native-ID index. NIF, LAND/LOD, sky and FaceGen keep separate mutable materials
and uniforms. Permanent shader pins and redundant strong FaceGen DDS caching
are removed. The ordinary SLS builder disposes its temporary StandardMaterial
input container and copied environment pass after the final material owns inputs.

After a door's authoritative commit/autosave, the destination remains hidden
while the old room and prototypes outside its model set retire. Three bounded
collection/finalizer passes drain mesh -> material -> shader/texture ownership;
hidden frames drain the rendering device frame ring before reveal. Gameplay
stays paused during this boundary. Save/retirement failures release the gate.
The selected owned audit proves retained destination geometry/contact ownership,
nested source-resource release and failure cleanup.

Upload/program counters are cumulative work, not resident memory. A partial
allocation profile sampled about 242 MiB of new outstanding allocations during
six seconds of exterior preparation: about 194 MiB through Godot, 47 MiB through
Vulkan and 1 MiB through Mono. Stripped C++ symbols prevent precise Godot-owner
attribution. The failed startup profile was discarded. Traced, traced_probes
and heapprofd were restored to stopped with persist.traced.enable=0. Stale log
collectors from previous failed processes were stopped.

## Physical result

Code 13 completed exterior preparation/commit then hit LOW_MEMORY before drawing.
Code 14 died during preparation, before its new retirement boundary could execute;
a cold exterior attempt also hit LOW_MEMORY. Those runs do not isolate a code-14
regression cause. Both authentic interior and exterior saves remain backed up.

Code 15 PID 13460 on the same driver/source save:

- Room: about 28–30 FPS stationary. Allocated native heap about 738 MiB versus
  about 1,080 MiB in code 13; one-run reduction about 342 MiB. Tracked video
  memory is about 172 MiB. These counters overlap and are not separate RAM bins.
- Ordinary door activation 18:06:59 local. Models ready 18:07:39, terrain ready
  18:07:42, authentic exterior autosave 18:07:44. Source retirement finished
  18:07:55: 406 -> 213 prototypes and about 604 -> 463 MiB managed live data.
- First exterior draw telemetry 18:08:26, about 87 seconds after activation.
  A temporary frame confirmed Goodsprings; it was inspected and deleted.
- The exterior stayed alive for roughly three minutes of visible rendering
  before the planned cold restart. It measured about 7–10 FPS and 2,166–2,214
  draw calls. One snapshot had native heap allocated about 1.32 GiB, RSS about
  1.3–1.4 GiB and process swap roughly 0.7–1.0 GiB. No sustained-30 claim.
- Ordinary movement changed the saved position by about 0.96 m. F5 created a
  valid manual slot and updated Continue. Predictive grid preparation ran; this
  does not establish a completed neighboring-cell commit or broad travel.

Cold Continue from that ordinary moved exterior save passed in fresh PID 14483:
Continue at 18:12:53, compositor/first exterior draw at 18:15:06–07 (about 133
seconds), later about 6–9 FPS. A temporary frame confirmed the restored view and
was deleted. The device was left paused in Goodsprings at 18:16:00. Paused FPS
was about 12; this coarse observation does not isolate CPU/GPU cost. The primary
save, manual slot and private backup match by SHA-256. The original interior,
code-12/code-13 exterior and code-15 door/manual checkpoints are also preserved.
Do not edit save fields or inject player/world state to manufacture success.

## Outdoor frame-cost experiment

Code 16 PID 16096 restored the same authentic exterior save, ordinary Continue
at 18:31:43, source world ready 18:32:59, visible draw telemetry by 18:34:02.
GPU time below is the main viewport; its first-person viewport adds about 2 ms.
CPU render submission/setup was generally about 10–17 ms with shadows and
about 9–11 ms without. Gameplay/process counters include other work and stalls;
do not add them to GPU time as disjoint frame slices.

Same camera, advancing live world, sequential bounded probes:

| World resolution | Sun shadow cascades | Active FPS | Main viewport GPU |
| --- | --- | --- | --- |
| 960x540 | 4, warm restored baseline | 9–10 | about 84 ms |
| 960x540 | 2 | 7–10 | about 79 ms |
| 960x540 | 1 | 8–9 | about 77–80 ms, occasional 91 ms |
| 960x540 | off | 7–10 | about 61–64 ms, occasional 73 ms |
| 640x360 | off | 7–10 | about 43–44 ms |

At 640x360/shadows off, pause reached about 21–22 FPS while GPU time stayed
about 43 ms; active play remained about 9–10. Both rendering and game updates
need work. These short, sequential observations are not a controlled identical
world-state replay or a sustained performance claim. There is no evidence that
the visual cuts alone deliver a worthwhile active-FPS gain. Restored 960x540 and
four cascades, then paused outdoors. UI remained 1280x720; save hash unchanged.

Only with the diagnostic FPS overlay, F6 cycles sun shadows 4/2/1/off and F7
cycles world scale .75/.5/1. XR ignores these diagnostic keys. The optional
`--sun-shadows 0|1|2|4` flag selects launch quality; absent it, four is unchanged.
`--performance-log` alone enables per-viewport timestamp measurements and logs
them asynchronously every five seconds, including while paused. No live harness,
frame recording, desktop GPU test, driver change or global OS tweak was needed.

A private 15-second simpleperf sample captured 2,069 CPU-cycle samples with none
lost. About 44% was unresolved, 25% through Godot, 8% Mono, 8% kernel and 5%
Vulkan; that does not identify a particular managed update owner. No tracing
services or security settings were changed. Collector/profile processes ended.

## Verification and next owners

Code 15 passes the full required Release/Debug, formatting/analyzer, contract,
launcher and native headless Godot gate, plus selected owned DDS, Shader,
cell-handoff and NPC assembly audits (all 13 Doc parts retained). Android export
and v2/v3 signatures pass. No desktop GPU test or frame recording was run.

1. Attribute active CPU update cost to individual owners, then remove repeated
   unchanged work. Profile mesh/material batching and shader/terrain cost for the
   independent GPU limit. Further resolution/shadow cuts alone did not solve FPS.
   Keep texture quality and owned admission intact; 30 FPS remains unachieved.
2. Exercise repeated doors, neighboring-cell commits and longer travel to bound
   warm-resource growth. One successful exit does not establish overall stability.
3. Preserve visible source gaps: 342 missing/unbound exterior references in the
   selected grid, unsupported SPT resources/controllers, property-free LOD
   material ownership, reference scripts/playGroup, Doc's travel/procedure and
   six earlier missing interior refs. Do not hide them or claim campaign parity.
4. Physical tag/trait controller acceptance and the transient right-stick focus
   loss remain open. Earlier menu diagnostics reported two ObjectDB leaks.

First-frame compositor stalls remain long and unisolated; older Continue runs
took about 100 seconds. The owner completed Doc's opening through VCG01 stage 200.
