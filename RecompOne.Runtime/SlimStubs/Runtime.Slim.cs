using RecompOne.Runtime.Context;
using RecompOne.Runtime.Events;
using RecompOne.Runtime.Host;
using RecompOne.Runtime.Memory;

namespace RecompOne.Runtime;

public enum RunMode { Retail, Devkit }

public static class Runtime
{
    static IRuntimePlatformHost? _platformHost;

    public static CpuContext? Cpu { get; private set; }
    public static IMemory? Mem { get; private set; }
    public static Gpu? Gpu;
    public static Spu? Spu;
    public static Cdrom.CdController? Cd;

    public static RunMode Mode { get; private set; } = RunMode.Retail;
    public static void SetMode(RunMode mode) => Mode = mode;

    public static string CdPath => Config.ConfigManager.Game.CdPath;

    public static void SetPlatformHost(IRuntimePlatformHost? host) => _platformHost = host;

    public static Config.ViewConfig View => Config.ConfigManager.View;
    public static void SaveView() { }

    public static Hardware.MemoryCard CardA = new(AppPaths.CardAPath) { Enabled = true };
    public static Hardware.MemoryCard CardB = new(AppPaths.CardBPath) { Enabled = true };
    public static readonly Memory.RamLogger RamLog = new();
    public static readonly Dispatch.OverlayEventLog OverlayLog = new();

    public static void SetEmbedParent(nint hwnd) { }
    public static void FitEmbeddedWindow() { }
    public static void SetHostFullscreenHandler(Action<bool>? handler) { }
    public static void SetFullscreen(bool on) { }
    public static void RequestFullscreenToggle() { }
    public static void RequestCheatMenuToggle() { }
    public static void RequestPauseMenuToggle() { }

    public static void Initialize(string title)
    {
        if (_platformHost == null)
            throw new InvalidOperationException("SWITCH_SLIM requires Runtime.SetPlatformHost(...) before Initialize.");

        _platformHost.Initialize(title);
        _platformHost.SetMasterVolume(
            Config.ConfigManager.Game.Muted ? 0f : Config.ConfigManager.Game.MasterVolume);

        if (Event.HasAnyListeners<RuntimeReadyEvent>())
            Event.Dispatch(new RuntimeReadyEvent());
    }

    public static void WaitForValidDisc()
    {
        _platformHost?.WaitForValidDisc();
    }

    public static void ShowNotice(string message)
    {
        _platformHost?.ShowNotice(message);
    }

    public static void SetStartupNotice(string message, string title = "Notice", string ackKey = "StartupNoticeAck")
    {
        _platformHost?.ShowNotice($"{title}: {message}");
    }

    public static void SetContext(CpuContext c, IMemory m)
    {
        Cpu = c;
        Mem = m;
    }

    public static void PresentFrame()
    {
        if (_platformHost == null) return;

        _platformHost.Present(Gpu);
        _platformHost.AttachAudio(Spu);

        FrameClock.Throttle();
        Sdk.LibCd.Tick();
        if (Mem != null)
        {
            Bios.BiosB.RefreshPad(Mem);
            Sdk.LibPad.Refresh(Mem);
        }
        Host.FramePacing.OnHostPresent(Mem);
        Host.FramePacing.PulseVblankIrq();
    }

    public static void DispatchIrq(int irq)
    {
        if (Cpu != null && Mem != null)
            Interrupts.Deliver(irq, Cpu, Mem);
    }

    public static void Shutdown()
    {
        _platformHost?.Shutdown();
    }
}
