# According to James v0.7.1 — Windows controller fix

- Windows now accepts generic USB/HID joysticks as well as recognized gamepads. Previously, joystick devices were ignored.
- Settings > Controller setup shows the detected controller, allows saved USB button bindings, and offers vertical-axis inversion.
- SNES positions remain B Swing/Back, Y Dodge, A Interact/Confirm, X Map, Start Pause and Select Map.
- Supported Bluetooth controllers use the same gamepad or joystick input paths once connected in Windows.

Verified in the Windows player: simulated standard gamepad and generic USB joystick movement, combat, interaction, map, pause, menu confirmation, saved rebinding and disconnect handling. Presentation and text-fit checks passed. Physical USB/Bluetooth hardware has not been verified.

This is a Windows-only hotfix. Extract the ZIP before running. If your USB controller uses different button numbering, bind its buttons in Settings > Controller setup.
