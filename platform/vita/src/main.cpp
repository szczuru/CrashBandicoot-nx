#include <stdio.h>
#include <string.h>
#include <psp2/kernel/processmgr.h>
#include <psp2/io/fcntl.h>
#include <psp2/ctrl.h>

#include "platform_host.h"

// Użytkownik kładzie legalny dump tutaj (nie commituj .bin/.cue)
static const char *kDefaultCue = "ux0:data/crash/Crash Bandicoot.cue";

int main(int argc, char *argv[]) {
  (void)argc;
  (void)argv;

  printf("[Vita] Crash Bandicoot recomp host stub\n");

  PlatformHost host;
  if (!host.Init()) {
    printf("[Vita] Host init failed\n");
    sceKernelDelayThread(3 * 1000 * 1000);
    return 1;
  }

  const char *cue = kDefaultCue;
  SceUID fd = sceIoOpen(cue, SCE_O_RDONLY, 0);
  if (fd < 0) {
    printf("[Vita] Brak disc: %s\n", cue);
    printf("[Vita] Poloz wlasny NTSC-U SCUS-94900 (.cue+.bin)\n");
  } else {
    printf("[Vita] Znaleziono cue: %s\n", cue);
    sceIoClose(fd);
  }

  host.RunSmoke(120);

  host.Shutdown();
  printf("[Vita] Exit\n");
  sceKernelDelayThread(1 * 1000 * 1000);
  return 0;
}
