using RecompOne.Runtime;

namespace CrashBandicoot.Switch;

public sealed class SwitchPlatformHost : IRuntimePlatformHost, IDisposable
{
    private readonly SwitchGraphics _graphics = new();
    private readonly SwitchAudio _audio = new();
    private readonly SwitchInput _input = new();
    private bool _alive;
    private int _presentCount;

    public SwitchGraphics Graphics => _graphics;
    public SwitchAudio Audio => _audio;
    public SwitchInput Input => _input;

    public void Initialize(string title)
    {
        Console.WriteLine($"[SwitchHost] Initialize: {title}");
        _graphics.PresentEvery = 2; // 3 = jeszcze mniej SDL, bardziej „film”
        _graphics.Init(960, 540);
        _audio.Init(44100, 2);
        _alive = true;
        _presentCount = 0;
    }

    public void WaitForValidDisc() =>
        Console.WriteLine("[SwitchHost] WaitForValidDisc (no-op)");

    public void Present(Gpu? gpu)
    {
        _presentCount++;
        _ = _input.ReadPad0();

        if (gpu is null || !gpu.DisplayEnabled)
            return;

        try
        {
            _graphics.BlitFromVram(
                gpu.Vram,
                Gpu.VramWidth,
                Gpu.VramHeight,
                gpu.DisplayX,
                gpu.DisplayY,
                gpu.DisplayWidth,
                gpu.DisplayHeight,
                gpu.Display24Bit);
        }
        catch (Exception ex)
        {
            if (_presentCount <= 3)
                Console.WriteLine($"[SwitchHost] Blit error: {ex.Message}");
        }

        // BEZ Thread.Sleep — timing robi Runtime FrameClock / SDL
    }

    public void AttachAudio(Spu? spu) => _ = spu;

    public void SetMasterVolume(float volume)
    {
        Console.WriteLine($"[SwitchHost] SetMasterVolume {volume:0.###}");
        _audio.SetVolume(volume);
    }

    public void ShowNotice(string message) =>
        Console.WriteLine($"[SwitchHost] Notice: {message}");

    public void Shutdown()
    {
        if (!_alive) return;
        Console.WriteLine($"[SwitchHost] Shutdown after {_presentCount} presents");
        _alive = false;
        _audio.Shutdown();
        _graphics.Shutdown();
    }

    public void Dispose() => Shutdown();
}
