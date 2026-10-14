# According to James v0.5.0 — Depth, lighting and goofy James

A playable Unity 2D dungeon crawler with shaded stone architecture, generated beveled floor tiles, soft actor shadows and animated torch/shrine lighting.

- New 24-pose James sheet: left/right pacing, six goofy dance poses and six conversation gestures. James stops pacing when approached but keeps animating; collision and interaction follow his position.
- Stable character size and isolated sprite extraction prevent neighboring sprites leaking into animation frames.
- Boss anticipation/recoil, breathing, swirling clouds and rotating projectiles add motion to combat. Existing directional hero and enemy animation sheets remain in use.
- Left/right doors, collision passages and arrival positions align to the background openings across all three dungeons.
- Thirty floors, three bosses, Settings-only difficulty, mystery gifts, trapped chests, secrets and shrine checkpoints remain playable.

Validation: Unity Windows build; 30-floor campaign and 1,800-layout model checks; James pacing/proximity/interaction tests; side-door transition and arrival tests; rendered menu, dancing/pacing James, chest, and dungeon/boss checks. Audio still needs manual listening.

Extract the ZIP before running AccordingToJames.exe; keep the Data folder and DLLs alongside it. Unity: 6000.3.12f1. The prior 3D blockout is retained in source but is not the active renderer.
