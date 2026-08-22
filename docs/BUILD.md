# Offline recompilation (PC only)

1. Zainstaluj .NET 9 lub 10 SDK.
2. Połóż legalny `.cue`+`.bin` **poza** repo.
3. Upewnij się, że w forku jest `CrashBandicoot.json` (z upstreamu).
4. Uruchom:
   ```bash
   chmod +x tools/recomp_offline.sh
   ./tools/recomp_offline.sh "/path/to/Crash Bandicoot.cue"
   ```
5. Wygenerowany kod zostaje lokalnie (`generated/` / `game/`) — **nie commitować**.
6. Podłącz gotowy assembly do hosta Switch (managed) albo później do natywnego loadera.

Disc jest wymagany także w runtime (paging NSF/NSD, audio tracks).

## CI
GitHub Actions buduje tylko hosty (Switch DLL, Vita VPK).  
Recomp gry **nie** działa w CI bez disc użytkownika.
