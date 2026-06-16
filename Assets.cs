namespace DevineClairvoyance;

/// <summary>
/// Loads images and the Harrington font from the Assets folder that ships
/// alongside the executable, replacing the old embedded My.Resources lookups.
/// Images are cached so the same bitmap is reused across forms.
/// </summary>
public static class Assets
{
    private static readonly string Dir = Path.Combine(AppContext.BaseDirectory, "Assets");
    private static readonly Dictionary<string, Image> _cache = new(StringComparer.OrdinalIgnoreCase);

    /// <summary>Load "{name}.png" from Assets (e.g. a card name or art asset).</summary>
    public static Image Image(string name)
    {
        if (_cache.TryGetValue(name, out var img))
            return img;

        var path = Path.Combine(Dir, name + ".png");
        // Read into memory so the file isn't locked and the bitmap survives caching.
        using var fs = File.OpenRead(path);
        var loaded = System.Drawing.Image.FromStream(fs);
        _cache[name] = loaded;
        return loaded;
    }

    public static Image Background => Image("tarot sheet");
    public static Image CardStack => Image("Card Stack");
    public static Image CurrentSituation => Image("3 Card Current Situation");
    public static Image Challenge => Image("3 Card Challenge");
    public static Image Advice => Image("3 Card Advice");

    public static readonly Icon AppIcon = LoadIcon();

    private static Icon LoadIcon()
    {
        var path = Path.Combine(Dir, "Icon1.ico");
        return File.Exists(path) ? new Icon(path) : SystemIcons.Application;
    }
}
