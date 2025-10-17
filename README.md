# FPS-Proto
Simple FPS Prototype

UNITY VERSION: UNITY 6000.2.7F2 (UNITY 6)
PACKAGES: NEW INPUT SYSTEM, NETCODE FOR ENTITIES, ENTITIES GRAPHICS, TEXTMESHPRO

Build running on Editor will act as Host-Client and other builds (apk, app, exe) act as client only.

Config inside project - find a file names GameConfig.cs inside Assets/Config and those static readonly values can be tweaked to check different scenarios.

Mobile input could not configured properly - (player controlling feel wise) due to time shortage but mac build is enjoyable.

SYSTEMS/FEATURES:
Multiplayer:
A) User will enter username/displayname (max 15 characters) to enter the game. User will act as Host-Client on Unity Editor and as a client on any other platform build.
B) Once the host connects and starts the game, other client players can also join.
C) Multiple clients can join and all will sync correctly.

Gameplay:
A) FPS Controls on Editor & Standalone builds (WASD/Arrows + Mouse). On-Screen joystick + Fire button on Android mobile build. Each player character is Owner Predicted.
B) Player's names will be shown on top of their characters as billboards (always face camera).
C) Players can shoot projectiles which despawn on player hit, below ground or timeout. These projectiles follow Predicted projectile path using custom gravity physics.
D) The projectiles use logic based server authoritative hit detection. Logic was used instead of actual physics to optimize for performance as Unity's Raycast checks are expensive (maybe fine for 100s of game objects but not for 1000s)
E) 100 target boxes move randomly (using random walker logic) - initially green, then changing color on each subsequent hit to first yellow then red and then despawn. Colors are changed using URPMaterialPropertyBaseColor component which is processed in parallel by the ECS framework. It uses GPU instancing to send a buffer of per-instance data to the shader which then applies the unique color to each instance of the mesh during the single, batched draw call.
F) Each player has 3 HP, die on 0, and respawn after 5s upon death at random point.
G) Disconnecting a client cleans up their character.
H) Shows ping and FPS counter as HUD.
