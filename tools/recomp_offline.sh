#!/usr/bin/env bash
set -euo pipefail

# Offline recomp na PC. Wymaga .cue użytkownika (nie w repo).
# Użycie: ./tools/recomp_offline.sh /path/to/Crash\ Bandicoot.cue

CUE="${1:-}"
if [[ -z "$CUE" || ! -f "$CUE" ]]; then
  echo "Usage: $0 /path/to/game.cue"
  exit 1
fi

ROOT="$(cd "$(dirname "$0")/.." && pwd)"
cd "$ROOT"

CONFIG="${ROOT}/CrashBandicoot.json"
if [[ ! -f "$CONFIG" ]]; then
  # alternatywne lokalizacje z upstreamu
  for c in \
    "${ROOT}/config/CrashBandicoot.json" \
    "${ROOT}/CrashBandicoot.Launcher/CrashBandicoot.json"
  do
    if [[ -f "$c" ]]; then
      CONFIG="$c"
      break
    fi
  done
fi

if [[ ! -f "$CONFIG" ]]; then
  echo "Brak CrashBandicoot.json — skopiuj config recompilera z upstreamu Matteo842/CrashBandicoot-Launcher."
  exit 1
fi

echo "[recomp] Building Recompiler..."
dotnet build RecompOne.Recompiler -c Release

echo "[recomp] Running recompiler (config: $CONFIG)..."
dotnet run --project RecompOne.Recompiler -c Release --no-build -- "$CONFIG"

echo "[recomp] Done. generated/ / game/ są lokalne — NIE commituj disc ani wygenerowanego kodu gry."
