using RecompOne.Runtime;

namespace CrashBandicoot.Switch;

/// <summary>
/// IRuntimePlatformHost bez Silk/GLFW/OpenAL.
/// Present: soft blit z Gpu.Vram.
/// </summary>
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
        _graphics.Init(1280, 720);
        _audio.Init(44100, 2);
        _alive = true;
        _presentCount = 0;
    }

    public void WaitForValidDisc()
    {
        Console.WriteLine("[SwitchHost] WaitForValidDisc (no-op)");
    }

    public void Present(Gpu? gpu)
    {
        _presentCount++;
        _ = _input.ReadPad0();

        if (gpu is null || !gpu.DisplayEnabled)
        {
            if (_presentCount <= 5 || _presentCount % 120 == 0)
                Console.WriteLine($"[SwitchHost] Present #{_presentCount} (gpu null or display off)");
            Thread.Sleep(1);
            return;
        }

        try
        {
            var vram = gpu.Vram;
            int vw = Gpu.VramWidth;
            int vh = Gpu.VramHeight;
            _graphics.BlitFromVram(
                vram,
                vw,
                vh,
                gpu.DisplayX,
                gpu.DisplayY,
                gpu.DisplayWidth,
                gpu.DisplayHeight,
                gpu.Display24Bit);
            _graphics.LogFrameIfNeeded(_presentCount);
        }
        catch (Exception ex)
        {
            if (_presentCount <= 3 || _presentCount % 120 == 0)
                Console.WriteLine($"[SwitchHost] Blit error: {ex.Message}");
        }

        if (_presentCount % 60 == 0)
        {
            Console.WriteLine(
                $"[SwitchHost] Present #{_presentCount} display={gpu.DisplayWidth}x{gpu.DisplayHeight} " +
                $"at ({gpu.DisplayX},{gpu.DisplayY}) 24bit={gpu.Display24Bit}");
        }

        Thread.Sleep(1);
    }

    public void AttachAudio(Spu? spu)
    {
        _ = spu;
    }

    public void SetMasterVolume(float volume)
    {
        Console.WriteLine($"[SwitchHost] SetMasterVolume {volume:0.###}");
        _audio.SetVolume(volume);
    }

    public void ShowNotice(string message)
    {
        Console.WriteLine($"[SwitchHost] Notice: {message}");
    }

    public void Shutdown()
    {
        if (!_alive)
            return;
        Console.WriteLine($"[SwitchHost] Shutdown after {_presentCount} presents");
        _alive = false;
        _audio.Shutdown();
        _graphics.Shutdown();
    }

    public void Dispose() => Shutdown();
}
