using System.Runtime.InteropServices;
using static SDL2.SDL;

namespace CrashBandicoot.Switch;

/// <summary>
/// Present przez SDL2 (mono-nx, jak explorer_demo).
/// VRAM PS1 → tekstura streaming → fullscreen.
/// </summary>
public sealed class SwitchGraphics
{
    private IntPtr _window;
    private IntPtr _renderer;
    private IntPtr _texture;
    private int _texW;
    private int _texH;
    private bool _ready;
    private int _framesPresented;
    private byte[]? _rgba;

    public bool Ready => _ready;
    public int FramesPresented => _framesPresented;

    public void Init(int width, int height)
    {
        Console.WriteLine($"[SwitchGraphics] SDL Init target {width}x{height}");

        if (SDL_Init(SDL_INIT_VIDEO | SDL_INIT_JOYSTICK) != 0)
        {
            Console.WriteLine($"[SwitchGraphics] SDL_Init FAIL: {SDL_GetError()}");
            return;
        }

        _window = SDL_CreateWindow(
            "Crash Bandicoot",
            SDL_WINDOWPOS_UNDEFINED,
            SDL_WINDOWPOS_UNDEFINED,
            width,
            height,
            0);

        if (_window == IntPtr.Zero)
        {
            Console.WriteLine($"[SwitchGraphics] SDL_CreateWindow FAIL: {SDL_GetError()}");
            return;
        }

        SDL_SetHint(SDL_HINT_RENDER_SCALE_QUALITY, "nearest");

        // Typ enum — nie uint (SDL2-CS)
        _renderer = SDL_CreateRenderer(
            _window,
            -1,
            SDL_RendererFlags.SDL_RENDERER_ACCELERATED |
            SDL_RendererFlags.SDL_RENDERER_PRESENTVSYNC);

        if (_renderer == IntPtr.Zero)
        {
            _renderer = SDL_CreateRenderer(
                _window,
                -1,
                SDL_RendererFlags.SDL_RENDERER_SOFTWARE);
        }

        if (_renderer == IntPtr.Zero)
        {
            Console.WriteLine($"[SwitchGraphics] SDL_CreateRenderer FAIL: {SDL_GetError()}");
            return;
        }

        _ready = true;
        Console.WriteLine("[SwitchGraphics] SDL window+renderer OK");
    }

    public void BlitFromVram(
        ushort[] vram,
        int vramWidth,
        int vramHeight,
        int dispX,
        int dispY,
        int dispW,
        int dispH,
        bool is24Bit)
    {
        if (!_ready || vram is null || dispW <= 0 || dispH <= 0)
            return;

        dispX = Math.Clamp(dispX, 0, vramWidth - 1);
        dispY = Math.Clamp(dispY, 0, vramHeight - 1);
        dispW = Math.Min(dispW, vramWidth - dispX);
        dispH = Math.Min(dispH, vramHeight - dispY);
        if (dispW <= 0 || dispH <= 0)
            return;

        EnsureTexture(dispW, dispH);
        if (_texture == IntPtr.Zero)
            return;

        var need = dispW * dispH * 4;
        if (_rgba is null || _rgba.Length < need)
            _rgba = new byte[need];

        var dst = _rgba;
        for (int y = 0; y < dispH; y++)
        {
            int srcRow = (dispY + y) * vramWidth + dispX;
            int dstRow = y * dispW * 4;
            for (int x = 0; x < dispW; x++)
            {
                int si = srcRow + x;
                if ((uint)si >= (uint)vram.Length)
                    break;
                ushort p = vram[si];
                int r = (p & 0x1F) << 3;
                int g = ((p >> 5) & 0x1F) << 3;
                int b = ((p >> 10) & 0x1F) << 3;
                int o = dstRow + x * 4;
                dst[o] = (byte)r;
                dst[o + 1] = (byte)g;
                dst[o + 2] = (byte)b;
                dst[o + 3] = 255;
            }
        }

        _ = is24Bit;

        unsafe
        {
            fixed (byte* ptr = dst)
            {
                if (SDL_UpdateTexture(_texture, IntPtr.Zero, (IntPtr)ptr, dispW * 4) != 0)
                {
                    if (_framesPresented < 3)
                        Console.WriteLine($"[SwitchGraphics] UpdateTexture: {SDL_GetError()}");
                    return;
                }
            }
        }

        SDL_SetRenderDrawColor(_renderer, 0, 0, 0, 255);
        SDL_RenderClear(_renderer);
        SDL_RenderCopy(_renderer, _texture, IntPtr.Zero, IntPtr.Zero);
        SDL_RenderPresent(_renderer);

        while (SDL_PollEvent(out _) != 0) { }

        _framesPresented++;
        if (_framesPresented == 1 || _framesPresented % 60 == 0)
            Console.WriteLine($"[SwitchGraphics] SDL present #{_framesPresented} src={dispW}x{dispH}");
    }

    private void EnsureTexture(int w, int h)
    {
        if (_texture != IntPtr.Zero && _texW == w && _texH == h)
            return;

        if (_texture != IntPtr.Zero)
        {
            SDL_DestroyTexture(_texture);
            _texture = IntPtr.Zero;
        }

        _texture = SDL_CreateTexture(
            _renderer,
            SDL_PIXELFORMAT_ABGR8888,
            (int)SDL_TextureAccess.SDL_TEXTUREACCESS_STREAMING,
            w,
            h);

        if (_texture == IntPtr.Zero)
        {
            _texture = SDL_CreateTexture(
                _renderer,
                SDL_PIXELFORMAT_RGBA8888,
                (int)SDL_TextureAccess.SDL_TEXTUREACCESS_STREAMING,
                w,
                h);
        }

        if (_texture == IntPtr.Zero)
        {
            Console.WriteLine($"[SwitchGraphics] CreateTexture FAIL: {SDL_GetError()}");
            return;
        }

        _texW = w;
        _texH = h;
        Console.WriteLine($"[SwitchGraphics] texture {w}x{h}");
    }

    public void Present(ReadOnlySpan<byte> unused) => _ = unused;

    public void LogFrameIfNeeded(int presentCount) { }

    public void Shutdown()
    {
        Console.WriteLine($"[SwitchGraphics] Shutdown (presented={_framesPresented})");
        if (_texture != IntPtr.Zero)
        {
            SDL_DestroyTexture(_texture);
            _texture = IntPtr.Zero;
        }
        if (_renderer != IntPtr.Zero)
        {
            SDL_DestroyRenderer(_renderer);
            _renderer = IntPtr.Zero;
        }
        if (_window != IntPtr.Zero)
        {
            SDL_DestroyWindow(_window);
            _window = IntPtr.Zero;
        }
        if (_ready)
            SDL_Quit();
        _ready = false;
        _rgba = null;
    }
}
