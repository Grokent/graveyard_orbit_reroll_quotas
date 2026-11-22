#!/usr/bin/env bash
# Additively install BepInEx (if missing) and the quotas plugin.
set -euo pipefail
ROOT="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
GAME="${GAME_DIR:-$HOME/.local/share/Steam/steamapps/common/Graveyard Orbit}"
BEP_ZIP="$ROOT/tools/BepInEx_win_x64_5.4.23.6.zip"
DLL="$ROOT/src/GraveyardOrbit.Quotas/bin/Release/GraveyardOrbit.Quotas.dll"

[[ -d "$GAME" ]] || { echo "Game folder not found: $GAME" >&2; exit 1; }
[[ -f "$DLL" ]] || { echo "Plugin not built. Run ./build.sh first." >&2; exit 1; }
[[ -f "$BEP_ZIP" ]] || { echo "Missing $BEP_ZIP (run ./setup.sh)." >&2; exit 1; }

if [[ ! -d "$GAME/BepInEx" ]]; then
  echo "Installing BepInEx 5.4.23.6 (additive)"
  unzip -oq "$BEP_ZIP" -d "$GAME"
fi

DEST="$GAME/BepInEx/plugins/GraveyardOrbit.Quotas"
mkdir -p "$DEST"
cp -f "$DLL" "$DEST/"

echo "Deployed -> $DEST/GraveyardOrbit.Quotas.dll"
echo "Logs: $GAME/BepInEx/LogOutput.log"
echo
echo "If the game is running, restart it (the old DLL stays loaded otherwise)."
