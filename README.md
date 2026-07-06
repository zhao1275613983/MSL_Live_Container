# MSL Live Container

Live container framework for Stoneshard ModShardLauncher mods.

This library mod adds reusable live-container parent objects and helper scripts.
Concrete container mods can inherit from these objects so container contents stay
instanced while still using the game's save/load flow.

## Build

The project targets `net6.0-windows` and references ModShardLauncher assemblies
from `J:\msl`.

```powershell
dotnet build .\LiveContainers.csproj -c Release
```

To pack an `.sml`, use the workspace packer from the development workspace:

```powershell
.\tools\build-msl-mod.ps1 -Project .\outputs\LiveContainersMSL\LiveContainers.csproj -NoInstall
```
