# According to James v0.7.0 — Desktop settings and SNES controls

- Exit to Desktop on the Windows title screen and pause menu.
- Saved Windowed/Fullscreen setting, applied immediately.
- Check for updates in Windows Settings queries this public GitHub repository. Newer releases can be downloaded with progress; size and available SHA256 checksum are verified. Open Download Folder reveals the ZIP for manual installation. Existing shrine saves stay in the persistent user-data folder.
- Button labels have larger interior padding and thinner ornamental borders.
- Gamepads use SNES physical-button layout: bottom B swings, left Y dodges, right A interacts/confirms, top X opens the map, Start pauses, Select also opens the map. D-pad/left stick move and navigate menus; B goes back. Keyboard, mouse and Android touch remain supported.
- Android Settings offers a GitHub releases link; desktop-only exits and display controls are hidden.

Verified: Windows build, menu/text-fit/button actions, fullscreen toggle/restore, actual quit from both menus, live GitHub update check and verified Windows release download, campaign/layout/movement/policy checks, simulated gamepad gameplay and menu navigation. Physical controllers and phone gameplay remain to be tested.

Windows: extract ZIP before running. Android: ARM64, Android8.0+, installable signed APK. APK built in an isolated Unity6000.6.5f1 project; Windows project remains Unity6000.3.12f1.
