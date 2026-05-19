# ShadowDash

Mobile-first Unity 6 2D top-down prototype foundation for an Archero-like project.

## Current State

- `Assets/Scenes/Boot.unity` is the startup scene.
- `BootSceneLoader` loads `MainGameplay` immediately.
- `Assets/Scenes/MainGameplay.unity` contains a placeholder player, an arena floor placeholder, and an orthographic follow camera.
- Player movement is Rigidbody2D-based top-down movement with WASD or Arrow keys for quick desktop testing.
- Platformer assumptions are intentionally absent: there is no jump, no ground check, and no gravity-based gameplay.
- Input is isolated in `PlayerKeyboardInput` and uses Unity Input System when enabled, with a legacy fallback guarded by compile symbols.

## Runtime Script Layout

- `Assets/Scripts/Core` - boot and project-level flow.
- `Assets/Scripts/Input` - input adapters.
- `Assets/Scripts/Player` - player-specific prototype logic.
- `Assets/Scripts/Camera` - camera behaviours.
- `Assets/Scripts/Enemies`, `Combat`, `UI`, `Utilities` - reserved for later systems.

## Not Implemented Yet

Enemies, combat, abilities, UI systems, save systems, monetization, and animations are intentionally out of scope for this foundation pass.
