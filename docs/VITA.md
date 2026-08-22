# PS Vita

## Wymagania
- HENkaku / h-encore / Trinity
- VitaShell
- Własny dump NTSC-U w `ux0:data/crash/`

## Układ
```text
ux0:data/crash/
  Crash Bandicoot.cue
  Crash Bandicoot.bin
```

## Build (CI)
Workflow **Vita** → Docker `vitasdk/vitasdk` → artifact `.vpk` / `eboot.bin`.

Lokalnie:
```bash
export VITASDK=/usr/local/vitasdk   # lub ścieżka z vdpm
cmake -S platform/vita -B platform/vita/build \
  -DCMAKE_TOOLCHAIN_FILE=$VITASDK/share/vita.toolchain.cmake \
  -DCMAKE_BUILD_TYPE=Release
cmake --build platform/vita/build
```

Zainstaluj VPK przez VitaShell.  
**Zmień `VITA_TITLEID` w CMakeLists.txt** na unikalny (9 znaków).

## Status
C++ stub (pad + smoke). GPU / SPU / CD / memory — do portu z RecompOne.Runtime (wydajniej natywnie niż Mono na Vicie).

## Prawne
Nie dystrybuuj disc image ani retail assets.
