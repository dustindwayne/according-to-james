# Current animation assets

## Assets/Resources/Art/TitleScreen.png

Generated with the built-in image generator using the user's original According to James poster as a visual reference.

Create a landscape 16:10 Unity game title screen background adapted from this original According to James poster. Preserve James's recognizable middle aged smiling face, salt-and-pepper beard, tan trucker cap, navy shirt, denim overalls, brown boots, pointing gesture and dangerous purple potion, dark blue torchlit stone dungeon, treasure, skeletons, goblin and mischievous comedy. Rich polished pixel-like illustrated art, crisp pixel edges beyond SNES. Rearrange composition: LEFT 55 percent contains the exact title 'ACCORDING TO JAMES' in monumental cracked gold stone lettering near upper left, with James seated on a treasure chest below it, torchlight and goblin at his boots. RIGHT 40 percent is a quiet dark recessed stone alcove, simple dark navy bricks, reserved for actual interactive menu buttons added by Unity. No baked-in menu buttons, no speech bubbles, no extra text, no watermarks. Cinematic amber torches against blue dungeon, dark readable right area. Keep all main content comfortably inside canvas borders.

## Assets/Resources/Art/James.png

Generated with the built-in image generator using the user's James photograph as the likeness reference. A single full-body sprite replaces the old grid crop in both the dungeon and conversation card.

Generate ONE SINGLE full-body NPC sprite of James, from this man's supplied likeness. Modern richly shaded pixel-like game art, crisp pixel edges, expressive face. Preserve middle-aged man salt-and-pepper full beard, smiling skeptical mischievous blue-eyed expression, gray and tan trucker cap, navy star T-shirt, blue denim bib overalls with brass buckles, sturdy brown boots. Full body standing facing viewer in slight top-down three-quarter dungeon RPG view, one hand pointing with terrible friendly confidence. Transparent alpha background. IMPORTANT ENTIRE HAT including crown fully visible, full head never clipped, BOTH boots fully visible. Character occupies central 75 percent height, generous completely empty transparent padding 12 percent ABOVE hat, 12 percent BELOW boots and 15 percent on sides. One isolated character only, NO SPRITE GRID, NO extra heads, no shadows, no text, no logo. This asset must fit inside a Unity portrait and world sprite without head cropping. Match face to reference but stylize as detailed pixel-art NPC.

Generated using the built-in image generator. These files belong to this Unity project. No CLI/API fallback was used.

## Assets/Resources/Art/HeroWalk.png

Use case: stylized-concept. Production game sprite atlas for According to James, top-down dungeon crawler. Rich detailed modern pixel art, pixel-like crisp edges, beyond SNES color depth but no blurry painterly shading. Regular brown-haired dude gray hoodie blue jeans brown sneakers holding short sword. EXACT grid SIX columns FOUR rows, evenly sized cells, transparent background actual alpha. Rows face DOWN, LEFT, RIGHT, UP respectively. Each row six distinct coherent walking frames, alternating legs, planted foot, arm counter swing, consistent face costume sword, same ground baseline and scale in each cell, no overlap no labels no borders. Top-down three-quarter view. Entire body fits cell with generous margin. This is a practical animation spritesheet.

## Assets/Resources/Art/Bosses.png

Production game atlas, modern richly shaded PIXEL ART with crisp visible pixel edges, transparent alpha background no ground shadows no text. EXACT grid FOUR columns TWELVE rows, all evenly spaced cells, top-down three-quarter full-body sprite animation. Characters: rows1-4 Butterscotch Granny: tiny old woman, lavender cardigan, handbag candy, mischievous face. rows5-8 Lunch Lady Ladle Fury: stout furious cafeteria lunch lady, hairnet mint uniform white apron metal tray and ladle. rows9-12 Chef Grumble: grumpy elderly male chef, gray mustache white chef hat red apron wielding sausage links. Within EACH character's four rows, row directions DOWN LEFT RIGHT UP in that order. FOUR distinct WALK-cycle frames across each row, alternating feet, stable head scale costume and silhouette. Consistent sprite baseline within cells generous transparent gaps, feet fully visible, no overlap. Beyond SNES detailed expressive pixel-like sprites, never photorealistic. 4 by12 practical evenly spaced atlas.

## Assets/Resources/Art/Enemies.png

Use case: stylized-concept. Production enemy animation atlas for modern top-down dungeon crawler According to James. Rich detailed expressive PIXEL-LIKE art, crisp visible pixels and controlled detailed shading beyond SNES. EXACT grid SIX columns THREE rows, transparent alpha background no shadows no labels no borders. Each row a consistent silly monster: row1 angry green dungeon goblin carrying a wooden spoon, row2 haunted cafeteria pudding cup with little legs and spooky eyes, row3 cowardly animated kitchen dust mop with little eyes and legs. Six frames per row: columns1-4 four coherent walk cycle frames facing down-right, column5 conspicuous attack windup pose (raises weapon or swells), column6 attack pose (swings or spits). Same scale baseline costume per row, evenly spaced equal cells, generous gaps, entirebody contained. Fullbody topdown threequarterview, monsters funny readable at80pixelsheight.

## v0.4 generated production sheets

Sources live in Assets/ArtSources. AtlasCompiler segments alpha silhouettes, aligns frames to shared baselines, and imports rigid Unity sprite rectangles into Resources/Art/Compiled. There are 144 imported sprites, including eight trimmed James portrait views. Actors retain padding around hats and boots.

Generated sheets: Hero (4 directions x 8 poses), JamesAnimated (8 idle/interaction poses), BossGranny, BossLunchLady and BossChef (4 directions x 4 walking poses each), Props (16 chest/door/shrine/stair/obstacle states), Menus (6 stone/parchment/wood panels), Effects (8 projectiles/attack/hazard sprites), and three clear-floor dungeon backgrounds. Enemies uses the existing generated 18-pose sheet.

The built-in image generator produced pixel-like artwork beyond SNES resolution. Prompts specified orthographic top-down actors, consistent scale and silhouettes, transparent evenly separated subjects, no lettering on sprites, no clipped hats or boots, fixed row/column pose order, and clear dungeon floors without baked-in interactive props. James used the supplied photograph for cap, beard, navy shirt and denim overalls. Menu art specified dark stone skull/rivet panels, parchment dialogue and oak/copper buttons without baked-in text. Runtime labels and interactions are Unity UI.

## v0.5 animation and floor assets

JamesGoofy.png: generated from JamesAnimated.png as likeness reference. Production transparent 4x6 sheet: six left walking poses, six right walking poses, six goofy dance poses, six conversation gestures. Prompt required full hats/hands/boots inside padded cells, consistent body scale/baseline, navy star shirt, denim overalls, tan trucker cap, salt-and-pepper beard, pixel-like dimensional shading, real alternating limb motion, no text and no baked-in ground shadows.

FloorDepth.png: generated transparent 3x4 atlas. Four blue-gray cellar slab variants, four warm brown-gray pantry variants, four violet-gray kitchen variants. Prompt required square orthographic floor slabs with upper-left lighting, shaded bevels, recessed mortar, chisel marks, clean/cracked/scuffed/moss variants, quiet readable interiors, transparent separated subjects, no perspective diamonds, walls, furniture or lettering.

Compiled outputs are Resources/Art/Compiled/JamesGoofy.png and FloorDepth.png. The compiler masks each connected subject, shares scale across all James poses, pads animation cells and adds safely trimmed UI portraits. Runtime soft shadows and light pools are generated Unity effects; actor lighting uses nearest torch distance. Floor variants use a deterministic coordinate hash.
