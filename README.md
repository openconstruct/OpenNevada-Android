# OpenNevada Android

An experimental Android bring-up of [OpenNV](https://github.com/nikamigaming-create/OpenNV),
a C# and Godot engine reimplementation that reads your own Fallout installation.

This fork targets Fallout: New Vegas on an ARM64 Android handheld. A complete
Android campaign, reproducible APK, and device performance have not yet been
verified. The inherited desktop runtime and its checks are the starting point.

## Source and release status

The original Android fork's September 28 Beta is described by its author as a
render, menu, video, and audio validation build, not a playable game. Its Android
changes were supplied separately in `OpenNV_Android-prep_14.zip`; they are not
present in the inherited public source branch. That archive also contains older
desktop files, so importing it wholesale would replace newer fixes.

This fork integrates reviewed Android changes into tracked source and keeps the
existing gameplay and persistence owners. See [Android bring-up](docs/android-bringup.md)
for the export configuration and [current work](docs/current-work.md) for verified
state and the next acceptance criteria.

## Desktop baseline

The inherited runtime uses Godot 4.7.2 Mono, the .NET SDK pinned by `global.json`,
and PowerShell 7. Node.js is used for retained desktop compatibility checks.

```powershell
dotnet build ./runtime/OpenNV.sln --configuration Release
./scripts/Test-GodotRuntime.ps1 -Godot '/path/to/Godot'
```

The existing Windows export preset alone does not produce an Android package.
Android export requirements will accompany the tested build.

## Game data

Supply your own New Vegas installation. Retail files are read-only inputs and
remain external to the application. No commercial assets, executables, saves,
or mod packages are included in this repository or its releases.

Thanks to the OpenNV authors and RedZeroGotcha's initial Android investigation.
