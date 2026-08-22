# Jak wgrać te pliki do forka

1. Zforkuj https://github.com/Matteo842/CrashBandicoot-Launcher
2. Sklonuj swój fork lokalnie.
3. Skopiuj **zawartość** tego katalogu `crash-consoles-starter/` do roota forka  
   (nadpisz / dodaj — nie kasuj `RecompOne.*` z upstreamu).
4. Scal `.gitignore` z istniejącym (dopisz sekcje z tej paczki).
5. `chmod +x tools/recomp_offline.sh`
6. Commit + push:
   ```bash
   git add platform .github docs tools .gitignore
   git commit -m "Add Switch (managed) and Vita (C++) host stubs + CI"
   git push
   ```
7. W GitHub → Actions sprawdź workflowy **Switch** i **Vita**.

## Kolejność pracy
1. Switch smoke DLL na mono-nx  
2. Input → software present → audio  
3. Podpięcie Runtime / Entry  
4. Vita: memory + CD + software present → pełny HLE  

Nie commituj: `*.bin`, `*.cue`, `game/`, `generated/`.
