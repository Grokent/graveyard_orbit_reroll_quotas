#!/usr/bin/env bash
# Build the quotas plugin with a local .NET SDK.
set -euo pipefail
ROOT="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"

DOTNET="$ROOT/tools/dotnet"
if [[ ! -x "$DOTNET/dotnet" ]]; then
  for SHARED in \
    "$HOME/Projects/agent/Graveyard Orbit Tweaks/tools/dotnet" \
    "$HOME/Projects/agent/Graveyard Orbit MP/tools/dotnet"; do
    if [[ -x "$SHARED/dotnet" ]]; then
      DOTNET="$SHARED"
      break
    fi
  done
fi
if [[ ! -x "$DOTNET/dotnet" ]]; then
  DOTNET="$(command -v dotnet || true)"
fi

[[ -n "$DOTNET" && -x "$DOTNET/dotnet" ]] || {
  echo "No dotnet SDK found (expected tools/dotnet, a sibling project's SDK, or dotnet on PATH)." >&2
  exit 1
}

export DOTNET_ROOT="$DOTNET"
export PATH="$DOTNET:$PATH"
export DOTNET_CLI_TELEMETRY_OPTOUT=1
export DOTNET_NOLOGO=1

dotnet build "$ROOT/src/GraveyardOrbit.Quotas/GraveyardOrbit.Quotas.csproj" -c Release "$@"
