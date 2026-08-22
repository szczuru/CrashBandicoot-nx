namespace CrashBandicoot.Switch;

/// <summary>
/// Mapowanie Joy-Con / Pro → PS1 pad halfword (jak BiosB.PadRead w RecompOne).
/// Crash 1: Cross (confirm) w low halfword m.in. 0x0040.
/// </summary>
public sealed class SwitchInput
{
    /// <summary>Stan pada 0 jako ushort (bitmask PS1-style).</summary>
    public ushort ReadPad0()
    {
        // TODO: padUpdate (libnx) lub SDL_GameController
        // Mapowanie przykładowe (do uzupełnienia):
        // A (Switch) → Cross (PS1)
        // B → Circle
        // X → Triangle
        // Y → Square
        // Plus → Start, Minus → Select
        // D-pad / lewy analog → kierunki
        return 0;
    }
}
