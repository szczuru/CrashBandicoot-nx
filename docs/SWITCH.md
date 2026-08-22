# Nintendo Switch

## Wymagania
- Atmosphere (homebrew)
- [mono-nx](https://github.com/exelix11/mono-nx) interpreter (release `sd_files`)
- Własny dump **Crash Bandicoot NTSC-U SCUS-94900** (`.cue` + `.bin`)

## Układ na SD
```text
sd:/
  mono/                         # z mono-nx
  switch/
    CrashBandicoot.Switch.dll   # artifact z GitHub Actions
  crash/
    Crash Bandicoot.cue
    Crash Bandicoot.bin
```

## Build (CI)
Push do `platform/switch/**` → artifact **switch-managed**.

Lokalnie:
```bash
dotnet build platform/switch/CrashBandicoot.Switch.csproj -c Release
```

## Status
Stub hosta (graphics/audio/input).  
Podłącz `RecompOne.Runtime` + offline recomp (`tools/recomp_offline.sh`) gdy będzie gotowe `game/`.

## Prawne
Nie dystrybuuj `.bin`/`.cue` ani wygenerowanego kodu gry. Tylko tools + host.
