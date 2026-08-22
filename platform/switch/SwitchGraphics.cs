using static SDL2.SDL;

namespace CrashBandicoot.Switch;

/// <summary>
/// Present SDL2: poprawne kolory PS1 (LUT RGB555→RGBA) + tańszy present.
/// </summary>
public sealed class SwitchGraphics
{
    // PS1: R=bit0-4, G=5-9, B=10-14 → packed RGBA8888 (byte order pod ABGR8888 LE)
    private static readonly uint[] Rgb555ToRgba = CreateLut();

    private IntPtr _window;
    private IntPtr _renderer;
    private IntPtr _texture;
    private int _texW;
    private int _texH;
    private bool _ready;
    private int _framesPresented;
    private int _blitCalls;
    private uint[]? _rgba32; // packed pixels

    /// <summary>Present na ekran co N wywołań Blit (2 = co druga klatka gry).</summary>
    public int PresentEvery { get; set; } = 2;

    public bool Ready => _ready;
    public int FramesPresented => _framesPresented;

    private static uint[] CreateLut()
    {
        var lut = new uint[65536];
        for (int p = 0; p < 65536; p++)
        {
            int r = (p & 0x1F) << 3;
            int g = ((p >> 5) & 0x1F) << 3;
            int b = ((p >> 10) & 0x1F) << 3;
            // rozciągnij 5→8 bit (opcjonalnie)
            r |= r >> 5;
            g |= g >> 5;
            b |= b >> 5;
            // bajty w pamięci: R,G,B,A → na LE często SDL_PIXELFORMAT_ABGR8888
            lut[p] = (uint)(r | (g << 8) | (b << 16) | (0xFF << 24));
        }
        return lut;
    }

    public void Init(int width, int height)
    {
        // Mniejsze okno = mniej pracy GPU przy scale
        if (width > 960) width = 960;
        if (height > 540) height = 540;

        Console.WriteLine($"[SwitchGraphics] SDL Init {width}x{height}, PresentEvery={PresentEvery}");

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
            Console.WriteLine($"[SwitchGraphics] CreateWindow FAIL: {SDL_GetError()}");
            return;
        }

        SDL_SetHint(SDL_HINT_RENDER_SCALE_QUALITY, "nearest");

        _renderer = SDL_CreateRenderer(
            _window,
            -1,
            SDL_RendererFlags.SDL_RENDERER_ACCELERATED);

        if (_renderer == IntPtr.Zero)
        {
            _renderer = SDL_CreateRenderer(
                _window,
                -1,
                SDL_RendererFlags.SDL_RENDERER_SOFTWARE);
        }

        if (_renderer == IntPtr.Zero)
        {
            Console.WriteLine($"[SwitchGraphics] CreateRenderer FAIL: {SDL_GetError()}");
            return;
        }

        _ready = true;
        Console.WriteLine("[SwitchGraphics] SDL OK");
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

        _blitCalls++;
        // Pomiń drogi SDL present — gra i tak woła Present co klatkę
        if (PresentEvery > 1 && (_blitCalls % PresentEvery) != 0)
            return;

        dispX = Math.Clamp(dispX, 0, vramWidth - 1);
        dispY = Math.Clamp(dispY, 0, vramHeight - 1);
        dispW = Math.Min(dispW, vramWidth - dispX);
        dispH = Math.Min(dispH, vramHeight - dispY);
        if (dispW <= 0 || dispH <= 0)
            return;

        EnsureTexture(dispW, dispH);
        if (_texture == IntPtr.Zero || _rgba32 is null)
            return;

        var lut = Rgb555ToRgba;
        var dst = _rgba32;
        int di = 0;

        for (int y = 0; y < dispH; y++)
        {
            int src = (dispY + y) * vramWidth + dispX;
            for (int x = 0; x < dispW; x++)
                dst[di++] = lut[vram[src + x] & 0x7FFF];
        }

        unsafe
        {
            fixed (uint* p = dst)
            {
                if (SDL_UpdateTexture(_texture, IntPtr.Zero, (IntPtr)p, dispW * 4) != 0)
                {
                    if (_framesPresented < 3)
                        Console.WriteLine($"[SwitchGraphics] UpdateTexture: {SDL_GetError()}");
                    return;
                }
            }
        }

        SDL_RenderClear(_renderer);
        SDL_RenderCopy(_renderer, _texture, IntPtr.Zero, IntPtr.Zero);
        SDL_RenderPresent(_renderer);

        _framesPresented++;
        if (_framesPresented == 1 || _framesPresented % 120 == 0)
            Console.WriteLine($"[SwitchGraphics] present #{_framesPresented} {dispW}x{dispH}");

        _ = is24Bit;
        _ = vramHeight;
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
        _rgba32 = new uint[w * h];
        Console.WriteLine($"[SwitchGraphics] texture RGBA {w}x{h}");
    }

    public void Present(ReadOnlySpan<byte> unused) => _ = unused;

    public void LogFrameIfNeeded(int presentCount) { }

    public void Shutdown()
    {
        Console.WriteLine($"[SwitchGraphics] Shutdown (presented={_framesPresented})");
        if (_texture != IntPtr.Zero) { SDL_DestroyTexture(_texture); _texture = IntPtr.Zero; }
        if (_renderer != IntPtr.Zero) { SDL_DestroyRenderer(_renderer); _renderer = IntPtr.Zero; }
        if (_window != IntPtr.Zero) { SDL_DestroyWindow(_window); _window = IntPtr.Zero; }
        if (_ready) SDL_Quit();
        _ready = false;
        _rgba32 = null;
    }
}
