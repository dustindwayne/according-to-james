# According to James v0.6.0 — Music, movement and dance party

Enemies now complete navigation waypoints instead of stopping just short and repeatedly targeting the same point. Chase and patrol routes refresh on arrival; unreachable destinations resolve to reachable grid cells.

- HUD captions, room-map labels and menu text sit inside the generated frames. James advice supports two centered lines; long dynamic labels fit without clipping.
- User-provided MP3 tracks play on the title screen, in rooms without living enemies, and during combat including bosses. Track changes crossfade; pausing resumes the current song at its position. James comedy cues, victory music and effects remain.
- New James YMCA, Macarena and robot rows add 24 full-body animation poses. James cycles these routines alongside his existing goofy dance, pacing and conversation gestures.

Verified: 30-floor campaign, 1,800 physical room layouts, doorway alignment, James interactions, waypoint regression in both directions, walking-frame progression, and 87 obstacle-room chase scenarios. Windows runtime checks verify imported music and menu/exploration/combat switching, all captured text fits, buttons work, and no runtime errors were logged. Independent agent reviewed all menu/HUD captures. MP3 decode and signal-level checks passed; manual listening is still useful for subjective loop quality.

Extract the Windows ZIP before running AccordingToJames.exe. Keep the Data folder and DLLs alongside it. Unity editor: 6000.3.12f1.
