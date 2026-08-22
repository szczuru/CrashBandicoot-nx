namespace CrashBandicoot.Switch;

/// <summary>
/// Soft present: konwersja PS1 VRAM (RGB555 / 24-bit) → RGBA8888.
/// Docelowo: upload do framebuffera libnx / SDL. Na razie bufor + checksum w logu.
/// </summary>
public sealed class SwitchGraphics
{
    private int _width;
    private int _height;
    private byte[]? _rgba;
    private int _lastW;
    private int _lastH;
    private uint _lastChecksum;
    private int _framesConverted;

    public int Width => _width;
    public int Height => _height;
    public ReadOnlySpan<byte> LastRgba => _rgba ?? ReadOnlySpan<byte>.Empty;
    public int FramesConverted => _framesConverted;

    public void Init(int width, int height)
    {
        _width = width;
        _height = height;
        Console.WriteLine($"[SwitchGraphics] Init {width}x{height}");
    }

    /// <summary>
    /// Kopiuje region display z PS1 VRAM (1024×512, ushort RGB555) do lokalnego RGBA.
    /// </summary>
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
        if (vram is null || dispW <= 0 || dispH <= 0)
            return;

        dispX = Math.Clamp(dispX, 0, vramWidth - 1);
        dispY = Math.Clamp(dispY, 0, vramHeight - 1);
        dispW = Math.Min(dispW, vramWidth - dispX);
        dispH = Math.Min(dispH, vramHeight - dispY);
        if (dispW <= 0 || dispH <= 0)
            return;

        var need = dispW * dispH * 4;
        if (_rgba is null || _rgba.Length < need)
            _rgba = new byte[need];

        uint checksum = 2166136261u;
        var dst = _rgba;

        if (!is24Bit)
        {
            // 15-bit: 1 texel = 1 ushort, packed BGR555
            for (int y = 0; y < dispH; y++)
            {
                int srcRow = (dispY + y) * vramWidth + dispX;
                int dstRow = y * dispW * 4;
                for (int x = 0; x < dispW; x++)
                {
                    ushort p = vram[srcRow + x];
                    int r = (p & 0x1F) << 3;
                    int g = ((p >> 5) & 0x1F) << 3;
                    int b = ((p >> 10) & 0x1F) << 3;
                    int o = dstRow + x * 4;
                    dst[o] = (byte)r;
                    dst[o + 1] = (byte)g;
                    dst[o + 2] = (byte)b;
                    dst[o + 3] = 255;
                    checksum ^= (uint)p;
                    checksum *= 16777619u;
                }
            }
        }
        else
        {
            // 24-bit: 2 pixele = 3 ushorty (uproszczenie layoutu PS1)
            for (int y = 0; y < dispH; y++)
            {
                int srcRow = (dispY + y) * vramWidth + dispX;
                int dstRow = y * dispW * 4;
                for (int x = 0; x < dispW; x++)
                {
                    int si = srcRow + x;
                    if (si >= vram.Length)
                        break;
                    ushort p = vram[si];
                    // fallback: traktuj jak 15-bit aż pełny 24-bit path będzie dopięty
                    int r = (p & 0x1F) << 3;
                    int g = ((p >> 5) & 0x1F) << 3;
                    int b = ((p >> 10) & 0x1F) << 3;
                    int o = dstRow + x * 4;
                    dst[o] = (byte)r;
                    dst[o + 1] = (byte)g;
                    dst[o + 2] = (byte)b;
                    dst[o + 3] = 255;
                    checksum ^= (uint)p;
                    checksum *= 16777619u;
                }
            }
        }

        _lastW = dispW;
        _lastH = dispH;
        _lastChecksum = checksum;
        _framesConverted++;

        // TODO: natywny present (libnx framebuffer / SDL texture) z _rgba[_lastW*_lastH*4]
    }

    public void LogFrameIfNeeded(int presentCount)
    {
        if (presentCount % 60 != 0 || _framesConverted == 0)
            return;
        Console.WriteLine(
            $"[SwitchGraphics] softframe #{_framesConverted} {_lastW}x{_lastH} checksum=0x{_lastChecksum:X8} rgbaBytes={_lastW * _lastH * 4}");
    }

    public void Present(ReadOnlySpan<byte> unused)
    {
        _ = unused;
    }

    public void Shutdown()
    {
        Console.WriteLine($"[SwitchGraphics] Shutdown (converted={_framesConverted})");
        _rgba = null;
    }
}
