namespace RecompOne.Runtime.Catalog;

/// <summary>
/// Pusty katalog tekstur pod SWITCH_SLIM (bez StbImage / mod texture packs).
/// </summary>
public sealed class TexturesCatalog
{
    public static TexturesCatalog Empty { get; } = new();

    public void Clear() { }

    public bool TryGet(object? key, out object? value)
    {
        value = null;
        return false;
    }
}
