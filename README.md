# Better Zoom

**Current release: 1.2.2 — for Kingdom Inc game version 1.0.14.**

[GitHub releases](https://github.com/KingdomIncModding/BetterZoom/releases) · [Nexus Mods](https://www.nexusmods.com/kingdominc/mods/14) · Version 1.2.2

Extend workshop and research-tree zoom with configurable limits, cursor anchoring, and Foundation settings integration for Kingdom Inc.

Zoom further out for panoramic factory overviews and closer in for precise pipe alignment using the standard mouse wheel.

## Features

- **Extended Zoom-Out:** Add up to 5 extra zoom-out steps (3 by default for 8x normal visible width and height; up to 32x at 5 steps).
- **Extended Zoom-In:** Add up to 5 extra zoom-in steps (3 by default for 8x closer zoom; up to 32x magnification at 5 steps).
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
| WorldFlowCamera.Update | Foundation hook (postfix) | Dynamically scales the tiled backdrop sprite to cover the expanded camera viewport. |
| WorldFlowCamera.ChangeZoomLevel | Foundation hook (prefix) | Replaces native zoom stepping with configurable zoom increments, extended bounds, and cursor-anchored camera positioning. |
| WorldFlowCamera.SetZoomLevel | Foundation hook (prefix and postfix) | Clamps native saved integer levels to 0–4, updates tracked floating zoom levels, and refreshes tiled backdrop coverage. |
| WorldFlowCamera.ZoomScale (getter) | Foundation hook (postfix) | Computes the camera zoom scale factor from the tracked floating zoom level across the extended range. |

## Removal

Close the game and delete `BepInEx/plugins/BetterZoom/`. Saved kingdoms remain fully compatible.

## Credits and Source

Created by **Urakka** (rockyamyx). [Source code on GitHub](https://github.com/KingdomIncModding/BetterZoom). Game artwork belongs to its respective owners.
