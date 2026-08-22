#pragma once

class PlatformHost {
public:
  bool Init();
  void RunSmoke(int frames);
  void Shutdown();
};
