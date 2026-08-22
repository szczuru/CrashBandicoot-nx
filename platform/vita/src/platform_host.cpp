#include "platform_host.h"
#include <stdio.h>
#include <psp2/kernel/threadmgr.h>
#include <psp2/ctrl.h>
#include <psp2/display.h>

bool PlatformHost::Init() {
  printf("[VitaHost] Init\n");
  sceCtrlSetSamplingMode(SCE_CTRL_MODE_ANALOG);
  return true;
}

void PlatformHost::RunSmoke(int frames) {
  SceCtrlData pad;
  for (int i = 0; i < frames; ++i) {
    sceCtrlPeekBufferPositive(0, &pad, 1);
    if (i % 30 == 0)
      printf("[VitaHost] frame %d buttons=0x%08x\n", i, pad.buttons);
    sceKernelDelayThread(16 * 1000); // ~60 Hz
  }
}

void PlatformHost::Shutdown() {
  printf("[VitaHost] Shutdown\n");
}
