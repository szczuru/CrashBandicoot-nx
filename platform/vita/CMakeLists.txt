cmake_minimum_required(VERSION 3.16)
project(CrashBandicootVita C CXX)

set(CMAKE_C_STANDARD 11)
set(CMAKE_CXX_STANDARD 17)
set(CMAKE_CXX_STANDARD_REQUIRED ON)

if(NOT DEFINED ENV{VITASDK})
  message(FATAL_ERROR "VITASDK is not set")
endif()

include("$ENV{VITASDK}/share/vita.cmake" REQUIRED)

set(VITA_APP_NAME "Crash Bandicoot Recomp")
set(VITA_TITLEID  "CRASH0001")  # zmień na unikalny 9 znaków (A-Z0-9)
set(VITA_VERSION  "01.00")

add_executable(${PROJECT_NAME}
  src/main.cpp
  src/platform_host.cpp
  src/gpu_hle.cpp
  src/spu_audio.cpp
  src/cdrom.cpp
  src/input.cpp
  src/memory.cpp
)

target_include_directories(${PROJECT_NAME} PRIVATE
  ${CMAKE_CURRENT_SOURCE_DIR}/include
)

target_link_libraries(${PROJECT_NAME}
  SceDisplay_stub
  SceCtrl_stub
  SceAudio_stub
  SceTouch_stub
  SceSysmodule_stub
  SceCommonDialog_stub
  SceAppUtil_stub
  ScePower_stub
  m
  c
)

vita_create_self(eboot.bin ${PROJECT_NAME} UNSAFE)

vita_create_vpk(${PROJECT_NAME}.vpk ${VITA_TITLEID} eboot.bin
  VERSION ${VITA_VERSION}
  NAME ${VITA_APP_NAME}
)
