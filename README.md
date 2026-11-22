# Graveyard Orbit Quotas

Standalone BepInEx plugin that adds a **Reroll $50** option to every incomplete
work order on the QUOTAS board. It does not require the co-op or tweaks mods,
and all three can be installed together.

## ⚠️ Disclaimer

This project is **100% vibe coded** and **100% the product of AI use**. It is an
unofficial fan mod. I make **no claims of ownership** over Graveyard Orbit, its
code, assets, names, or anything derived from it. **All credit and all rights
belong to Anchorhead Games.** If you own the game, you are welcome to use this
mod for private play. No game code or game asset files are redistributed here.

## Install (Windows and Linux are the same)

You only need the release zip — no compiler and no separate Linux build.

1. Close Graveyard Orbit.
2. In Steam, right-click **Graveyard Orbit** → **Manage** → **Browse local files**.
   That opens the game folder (the one containing `GraveyardOrbit.exe`).
3. Download the release zip:
   **[GraveyardOrbit-RerollQuotas-0.1.0.zip](https://github.com/Grokent/graveyard_orbit_reroll_quotas/releases/latest/download/GraveyardOrbit-RerollQuotas-0.1.0.zip)**
4. Extract **everything** from the zip into the game folder that opened in step 2.
   It only adds files (`winhttp.dll`, `doorstop_config.ini`,
   `.doorstop_version`, `changelog.txt`, `BepInEx/`); nothing in the original
   game is replaced.
5. **Linux / Proton only:** add this launch option once (game → Properties →
   Launch Options). Windows needs no launch options:

   ```
   WINEDLLOVERRIDES="winhttp=n,b" %command%
   ```

6. Launch the game through Steam normally.

### Uninstall

Delete `BepInEx/plugins/GraveyardOrbit.Quotas/`. Remove `winhttp.dll`,
`doorstop_config.ini`, `.doorstop_version`, `changelog.txt` and `BepInEx/` only
if you do not use any other BepInEx mod.

## Use

Open the QUOTAS screen in the base.

- An **incomplete** work order shows **REROLL $50** where the CLAIM key normally
  sits.
- Once the target is met, the same slot becomes the normal **CLAIM** key.
- A paid card still shows **PAID** and cannot be rerolled.

Clicking REROLL immediately charges $50 and replaces that one card with a new
random work order of the same kind:

| Original card | Reroll pool |
|---|---|
| Family order (Laser, Cannon, Dynamo, Pulse, Drill, Drone) | A different weapon family, same category |
| General order (tiles, substrate) | Another general order |
| Cross-goal order (relics, fusing, ferrying, rooms, jumps) | Another cross-goal order |
| Purchase order (buy upgrades) | Another purchase order |

Additional rules:

- A **Drill** order will never reroll back into another Drill order; the same
  applies to every weapon family.
- Rerolling resets that card's progress to 0 and leaves the other nine cards
  untouched.
- Duplicate work orders are allowed, but the card always changes to a different
  work order from the one it replaced.
- The button is greyed out if you cannot afford the reroll, or if no other work
  order of that kind is available.
- The vanilla 10-card mix, set progression and rewards are unchanged.

## Configuration

`BepInEx/config/agentbox.graveyardorbit.quotas.cfg`

```ini
[Reroll]
# Money charged to reroll one incomplete work order. Set 0 for free rerolls.
Price = 50
```

## Compatibility

- Works alongside **Graveyard Orbit Co-op** and **Graveyard Orbit Tweaks**; each
  plugin lives in its own `BepInEx/plugins/` folder.
- No base game files are modified. The plugin patches the quota screen and the
  saved `quota_state` data at runtime.

## Build from source (developers)

Requires a legitimate copy of the game, plus `unzip`, `curl` and Python 3.

```bash
./setup.sh     # project-local .NET 9 SDK + BepInEx compile refs
./build.sh
./deploy.sh    # install into the local game folder
./package.sh   # release/GraveyardOrbit-RerollQuotas-0.1.0.zip
```

Set `GAME_DIR` if the game is not in the default Steam library.

## License and credits

The plugin's own code is MIT licensed (see `LICENSE`). The release zip bundles
BepInEx, which is LGPL-2.1 (see `THIRD_PARTY_NOTICES.md` and
`licenses/LGPL-2.1.txt`).

Graveyard Orbit and all related content are © **Anchorhead Games**. This is an
unofficial, fan-made, AI-generated project with no affiliation with or
endorsement from Anchorhead Games. All rights to the game and its assets remain
with them.
