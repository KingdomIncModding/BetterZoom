# Better Zoom

**Current release: 1.2.1 — for Kingdom Inc game version 1.0.14.**

[GitHub releases](https://github.com/KingdomIncModding/KingdomInc_BetterZoom/releases) · [Nexus Mods](https://www.nexusmods.com/kingdominc/mods/14) · Version 1.2.1

Extend workshop and research-tree zoom with configurable limits, cursor anchoring, and Foundation settings integration for Kingdom Inc.

Zoom further out for panoramic factory overviews and closer in for precise pipe alignment using the standard mouse wheel.

## Features

- **Extended Zoom-Out:** Add up to 5 extra zoom-out steps, displaying up to eight times the normal visible width and height.
- **Extended Zoom-In:** Add up to 5 extra zoom-in steps for extreme close-up inspection.
- **Cursor-Anchored Zooming:** Zooming naturally centers and tracks under your cursor position.
- **Seamless Panoramic Backgrounds:** Research tree and workshop backgrounds dynamically expand to cover wide-angle viewing without blank margins.
- **Foundation Settings Integration:** Configure zoom levels and step scaling directly in-game via the Foundation mod settings menu.
- **Per-Kingdom Tech Unlock:** Research Better Zoom under Tech Tree > Mods for 100 gold to activate extended zoom levels for each kingdom.

## Usage and Controls

1. Open **Tech Tree > Mods** and research **Better Zoom** (100 gold) to unlock extended zoom.
2. Use the standard **Mouse Scroll Wheel** in the workshop and research views to zoom beyond vanilla limits.
3. Configure your preferred extra zoom steps (0–5 in each direction) under **Options > Mods > Better Zoom** or in `BepInEx/config/urakka.kingdominc.betterzoom.cfg`.

## Requirements

- Kingdom Inc (Windows x64 Mono)
- [BepInEx 5](https://github.com/BepInEx/BepInEx/releases/tag/v5.4.23.6) (5.4.23.6 or newer)
- [Foundation](https://www.nexusmods.com/kingdominc/mods/18) (0.14.0 or newer)

## Installation

1. Install BepInEx 5 for Kingdom Inc.
2. Install Foundation (0.14.0 or newer).
3. Extract the release ZIP into your Kingdom Inc game directory.
4. Verify the file exists at `BepInEx/plugins/BetterZoom/BetterZoom.dll`.
5. Launch the game, open Tech Tree > Mods, and research Better Zoom!

## Compatibility

| Class.Method | Hook Type | Impact |
|---|---|---|
| WorldFlowCamera.Update | Foundation hook (postfix) | Dynamically adjusts camera view bounds, grid sizing, and background extents for extended zoom levels. |
| WorldFlowCamera.ChangeZoomLevel | Foundation hook (prefix) | Replaces the native zoom clamping logic to allow configured extra zoom-in and zoom-out steps while preserving cursor anchoring. |
| WorldFlowCamera.SetZoomLevel | Foundation hook (prefix) | Applies configured extended zoom level bounds and orthographic camera size scaling. |
| WorldFlowCamera.ZoomScale (getter) | Foundation hook (postfix) | Returns the accurate scale factor calculated across the extended zoom level range. |

## Removal

Close the game and delete `BepInEx/plugins/BetterZoom/`. Saved kingdoms remain fully compatible.

## Credits and Source

Created by **Urakka** (rockyamyx). [Source code on GitHub](https://github.com/KingdomIncModding/KingdomInc_BetterZoom). Game artwork belongs to its respective owners.
