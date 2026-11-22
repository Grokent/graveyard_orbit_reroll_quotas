# AGENTS.md — Graveyard Orbit Quotas

## Purpose

Standalone BepInEx/Harmony plugin for **Graveyard Orbit** that adds a paid
**REROLL $50** action to incomplete work orders on the QUOTAS board. Separate
from the co-op and tweaks mods; all can be installed together.

- **Repo:** https://github.com/Grokent/graveyard_orbit_reroll_quotas (branch `main`)
- **License:** MIT (`LICENSE`); BepInEx LGPL-2.1 notices in `THIRD_PARTY_NOTICES.md`

## Environment

- Linux (CachyOS); game via **Steam/Proton**, AppID **4654120**, Unity
  **6000.5.0f1**, Mono; BepInEx 5.4.23.6 additive.
- Proton launch option: `WINEDLLOVERRIDES="winhttp=n,b" %command%`.
- Game path: `~/.local/share/Steam/steamapps/common/Graveyard Orbit`.
- Decompiled reference code lives in the co-op project:
  `~/Projects/agent/Graveyard Orbit MP/decompiled/` (do not redistribute).
- Relevant game types: `GraveyardOrbit.QuotaState`, `QuotaDef`, `QuotaGoal`,
  `QuotaCatalog`, `GraveyardOrbit.UI.QuotasScreen`, `GraveyardOrbit.UI.SlabFrame`.

## How it works

- `QuotasScreen.MakeCard(QuotaState q, JObject inst, int index)` is patched with
  a postfix. For an active, unclaimed, incomplete card the vanilla claim key
  (`.q-slabkey` wrapper + `Button`) is taken over:
  - wrapper visibility forced visible (vanilla hides it until complete),
  - button text becomes `REROLL $N`,
  - button enabled only when affordable and a replacement exists,
  - frame/button recoloured orange to distinguish it from CLAIM.
- When the target is met, the patch does nothing: the vanilla CLAIM key stays in
  the exact same slot. Paid cards keep PAID. This is required behaviour.
- `QuotaReroll` replaces the `QuotaState.Active` `JObject` fields in place:
  `id = newId`, `progress = 0`, `claimed = false`, then `QuotaState.Save()`.
- Replacement pool mirrors the vanilla category split:
  - `IsFamily` → other weapon families (excludes the current family),
  - `IsGeneral` → other general orders,
  - `IsPurchase` → other purchase orders,
  - `IsCross` → other cross-goal orders,
  - non-substrate material orders are never part of the normal draw and are not
    used as replacements.
- Locks respected: `GameData.DemoLocksDrones || QuotaState.SetIndex <= 2` plus
  `QuotaCatalog.NeedsDrones`, and `GameData.IsDemoLockedBranch(family)`.
- Duplicates on other active cards are allowed by design (user choice). The one
  card being rerolled always changes to a different `id`.

## Price / config

`BepInEx/config/agentbox.graveyardorbit.quotas.cfg`

```ini
[Reroll]
Price = 50
```

`Price = 0` is allowed (free). The key is disabled when the player cannot
afford it; money never goes negative. `GameState.Money` setter tracks spending,
and `QuotasScreen.DevRefresh()` rebuilds the cards + cash label after a reroll.

## Build / deploy / package

```bash
./setup.sh     # portable .NET 9 SDK + BepInEx compile refs (project-local)
./build.sh
./deploy.sh    # -> BepInEx/plugins/GraveyardOrbit.Quotas/
./package.sh   # release/GraveyardOrbit-RerollQuotas-0.1.0.zip (gitignored build output)
```

- `build.sh` prefers `tools/dotnet`, then the Tweaks/co-op project's portable
  SDK, then `dotnet` on PATH.
- No dependency on the co-op or tweaks plugins; no shared state.
- Each release zip is attached to GitHub Releases and also mirrored in
  `dist/` (with `dist/SHA256SUMS.txt`) so it is visible in the repository file
  listing. Keep the two copies identical.

## Gotchas

- `QuotasScreen.MakeCard` is private; patch by string name. If a game update
  renames it or the `.q-slabkey` class, the patch logs a warning and the mod is
  inert (game still works).
- `MakeCard` is also used for the claim animation's oversized card. Claimed and
  complete cards are skipped, so the big animation card never gets a reroll
  button.
- The vanilla claim key's wrapper is explicitly `visibility = Hidden` for
  incomplete cards and its `Button` is disabled; both must be overridden.
- Rebuild the screen after a reroll with the public `QuotasScreen.DevRefresh()`
  (it calls the private `Rebuild` while the screen is visible).
- Do not call `QuotaState.DevRedraw` or `DevMakeClaimable`; those are debug
  actions and replace/mutate the whole set.
- Reroll changes only `quota_state.active`; never touch `set`, claimed cards or
  the catalog.
- Never modify base game files. All changes are runtime patches.
- Restart testing by killing the Steam launch client, not `pgrep -f
  GraveyardOrbit` (kills the shell).
- Logs: `BepInEx/LogOutput.log`; expect
  `[Graveyard Orbit Quotas] Quotas loaded: work-order reroll costs $50.`

## Verification checklist

1. Open QUOTAS: every incomplete card shows an orange `REROLL $50` in the
   lower-right key slot; complete cards show CLAIM; paid cards show PAID.
2. Click reroll with >= $50: money drops by $50, cash label updates, card text
   changes, progress resets to 0, the other nine cards keep their progress.
3. A Drill order never rerolls to another Drill order; general/cross/purchase
   cards keep their category.
4. With < $50 the reroll key is visible but greyed/disabled and cannot be
   clicked.
5. Reopen QUOTAS and restart the game: the rerolled card persists (saved in
   `quota_state.active`).

## Working conventions

- Keep patches defensive (try/catch, no exceptions into gameplay).
- Bump `QuotasPlugin.PluginVersion` when shipping.
- Commit/push to the repo when asked; keep `release/`, `tools/`, `bin/obj`
  gitignored. `dist/` is deliberately tracked.
- README/INSTALL must carry the AI-use disclaimer (100% vibe coded, no
  ownership claims, rights to Anchorhead Games).
