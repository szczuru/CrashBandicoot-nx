namespace CrashBandicoot.Switch;

public sealed class SwitchInput
{
    /// <summary>
    /// Bitmaska w stylu PS1 digital pad (na razie 0 — brak natywnego padu).
    /// </summary>
    public ushort ReadPad0() => 0;
}
