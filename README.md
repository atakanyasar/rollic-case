# Color Block Jam
## Open and run

1. Open the project in Unity 2022.3.62f2 and let the packages import.
2. Open `Assets/Scenes/MainScene.unity` and enter Play Mode.
3. Press the level button, then Play. You can also run `GameplayScene` directly with the saved level selection.

The target resolution is **1080 × 1920 portrait**. I checked the layout at both **1080 × 1920** and **1080 × 2400 (20:9)** without layout issues. Progression loops back to level 1 after level 5.

## Make a level

Open **Tools → Color Block Jam → Level Editor** in Edit Mode.

1. Click New or open an existing level. Set its unique ID, dimensions and time limit.
2. Click Create New Block. Choose its color, movement axis and ice count, then draw a connected shape and click Create.
3. Drag the block from the staging area onto the board. Select a block to edit or delete it.
4. Click a border cell to add a door. Set its side, color and inclusive Start/End indices, then click Add/Apply Door.
5. Place or delete all staged blocks, then Save. Save As needs a new filename and an unused catalog ID.
6. Click Add Saved Level to Catalog, using the catalog assigned in GameplayScene, then Test Level to play it. The previous progression and coin balance are restored when you leave Play Mode.

Undo/Redo works on the working copy; Save writes it to the asset. Shuffle repositions blocks while preserving their shapes and restrictions, but does not guarantee a solvable result.

### Validation

I validate the layout before saving:

- Positive level ID, dimensions and time; at least one block and door; no unplaced staged blocks.
- Unique block IDs, connected non-empty shapes without duplicate cells, normalized offsets and non-negative ice counts.
- Blocks within bounds without overlap; valid door ranges without overlap on the same edge. Resizing cannot leave existing blocks or doors outside the board.
- Unique level IDs within the selected catalog, and no overwriting an existing file through Save As. Test Level also checks catalog membership and that the catalog matches GameplayScene.

I do not check matching doors for every block color, sufficient door width, compatible arrow directions, ice deadlocks, overall solvability or whether the time limit is sufficient. Shuffle does not verify solvability, and level ID uniqueness is not checked across assets outside the selected catalog.

## Architecture decisions

- I use ScriptableObject assets for level definitions and create a fresh runtime model on load. This keeps authored data separate from mutable gameplay state and makes restarting straightforward.
- I separated the model, input, view and flow code. Grid owns spatial rules, Level owns blocks and exit consequences, and LevelSession owns session state, timer decisions, coin rules and progression. LevelController connects these to Unity views and scene navigation, so the rules can be checked without running the scene.
- I reuse gameplay placement rules and mesh rendering in the editor to avoid maintaining a second implementation. Editor changes stay in a working copy until Save, with validation before writing assets.
- I separate continuous drag visuals from discrete grid movement. Blocks follow the pointer smoothly while the model checks each cell transition to prevent jumping over obstacles.
- I handle exits in two stages: I lock the block when its exit starts and remove it from the grid after the animation finishes. This keeps its cells occupied during the animation so other blocks cannot move through it.
- I share the settings panel through a prefab and use a variant for the gameplay-only Home button. This keeps common controls consistent while allowing screen-specific layouts.
- I keep color materials in a shared BlockPalette and create separate masked materials for the board. This keeps colors consistent without modifying source assets.
- I use cached PlayerPrefs integers for the small amount of persistent data: progression, coins and settings. I reuse input objects and only update timer text when the displayed second changes to avoid unnecessary frame work.

## Known problems and incomplete parts

- I checked the two required aspect ratios; other resolutions and aspect ratios may need further UI adjustments.
- I have not implemented obstacle cells inside the board.
- Exit particles currently cover the full door width. I still need to position and size the effect to match the exiting block's occupied span: a one-cell-wide block exiting through a two-cell-wide door should emit particles only from the cell it passes through.
- Different materials and textures limit batching. If profiling shows a need, I could organize compatible textures into atlases and share materials, or use vertex colors for block colors, to help reduce draw calls. I have not implemented these optimizations.
- Boosters, lives and actual audio/vibration effects are placeholders. Settings toggles save their state and provide visual feedback.
- There is no solvability validator. Shuffle can produce unsolvable layouts, especially when moving frozen blocks.

## LLM usage

I used **OpenAI Codex** for requirements analysis, refactoring, editor code and this README. I also used it for drag-and-drop, block mesh selection, rotation and pivot placement, board centering and camera fitting. I supplied and corrected the asset orientation and pivot information.

## Work time

I estimate **approximately 30-35 hours** in total. 
