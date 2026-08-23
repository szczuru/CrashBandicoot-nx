using System.Text.Json;

namespace RecompOne.Runtime.Catalogs;

/// <summary>SWITCH_SLIM: pusty katalog tekstur (bez PNG / ModLoader).</summary>
public sealed class TexturesCatalog
{
    public static TexturesCatalog Empty { get; } = new();

    public int Count => 0;

    public static TexturesCatalog FromJson(JsonDocument? doc) => Empty;

    internal bool TryResolve(
        int x, int y, int w, int h, ushort[]? pixels, int count, out TextureInfo info)
    {
        info = null!;
        return false;
    }

    public bool TryGet(string id, out TextureInfo info)
    {
        info = null!;
        return false;
    }

    public void Replace(string id, ReadOnlySpan<byte> pngBytes) { }

    public void Replace(string id, string pngPath) { }

    public int ReplaceMany(IEnumerable<(string id, string pngPath)> entries) => 0;

    public int ReplaceMany(IEnumerable<(string id, byte[] pngBytes)> entries) => 0;

    public int ReplaceDirectory(string directory) => 0;

    public void ClearReplace(string id) { }

    public void ClearAllReplaces() { }

    public bool ReloadModAssets() => false;
}
