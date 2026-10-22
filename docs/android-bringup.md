# Android bring-up

The initial target is an ARM64 Mangmi Air X with Android 14, a 720p system
display override, and Turnip. No APK or device gameplay acceptance is established
by the current desktop checks.

## Build configuration

Use Godot 4.7.2 Mono and its matching export templates, the .NET SDK pinned in
`global.json`, and the Android SDK/JDK required by that Godot release. Configure
the Android SDK and Java SDK paths in Godot's editor settings. Keep local SDK
paths and signing credentials outside tracked source.

The preset is `Android Handheld Experimental`. It exports ARM64 only, uses a
separate package identity, and passes `-- --launcher` as its launch arguments.
The Android framework is net9.0, as required by Godot 4.7.2's non-Gradle C#
template; desktop retains net8.0. Godot's Mobile renderer is selected through a
platform override. Shader/material compatibility remains a device check.

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

These are the intended export steps, not a completed APK verification. Android
editor quick-deploy discovery is disabled because it triggers an EditorSettings
shutdown error in 4.7.2. The issue reproduces with an empty project; disabling
discovery leaves a clean editor exit and does not disable command-line export.

## Source integration

The original Android release supplies changes separately in a source archive.
Review startup, access to the selected installation, input, serialization, movie
playback, and rendering individually. That archive also contains earlier desktop
code; importing it wholesale would remove newer gameplay and persistence fixes.

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
