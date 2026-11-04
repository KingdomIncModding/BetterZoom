# Better Zoom

Zoom further out and further in in Kingdom Inc., using the normal mouse wheel controls. Works in both the workshop and research tree.

Version: 1.0.0. Author: urakkaamyx.

- Three additional zoom-out steps show up to eight times the vanilla maximum width and height.
- Three additional zoom-in steps provide up to eight times the vanilla maximum magnification.
- Zoom stays anchored under the mouse. Keyboard panning, background selection, font scaling, UI scroll blocking and Shift+wheel workshop switching retain their native behavior.
- Each direction can be configured independently from zero to five additional steps.

## Requirements and installation

Built and tested against Kingdom Inc. Release v1.0.12, Unity 6000.3.27f2, and BepInEx 5.4.23.6 (Windows x64, Mono). Harmony is supplied by BepInEx. Other game builds are unverified.

1. Install BepInEx 5 for Windows x64 if needed.
2. Close Kingdom Inc.
3. Extract `KingdomInc-BetterZoom-1.0.0.zip` into the game folder containing `KingdomInc.exe`.
4. Confirm `BepInEx/plugins/BetterZoom/BetterZoom.dll` exists, then launch the game.
5. Scroll in the workshop or research tree to move past the old limits.

Manual installation is verified; mod-manager deployment is untested. BepInEx itself is not bundled.

## Configuration

After the first launch, close the game and edit `BepInEx/config/urakka.kingdominc.betterzoom.cfg`:

```ini
[Zoom]
ExtraZoomOutLevels = 3
ExtraZoomInLevels = 3
```

Each extra step doubles or halves magnification. Values are limited to 0 through 5. Set either direction to 0 to retain its normal limit. Restart after editing.

## Saves, updating and removal

Better Zoom adds no buildings, items, research or save fields. Extended zoom is temporary camera state; the game continues to store a normal zoom level. Reopening a view or restarting can reset an extended view to the nearest native zoom level.

To update, close the game and replace only `BepInEx/plugins/BetterZoom/BetterZoom.dll`. To remove, close the game and delete the `BetterZoom` plugin folder. You may also delete its configuration file. Saves remain usable without the plugin.

Other mods replacing the same camera zoom methods may conflict. At distant zoom, text and buildings naturally become small. Extreme views can increase visible geometry and rendering load. Keyboard movement retains the game's normal speed at the nearest native zoom level.

## Development and verification

Keep this source folder at `<KingdomInc>/_Mods/BetterZoom`; game and loader assemblies are local build references and are not redistributed.

```powershell
dotnet build .\BetterZoom.csproj -c Release
.\verify-isolated.ps1
.\verify-isolated.ps1 -WithExistingPlugins
.\Tools\package.ps1
.\install.ps1
```

For a first installation while the game is running, `install.ps1 -StageForNextLaunch` stages the DLL for the next restart. An existing installed DLL is never replaced while the game runs.

The isolated verification runtime loads a disposable kingdom, blocks save loading/writing and Steam achievements, exercises both cameras, wheel input, scale and cursor anchoring, zero-extension settings, keyboard panning, safe native save values and removal. It never loads a user kingdom. The companion run repeats these checks with the currently installed plugins. Reports in `Verification/` identify the exact tested DLL by SHA-256. The packager requires matching passing reports and verifies a clean extraction.

Source repository naming convention: `urakkaamyx/KingdomInc_BetterZoom`.

## Changelog

1.0.0: Initial release with extended workshop/research zoom, cursor anchoring and configurable limits.

## Credits and troubleshooting

Created by urakkaamyx. Uses BepInEx and Harmony; no base-game assemblies or artwork are bundled. Source licensing has not been selected.

For problems, include your game version, Better Zoom version, configuration and `BepInEx/LogOutput.log`.

Source repository: https://github.com/urakkaamyx/KingdomInc_BetterZoom
