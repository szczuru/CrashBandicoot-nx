using RecompOne.Runtime;
using RecompOne.Runtime.Hle;

namespace CrashBandicoot.Switch;

/// <summary>
/// Host bez Silk/GLFW/OpenAL — wymagany na mono-nx / Switch.
/// Runtime.SetPlatformHost(this) PRZED Entry.Run.
/// </summary>
public sealed class SwitchPlatformHost : IRuntimePlatformHost, IDisposable
{
    private readonly SwitchGraphics _graphics = new();
    private readonly SwitchAudio _audio = new();
    private readonly SwitchInput _input = new();
    private bool _alive;
    private int _presentCount;
    private long _lastLogPresent;

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
        _lastLogPresent = 0;
    }

    public void WaitForValidDisc()
    {
        Console.WriteLine("[SwitchHost] WaitForValidDisc (no-op)");
    }

    public void Present(Gpu? gpu)
    {
        _presentCount++;

        // Odśwież pad (Runtime i tak woła Bios pad; tu opcjonalnie virtual)
        try
        {
            var buttons = _input.ReadPad0();
            // Hardware.Controller.SetVirtualPadState — jeśli dostępne w Twojej wersji Runtime
            // RecompOne.Runtime.Hardware.Controller.SetVirtualPadState(buttons);
            _ = buttons;
        }
        catch
        {
            // ignore
        }

        if (gpu != null && gpu.DisplayEnabled)
        {
            // Na razie tylko log co ~60 presentów — bez prawdziwego blit
            if (_presentCount - _lastLogPresent >= 60)
            {
                _lastLogPresent = _presentCount;
                Console.WriteLine(
                    $"[SwitchHost] Present #{_presentCount} display={gpu.DisplayWidth}x{gpu.DisplayHeight} " +
                    $"at ({gpu.DisplayX},{gpu.DisplayY}) 24bit={gpu.Display24Bit}");
            }

            _graphics.Present(ReadOnlySpan<byte>.Empty);
        }
        else if (_presentCount <= 5 || _presentCount % 120 == 0)
        {
            Console.WriteLine($"[SwitchHost] Present #{_presentCount} (gpu null or display off)");
        }

        // Lekki throttle, żeby nie zjeść 100% CPU na Switchu
        Thread.Sleep(1);
    }

    public void AttachAudio(Spu? spu)
    {
        if (spu is null)
            return;
        // TODO: SPU → PCM → native audio
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
