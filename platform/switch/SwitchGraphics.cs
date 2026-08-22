using static SDL2.SDL;

namespace CrashBandicoot.Switch;

/// <summary>
/// Szybki present: VRAM RGB555 → SDL texture (bez konwersji do RGBA).
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
    private ushort[]? _tight; // gęsty bufor dispW*dispH (pitch = width)

    public bool Ready => _ready;
    public int FramesPresented => _framesPresented;

    public void Init(int width, int height)
    {
        Console.WriteLine($"[SwitchGraphics] SDL Init {width}x{height}");

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

        // nearest = tańsze skalowanie 512x240 → 1280x720
        SDL_SetHint(SDL_HINT_RENDER_SCALE_QUALITY, "nearest");

        // Najpierw accelerated BEZ vsync — limituje FrameClock / gra, nie SDL
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
        Console.WriteLine("[SwitchGraphics] SDL OK (no vsync flag)");
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
        if (_texture == IntPtr.Zero || _tight is null)
            return;

        // Skopiuj tylko region display do gęstego bufora (pitch = dispW)
        // Zamiast konwersji RGBA — zostaje RGB555
        var tight = _tight;
        int needed = dispW * dispH;
        if (tight.Length < needed)
        {
            _tight = tight = new ushort[needed];
        }

        for (int y = 0; y < dispH; y++)
        {
            int src = (dispY + y) * vramWidth + dispX;
            int dst = y * dispW;
            Array.Copy(vram, src, tight, dst, dispW);
        }

        unsafe
        {
            fixed (ushort* p = tight)
            {
                // pitch w bajtach = dispW * 2
                if (SDL_UpdateTexture(_texture, IntPtr.Zero, (IntPtr)p, dispW * 2) != 0)
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

        // Max kilka eventów, bez pełnego drenażu co klatkę
        for (int i = 0; i < 4 && SDL_PollEvent(out _) != 0; i++) { }

        _framesPresented++;
        if (_framesPresented == 1 || _framesPresented % 300 == 0)
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

        // 15-bit — bez konwersji CPU do 32-bit
        // Jeśli kolory złe: spróbuj SDL_PIXELFORMAT_BGR555 / ARGB1555 / RGBA5551
        _texture = SDL_CreateTexture(
            _renderer,
            SDL_PIXELFORMAT_RGB555,
            (int)SDL_TextureAccess.SDL_TEXTUREACCESS_STREAMING,
            w,
            h);

        if (_texture == IntPtr.Zero)
        {
            _texture = SDL_CreateTexture(
                _renderer,
                SDL_PIXELFORMAT_BGR555,
                (int)SDL_TextureAccess.SDL_TEXTUREACCESS_STREAMING,
                w,
                h);
        }

        if (_texture == IntPtr.Zero)
        {
            Console.WriteLine($"[SwitchGraphics] CreateTexture 555 FAIL: {SDL_GetError()}");
            return;
        }

        _texW = w;
        _texH = h;
        _tight = new ushort[w * h];
        Console.WriteLine($"[SwitchGraphics] texture RGB555 {w}x{h}");
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
        _tight = null;
    }
}
