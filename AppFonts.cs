using System.Drawing.Text;

namespace DevineClairvoyance;

/// <summary>
/// Supplies the app's decorative script font with graceful fallback. The original
/// VB app hard-coded "Segoe Print"; on a machine without it (or with different
/// system text settings) that broke layouts, so we resolve to the best available
/// script-style family once and reuse it. Point sizes are kept — WinForms scales
/// them with the system DPI/font setting.
/// </summary>
public static class AppFonts
{
    private static readonly FontFamily Family = ResolveScriptFamily();

    // Optional uniform font-size multiplier (set DEVCLAIR_FONTSCALE) used to
    // sanity-check layouts against larger system text settings. Defaults to 1.
    private static readonly float Scale = ReadScale();

    private static float ReadScale()
    {
        var v = Environment.GetEnvironmentVariable("DEVCLAIR_FONTSCALE");
        return float.TryParse(v, out var s) && s > 0 ? s : 1f;
    }

    private static FontFamily ResolveScriptFamily()
    {
        string[] preferred = { "Segoe Print", "Segoe Script", "Ink Free", "Comic Sans MS" };
        using var installed = new InstalledFontCollection();
        var available = installed.Families
            .Select(f => f.Name)
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        foreach (var name in preferred)
            if (available.Contains(name))
                return new FontFamily(name);

        return FontFamily.GenericSansSerif;
    }

    public static Font Script(float size, FontStyle style = FontStyle.Regular) =>
        new(Family, size * Scale, style);
}
