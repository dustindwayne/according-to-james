# According to James — Unity

Open this folder in Unity Hub using Unity 6.3 LTS (6000.3.12f1). Open `Assets/Scenes/Dungeon.unity`, then press Play. The game creates its camera, sprites, music and controls at startup.

Download the Windows ZIP from this repository's Releases, extract it, and run `AccordingToJames.exe`. Keep the executable beside its Data folder and UnityPlayer.dll. Local builds appear in `Builds/Windows`.

Controls: WASD/arrows to move, Space/J to swing, Shift/K to dodge, E/X to interact, M for map, Escape to pause. Set starting difficulty and music volume in Settings.

Progress saves only when you interact with a rest shrine in the starting room on floors 1, 5 and 9 of each dungeon (global floors 1, 5, 9, 11, 15, 19, 21, 25, 29). Pausing, opening the map, quitting and talking to James never save. Death lets you return to the last shrine. The new checkpoint format uses a separate slot from the earlier prototype's automatic saves.

Three dungeons contain ten floors each, with four to six connected rooms per floor. Difficulty scales from the Settings value by 1.08 per floor. James gives terrible advice and has his own comedic chiptune.

Each James encounter offers one unnamed mystery gift and one finger pull. Gifts can heal, boost sword damage temporarily, cause a short beer wobble, or produce a tiny stomach mishap. Pulling his finger produces a fart or another gift. All gift/chest/pitfall mishaps leave at least one health and remove at most 8% of maximum health. Item effects are revealed only after accepting; there are no named inventory entries.

James can reveal a loose-stone secret entrance. It reaches the floor's last room, sometimes through a shallow pitfall. The shortcut never bypasses the cleared-room stair requirement. Chests can contain mystery items or a pepper trap with a small reward.

The three bosses are Butterscotch Granny (candy and fart spray), Lunch Lady Ladle Fury (cafeteria trays and gravy), and Chef Grumble (sausage volleys and flour). Enemies patrol when distant, chase around obstacles using grid navigation, wind up before attacking, shoot from range, or flee when wounded. Red markers signal attacks.

The hero has 24 generated walking frames: six per direction. Bosses have directional walking cycles; regular enemies have walk, windup and strike frames. The richer art remains pixel-like, uses point filtering and transparency, and keeps movement frames on a shared baseline. Walking advances with actual movement and returns to idle when stopped. James retains the supplied likeness. Music and the finger-pull sound are synthesized locally.

To rebuild Windows, run `build.ps1` in PowerShell. To run the independent campaign checks, use `dotnet run --project Tests/ModelChecks.csproj`.

Validation: Unity compiles and builds Windows successfully. Independent campaign checks cover all thirty floors, connectivity, three boss patterns, checkpoint schedule, safe gifts, finger pulls, trapped chests, secret pitfalls, pathfinding, ranged attacks and retreat. The native runtime self-test initializes the sprite atlas, world rendering, music and movement. Run the built executable with `--self-test` for this check. Audible playback still needs a manual playthrough.

Source artwork was generated with the built-in image generator. Current atlas prompts and paths are recorded in `ART_PROMPTS.md`. This repository contains only the Unity project; Windows builds are distributed through Releases.
